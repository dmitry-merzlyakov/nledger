<#
.SYNOPSIS
Getting NLedger Python extension ready to work

.DESCRIPTION
This script manages NLedger Python connection settings and dependent external software. Basic responsibilities are:
- Validating local Python deployment to check compliance with NLedger requirements
- Optional installing and configuring embedded Python deployment (any versions; Windows only)
- Producing NLedger Python Extension settings file that allows NLedger to connect and use Python 
- Enabling Python Extension in NLedger configuration settings
- Optional installing NLedger Python module to Python environment that allows to use NLedger capabilities in Python session

The provided commands allow you configure the connection either automatically or with any needed granularity. The commands are:
Informational functions:
- discover - Find Python deployments on the local machine
- status - Show NLedger Python integration status
Connection management function:
- connect - Create a connection file that references to a Python deployment
- disconnect - Remove the connection file
- enable - Enable Python extension in NLedger settings (creates connection settings if they were not created yet)
- disable - Disable Python extension in NLedger settings
Technical functions for advanced configuring:
- install-python - Install an isolated embedded Python (Windows only)
- uninstall-python - Uninstall embedded Python
- install-wheel - Install Wheel module (required to build Ledger package)
- uninstall-wheel - Uninstall Wheel module
- install-pythonnet - Install PythonNet module
- uninstall-pythonnet - Uninstall PythonNet module
- install-ledger - Install Ledger module (and PythonNet if it has not been installed yet)
- uninstall-ledger - Uninstall Ledger module
- test-ledger - Run Ledger module tests (ledger_tests.py)
- test-connection - Return NLedger Python connection settings in a technical format

Hint: in most cases, you only need to execute 'enable' command to configure and enable Python integration.
The commands can be executed by running the script with '-command' parameter or in console mode by typing a command name.

.PARAMETER command
Command to execute. You can get detail information for every command by typing 'get-help [command]' in the console

.PARAMETER path
Optional path to Python executable file

.PARAMETER version
Optional version of dependent software

Note: use 'set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process' to run the script in dev terminal

#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][ValidateSet("discover","status","connect","disconnect","enable","disable","test-connection","install-python","install-wheel","install-pythonnet","install-ledger","uninstall-python","uninstall-wheel","uninstall-pythonnet","uninstall-ledger","test-ledger")]$command,
    [Parameter(Mandatory=$False)][string]$path,
    [Parameter(Mandatory=$False)][string]$version,
    [Switch][bool]$noAnsiColor = $False

)

trap 
{
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/PyManagement.psm1 -Force
Import-Module $Script:ScriptPath/../NLManagement/NLSetup.psm1 -Force
Import-Module $Script:ScriptPath/../NLManagement/NLCommon.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

if ($noAnsiColor) {$Global:ANSI_Colorization=$False}

## Default settings

$appPrefix = "NLedger"
$pyPlatform = "amd64"
$pyWheelModule = "wheel"
$pyNetModule = "PythonNet"
$pyLedgerModule = "Ledger"
$pyEmbedVersion = "3.8.1"

[string]$Script:localAppData = [Environment]::GetFolderPath("LocalApplicationData")
[string]$Script:settingsFileName = [System.IO.Path]::GetFullPath($(Join-Path $Script:localAppData "NLedger/NLedger.Extensibility.Python.settings.xml"))

### NLedger Python Settings ###

function Get-PythonIntegrationSettings {
    [CmdletBinding()]
    Param()

    if (!(Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf)) {
        Write-Verbose "File $Script:settingsFileName does not exist; no settings."
        return $null
    }

    [xml]$Private:xmlContent = Get-Content $Script:settingsFileName
    $Private:settings = [PSCustomObject]@{
        PyExecutable = $Private:xmlContent.'nledger-python-settings'.'py-executable'
        PyDll = $Private:xmlContent.'nledger-python-settings'.'py-dll'
    }
    Write-Verbose "Read settings: $Private:settings"
    return $Private:settings
}

function Set-PythonIntegrationSettings {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$true)]$settings)

    [xml]$Private:doc = New-Object System.Xml.XmlDocument
    $null = $Private:doc.AppendChild($Private:doc.CreateXmlDeclaration("1.0","UTF-8",$null))
    $null = $Private:doc.AppendChild($Private:doc.CreateComment("NLedger Python Integration Settings"))
    $Private:root = $Private:doc.CreateElement("nledger-python-settings")

    $Private:elm = $Private:doc.CreateElement("py-executable")
    $Private:elm.InnerText = $settings.PyExecutable
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-dll")
    $Private:elm.InnerText = $settings.PyDll
    $null = $Private:root.AppendChild($Private:elm)

    $null = $Private:doc.AppendChild($Private:root)

    $Private:folderName = [System.IO.Path]::GetDirectoryName($Script:settingsFileName)
    if (!(Test-Path -LiteralPath $Private:folderName -PathType Container)) {
        Write-Verbose "Creating a folder for settings file: $Private:folderName"
        $null = New-Item -ItemType Directory -Force -Path $Private:folderName
        if (!(Test-Path -LiteralPath $Private:folderName -PathType Container)) { throw "Cannot create folder '$Private:folderName'" }
    }

    $Private:doc.Save($Script:settingsFileName)
    Write-Verbose "Settings $settings are saved to file $Script:settingsFileName"

}

function Remove-PythonIntegrationSettings {
    [CmdletBinding()]
    Param()

    if (Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf) {
        Write-Verbose "Removing NLedger Python integration settings: $Script:settingsFileName"
        $null = Remove-Item -LiteralPath $Script:settingsFileName
        if (Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf) { throw "Cannot delete file $Script:settingsFileName" }
    }
}

function Update-PythonEnvironmentSettings {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pyExecutable,
        [Parameter(Mandatory=$True)][string]$pyDll
    )

    $Private:settings = Get-PythonIntegrationSettings
    if(!$Private:settings) {
        $Private:settings = [PSCustomObject]@{
            PyExecutable = $pyExecutable
            PyDll = $pyDll
        }    
    } else {
        $Private:settings.PyExecutable = $pyExecutable
        $Private:settings.PyDll = $pyDll
    }

    $null = Set-PythonIntegrationSettings $Private:settings
    return $Private:settings
}

### Functions ###

function Get-PyInfo {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    Write-Verbose "Getting Python deployment information located by path $pyExecutable"

    $Private:info = @{}

    if (Test-Path -LiteralPath $pyExecutable -PathType Leaf) {
        $Private:info["pyExecutable"] = [System.IO.Path]::GetFullPath($pyExecutable)
        $Private:pyVersion = Get-PyExpandedVersion -pyExecutable $pyExecutable
        if ($Private:pyVersion) {
            $Private:info["pyVersion"] = $Private:pyVersion
            $Private:info["pyPlatform"] = Get-PyPlatform -pyExecutable $pyExecutable
            $Private:pipVersion = Get-PipVersion -pyExecutable $pyExecutable
            if ($Private:pipVersion) {
                $Private:info["pipVersion"] = $Private:pipVersion
                $Private:info["pyWheelInfo"] = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyWheelModule
                $Private:info["pyHome"] = Split-Path $pyExecutable
                $Private:info["pyPath"] = [String]::Join(";", $(Get-PyPath -pyExecutable $pyExecutable))
                $Private:info["pyDll"] = $(Get-PyDll -pyExecutable $pyExecutable)
                $Private:info["pyNetModuleInfo"] = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyNetModule
                $Private:info["pyLedgerModuleInfo"] = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyLedgerModule
            } else { $Private:info["status_error"] = "Pip is not installed. Please, refer to Python documentation or to https://bootstrap.pypa.io for installation instructions" }
        } else { $Private:info["status_error"] = "Cannot get Python version ($pyExecutable)" }
    } else { $Private:info["status_error"] = "Python executable file '$pyExecutable' not found" }

    return [PSCustomObject]$Private:info
}

function Write-PyInfo {
    Begin { $Private:infos = @() }
    Process { 
        $Private:pyInfo = [ordered]@{}
        if ($_.pyExecutable) { $Private:pyInfo["Python Executable"] = $_.pyExecutable}
        if ($_.pyVersion) { $Private:pyInfo["Python Version"] = "$($_.pyVersion.Major).$($_.pyVersion.Minor).$($_.pyVersion.Build)"}
        if ($_.pyPlatform) { $Private:pyInfo["Python Platform"] = $_.pyPlatform}
        if ($_.pipVersion) { $Private:pyInfo["Pip Version"] = $_.pipVersion}
        if ($_.pyWheelInfo) { $Private:pyInfo["Wheel Version"] = $_.pyWheelInfo.Version}
        if ($_.pyHome) { $Private:pyInfo["Python HOME"] = $_.pyHome}
        if ($_.pyPath) { $Private:pyInfo["Python PATH"] = (($_.pyPath -split ";") -join "`n")}
        if ($_.pyDll) { $Private:pyInfo["Python Dll"] = $_.pyDll}
        if ($_.pyNetModuleInfo) { $Private:pyInfo["PythonNet Module Version"] = $_.pyNetModuleInfo.Version}
        if ($_.pyLedgerModuleInfo) { $Private:pyInfo["Ledger Module Version"] = $_.pyLedgerModuleInfo.Version}
        if ($_.status_error) { $Private:pyInfo["Status"] = ("{c:DarkRed}[Unreliable environment]{f:Normal} $($_.status_error)" | Out-AnsiString) }
        $Private:infos += [PSCustomObject]$Private:pyInfo
    }
    End { $Private:infos | Format-List }
}

function Get-LatestAvailableLedgerModule {
    [CmdletBinding()]
    Param()

    Get-ChildItem -LiteralPath $Script:ScriptPath -Filter "ledger-*-py*-none-any.whl" | 
        ForEach-Object { @{ FileName=$_.FullName; Version=[System.Version](($_.Name | Select-String -Pattern "ledger-(?<ver>\d+\.\d+\.\d+)-py\d-none-any\.whl").Matches.Groups[1].Value) } } |
        Sort-Object -Property Version -Descending |
        Select-Object -First 1
}

function Get-StatusInfo {
    [CmdletBinding()]
    Param()

    $Private:info = @{}

    $Private:info["py_settings_filename"] = $Script:settingsFileName
    $Private:info["py_settings"] = (Get-PythonIntegrationSettings)
    if ($Private:info["py_settings"]) { 
        $Private:info["py_executable"] = $Private:info["py_settings"].PyExecutable
        $Private:info["py_info"] = (Get-PyInfo -pyExecutable $Private:info["py_settings"].PyExecutable)
        $Private:info["is_py_info_valid"] = !($Private:info["py_info"].status_error)
        if ($Private:info["is_py_info_valid"]) {
            $Private:info["pynet_module_version"] = $Private:info["py_info"].pyNetModuleInfo.Version
            $Private:info["pyledger_module_version"] = $Private:info["py_info"].pyLedgerModuleInfo.Version
            $Private:info["pyledger_module_expanded_version"] = $Private:info["py_info"].pyLedgerModuleInfo.ExpandedVersion
            $Private:info["latest_pyledger_module_version"] = (Get-LatestAvailableLedgerModule).Version
        }
        else {
            $Private:info["py_status_error"] = $Private:info["py_info"].status_error
        }
    }
    $Private:info["nledger_binaries"] = (Test-NLedgerBinaryLoaded)
    if ($Private:info["nledger_binaries"]) {
        $Private:info["extension_provider"] = ($(GetConfigData).Settings | Where-Object { $_.Name -eq "ExtensionProvider" } | Select-Object -First 1).EffectiveSettingValue
        $Private:info["is_python_extension_enabled"] = $Private:info["extension_provider"] -eq "python"
    }

    return [PSCustomObject]$Private:info
}

function Write-StatusInfo {
    Begin { $Private:infos = @() }
    Process { 
        $Private:info = [ordered]@{}
        if ($_.py_settings) {
            if ($_.is_py_info_valid) {
                $Private:info["Python Connection"] = ("{c:DarkGreen}[Enabled]{f:Normal} Connection is configured and active" | Out-AnsiString)
                $Private:info["Connection Settings"] = ( $_.py_settings_filename )
                $Private:info["Python Executable"] = ( $_.py_executable )
                $Private:info["PythonNet Module"] = $( if($_.pynet_module_version){$_.pynet_module_version}else{"Not installed"} )
                $Private:ledgerAvailable = $(if ($_.latest_pyledger_module_version -and $_.pyledger_module_expanded_version -lt $_.latest_pyledger_module_version) {"`nLedger Module $($_.latest_pyledger_module_version.ToString(3)) is available and can be installed"} else {""})
                $Private:info["Ledger Module"] = $( if($_.pyledger_module_version){"$($_.pyledger_module_version)$Private:ledgerAvailable"}else{"Not installed$Private:ledgerAvailable"} )
            } else {
                $Private:info["Python Connection"] = ("{c:DarkRed}[Disabled]{f:Normal} Connection is configured but referenced Python is not valid" | Out-AnsiString)
                $Private:info["Connection Settings"] = ( $_.py_settings_filename )
                $Private:info["Python Executable"] = ( $_.py_executable )
                $Private:info["Python Status"] = ("{c:DarkYellow}[Not Valid]{f:Normal} $($_.py_status_error)`nConfigure connection to another Python environment" | Out-AnsiString)
            }
        } else {
            $Private:info["Python Connection"] = ("{c:DarkRed}[Disabled]{f:Normal} Connection is not configured" | Out-AnsiString)
            $Private:info["Connection Settings"] = ("{c:DarkYellow}[File not found]{f:Normal} $($_.py_settings_filename)`nUse Set-PythonConnection command to create a connection file" | Out-AnsiString)
        }
        if ($_.nledger_binaries) {
            if ($_.is_python_extension_enabled) {
                $Private:info["Python Extension"] = ("{c:DarkGreen}[Enabled]{f:Normal} Extension is active" | Out-AnsiString)
            } else {
                if ([String]::IsNullOrEmpty($_.extension_provider)) {
                    $Private:info["Python Extension"] = ("{c:DarkRed}[Disabled]{f:Normal} Extension provider is not set.`nUse {c:DarkYellow}enable{f:Normal} command to set correct provider." | Out-AnsiString)
                } else {
                    $Private:info["Python Extension"] = ("{c:DarkRed}[Disabled]{f:Normal} Current extension provider is '$($_.extension_provider)' whereas 'python' expected`nUse {c:DarkYellow}enable{f:Normal} command to set correct provider." | Out-AnsiString)
                }
            }
        } else {
            $Private:info["Python Extension"] = ("{c:DarkRed}[Not Available]{f:Normal} NLedger binaries not found`nBuild binaries or correct deployment issues" | Out-AnsiString)
        }

        $Private:infos += [PSCustomObject]$Private:info
    }
    End { $Private:infos | Format-List }
}

function Test-Connection {
    [CmdletBinding()]
    Param()

    $Private:settings = Get-PythonIntegrationSettings
    if ($Private:settings) {
        $Private:pyInfo = Get-PyInfo -pyExecutable $Private:settings.PyExecutable
        return [System.Management.Automation.PSSerializer]::Serialize([PSCustomObject]@{
            IsConnectionValid = -not [Boolean]$Private:pyInfo.status_error
            IsWheelInstalled = [Boolean]$Private:pyInfo.pyWheelInfo
            IsPythonNetInstalled = [Boolean]$Private:pyInfo.pyNetModuleInfo
        })
    }
    return $null
}

### Commands ###

<#
.SYNOPSIS
Discovers available Python installations on the local machine

.DESCRIPTION
Looks for local Python installations and checks whether they are ready for NLedger Python integration.

It observes the following locations:
- folders listed in PATH environment variable (local Python deployment)
- the folder with isolated embedded Python deployments for NLedger (NLedger application data folder)
- the folder that is specified in NLedger Python extension settings (if they exist)

If you would like to check Python deployment in a specific folder, use 'path' parameter.

If Python installation is detected, the command returns the following information:
- full path to Python executable file
- Python version
- Python platform (x86 or x64)
- Pip version (if installed)
- Python HOME and PATH variables
- Python dll name
- PythonNet module version (if installed)
- Ledger module version (if installed)

The minimal required indicator that Python installation can be used by NLedger is returned valid Python version.
It is also essential to check that Python platform matches NLedger process (x64 in default configuration).
PythonNet and Ledger modules are optional and needs to be installed if you want to use NLedger capabilities in Python session.

.PARAMETER path
Optional parameter containing a full path to a Python executable file.

#>
function Discover {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)
  
    Write-Output ""
    if($path) {
        Write-Output ("{c:DarkCyan}[Local Python]{f:Normal} Testing Python by path: $path" | Out-AnsiString )
        Get-PyInfo -pyExecutable $path | Write-PyInfo
        Write-Output ""
      
    } else {
        Write-Verbose "Search local Python"
        $path = Search-PyExecutable
        if ($path) {
            Write-Output ("{c:DarkCyan}[Local Python]{f:Normal} Found by path: $path" | Out-AnsiString )
            Get-PyInfo -pyExecutable $path | Write-PyInfo
        } else { Write-Output ("{c:DarkCyan}[Local Python]{f:Normal} Not found" | Out-AnsiString )}
        Write-Output ""
  
        if ($Script:isWindowsPlatform) {
          Write-Verbose "Search embed Python"
          $embeds = Search-PyEmbedInstalled -appPrefix $appPrefix -pyPlatform $pyPlatform -fullPath
          if (!$embeds) { 
              Write-Output ("{c:DarkCyan}[Embed Python]{f:Normal} Not found" | Out-AnsiString )
          } else {
              Write-Output ("{c:DarkCyan}[Embed Python]{f:Normal} Found $(($embeds | Measure-Object).Count) deployment(s)" | Out-AnsiString )
              $embeds | ForEach-Object { Get-PyInfo -pyExecutable $_ } | Write-PyInfo
          }
        }
          
        Write-Verbose "Check NLedger Python settings"
        $settings = Get-PythonIntegrationSettings
        if($settings) {
            Write-Output ( "{c:DarkCyan}[NLedger Settings]{f:Normal} Found connection settings file: $Script:settingsFileName" | Out-AnsiString )
            Write-Output "Configured path to Python executable: $($settings.PyExecutable)"
            Get-PyInfo -pyExecutable $settings.PyExecutable | Write-PyInfo
        } else { Write-Output ("{c:DarkCyan}[NLedger Settings]{f:Normal} Not found (no file $Script:settingsFileName)" | Out-AnsiString ) }
        Write-Output ""
    }  
}

<#
.SYNOPSIS
Creates or updates NLedger Python extension settings

.DESCRIPTION
NLedger Python extension settings describe which Python deployment will be used by NLedger.
They are incorporated into a single XML file in a pre-defined location (NLedger.Extensibility.Python.settings.xml in NLedger application data folder).
Settings include a full path to a Python executable file, Python HOME and PATH variables and the name of Python dll.

The command is intended to create NLedger Python extension settings automatically by extracting needed information from an existing Python deployment.

In the first step, it determines the location of Python deployment in the following order:
- If 'path' parameter is specified, it uses the parameter value explicitely
- If 'embed' parameter is specified, it installs an embedded Python in NLedger application folder and refers to it
- If extension settings file already exists, it extracts the path from the file
- Otherwise, it tries to find a local Python installation.

When Python deployment is found, it checks whether it is in valid state and creates the settings file.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
The command will use the specified Python deployment to establish the connection.

.PARAMETER embed
Optional parameter containing a version of embedded Python. 
If this parameter is specified, the command installs and refers to the specified embedded Python to establish the connection.
This parameter is available on Windows only.

#>
function Connect {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)][string]$path,
        [Parameter(Mandatory=$False)][string]$embed
    )

    if ($path -and $embed) { throw "Connect command cannot get both 'path' and 'embed' parameters at the same time."}

    Write-Output ""
    $Private:settings = Get-PythonIntegrationSettings

    if (!$path -and !$embed) {
        Write-Verbose "Auto-configuring parameters. Checking existing settings first."
        if ($Private:settings) {
            $path = $settings.PyExecutable
            Write-Output "Found path '$path' specified in NLedger Python settings ($Script:settingsFileName)"
        } else {
            Write-Verbose "No settings file. Searching local Python"
            $path = Search-PyExecutable
            if (!$path) {
                if ($Script:isWindowsPlatform) {
                    $embed = $pyEmbedVersion
                    Write-Output "Auto-configuring: use embed Python $pyEmbedVersion (since local Python not found and no settings file)"
                } else { throw "Auto-configuring is not possible: local Python not found and no settings file. Specify 'path' parameter." }
            }
        }        
    }

    if ($embed) {
        Write-Verbose "Installing embedded python $embed"
        $path = Search-PyExecutable -pyHome (Get-PyEmbed -appPrefix $appPrefix -pyVersion $embed -pyPlatform $pyPlatform)
        Write-Output "Installed embed Python $embed by path $path"
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Connection error: cannot use Python by path $path (Error: $($Private:info.status_error))" }

    Write-Verbose "Update or create settings"
    if (!$settings -or ($settings.PyExecutable -ne $Private:info.pyExecutable -or $settings.PyDll -ne $Private:info.pyDll)) {
        Write-Verbose "Settings file is outdated"
        $settings = Update-PythonEnvironmentSettings -pyExecutable $Private:info.pyExecutable -pyDll $Private:info.pyDll
        Write-Output "Settings file ($Script:settingsFileName) is updated"
    } else { Write-Verbose "Settings file $Script:settingsFileName is actual"}

    Write-Output ("NLedger Python connection is {c:DarkYellow}active{f:Normal} (Settings file: $Script:settingsFileName)`n" | Out-AnsiString)
}

<#
.SYNOPSIS
Shows current status of NLedger Python integration

.DESCRIPTION
NLedger Python integration works when two conditions are satisfied:
- Python Extension settings are configured and refer to a valid Python deployment (Python Connection)
- Python Extension is enabled in NLedger settings (Python Extension)

This command checks both conditions and returns a final status. 
If both indicators are Green, NLedger can process Python extensions in data files and manage python-related commands.

In addition, information about installed Python modules (PythonNet and Ledger) 
indicates whether Ledger capabilities can be used in a Python session (both modules should be installed).
Remember that Python sessions work isolately and do not need for any settings actually.

#>
function Status {
    [CmdletBinding()]
    Param()

    Write-Output ("`n{c:DarkCyan}[Python Extension Status]{f:Normal}`n" | Out-AnsiString )
    Write-Output "'Python Connection' status indicates whether the reference to Python is properly configured"
    Write-Output "'Python Extension' status indicates whether NLedger uses the referenced Python"

    Get-StatusInfo | Write-StatusInfo
}

<#
.SYNOPSIS
Enables Python extension in NLedger settings

.DESCRIPTION
Python extension should be enabled in NLedger settings for it to work.

This command checks whether Python extension is properly configured, executes 'connect' command otherwise and 
sets NLedger settings 'ExtensionProvider' to 'python' to make the extension operable.

#>
function Enable {
    [CmdletBinding()]
    Param()
    
    $Private:info = Get-StatusInfo
    if(!$Private:info.nledger_binaries) {throw "Cannot enable Python extension because NLedger binaries not found. Build NLedger or correct deployment issues"}
    if ($Private:info.extension_provider -and $Private:info.extension_provider -ne "python") {throw "Cannot enable Python extension because 'ExtensionProvider' setting value in NLedger configuration already configured for another provider: $($Private:info.extensionProvider)"}

    Connect
    if ($Private:info.extension_provider -ne "python") { $null = SetConfigValue -scope "user" -settingName "ExtensionProvider" -settingValue "python" }

    $Private:info = Get-StatusInfo
    if ($Private:info.extension_provider -ne "python") { Write-Output ("{c:DarkRed}Cannot enable Python extension{f:Normal}" | Out-AnsiString) }
    else { Write-Output ("NLedger Python extension is {c:DarkGreen}enabled{f:Normal}" | Out-AnsiString)}
    Write-Output ""
}

<#
.SYNOPSIS
Disables Python extension in NLedger settings

.DESCRIPTION
Sets an empty value to NLedger 'ExtensionProvider' setting, making Python extension unavailable.
Extension configuration settings (the file NLedger.Extensibility.Python.settings.xml) themselves are not changed.

#>
function Disable {
    [CmdletBinding()]
    Param()
    
    Write-Output ""
    $Private:info = Get-StatusInfo
    if(!$Private:info.nledger_binaries) {throw "Cannot enable Python extension because NLedger binaries not found. Build NLedger or correct deployment issues"}    
    if ($Private:info.extension_provider -and $Private:info.extension_provider -ne "python") {throw "Cannot disable extension because 'ExtensionProvider' setting value in NLedger configuration already configured for another provider: $($Private:info.extensionProvider)"}

    if ($Private:info.extension_provider -eq "python") { $null = RemoveConfigValue -scope "user" -settingName "ExtensionProvider" }

    $Private:info = Get-StatusInfo
    if ($Private:info.extension_provider -eq "python") { Write-Output ("{c:DarkRed}Cannot disable extension{f:Normal}; use NLedger Configuration console to sort out the issue manually" | Out-AnsiString) }
    else { Write-Output ("NLedger Python extension is {c:DarkRed}disabled{f:Normal}" | Out-AnsiString) }
    Write-Output ""
}

<#
.SYNOPSIS
Removes NLedger Python extension settings

.DESCRIPTION
Deletes the file with NLedger Python extension settings (NLedger.Extensibility.Python.settings.xml)
It makes Python extension unavailable regardless of whether the NLedger setting (ExtensionProvider) is configured for Python

#>
function Disconnect {
    [CmdletBinding()]
    Param()
    
    Write-Output ""
    $Private:info = Get-StatusInfo
    if ($Private:info.extension_provider -eq "python") {throw "Cannot unlink Python connection because Python extension is currently active ('ExtensionProvider' setting value in NLedger configuration is 'python'). Disable extension first using 'disable' command"}

    Remove-PythonIntegrationSettings
    Write-Output ("NLedger Python connection is {c:DarkYellow}not active{c:Gray} (settings file $Script:settingsFileName is removed)`n" | Out-AnsiString)
}

<#
.SYNOPSIS
Installs embedded Python

.DESCRIPTION
Downloads, installs and configures an isolated embedded Python for NLedger.
This is a Windows-only command; it is not available for other OS.

All embedded python deployments are located in NLedger application data folder ([user]\AppData\Local\NLedger\).
It is acceptable to install several different versions of embedded Python; all they will be located at the same place.
Once the embedded Python is installed, you can use the path to Python executable file to complete connection configuration.

The command downloads Python package by URL https://www.python.org/ftp/python/[version]/[embedded package name].
After installing Python, it also downloads and installs Pip module (https://bootstrap.pypa.io/get-pip.py) so that the embedded Python can manage regular Wheel packages.
Note: before installing Pip, it also corrects Python's _PTH file (uncomments 'import site') that fixes installing Pip for embedded deployments.

When the command finishes, it shows the full path to the installed Python.

.PARAMETER version
Optional parameter containing a version of embedded Python.
Default value is 3.8.1 (this version will be downloaded if 'version' parameter is omitted)
#>
function Install-Python {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$version = $pyEmbedVersion)

    Write-Output ""
    Write-Output "Embedded Python is available by path: $(Get-PyEmbed -appPrefix $appPrefix -pyVersion $version -pyPlatform $pyPlatform)"
    Write-Output ""
}

<#
.SYNOPSIS
Uninstalls embedded Python

.DESCRIPTION
Removes previously installed embedded Python from NLedger application data folder.

.PARAMETER version
Optional parameter containing a version of embedded Python.
Default value is 3.8.1 (this version will be uninstalled if 'version' parameter is omitted)
#>
function Uninstall-Python {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$version = $pyEmbedVersion)

    Write-Output ""
    $Private:settings = Get-PythonIntegrationSettings
    $Private:path = Test-PyEmbedInstalled -appPrefix $appPrefix -pyPlatform $pyPlatform -pyVersion $version
    if ($Private:path) {
        if ($Private:settings -and [System.IO.Path]::GetFullPath($(Split-Path $Private:settings.PyExecutable)) -eq $Private:path) { throw "Cannot uninstall embed Python $version because it is currently linked (PyHome $Private:path). Unlink connection first."}
        Uninstall-PyEmbed -appPrefix $appPrefix -pyPlatform $pyPlatform -pyVersion $version
    }

    Write-Output "Embedded Python $version is uninstalled"
    Write-Output ""
}

<#
.SYNOPSIS
Installs Wheel module to Python environment

.DESCRIPTION
Installs Wheel module to Python by means of Pip.

Note: Wheel module is only needed if you want to build Ledger package from source code (e.g. by means of get-nledger-up.ps1).
.Net Ledger and Ledger module can work without Wheel. 

Note: you can check whether Wheel is installed by means of "discover" command. It will show Wheel version if it is found.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.
#>
function Install-Wheel {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)][string]$path
    )

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    if(!($Private:info.pyWheelInfo)) {
        $null = Install-PyModule -pyExe $path -pyModule $pyWheelModule -Verbose:$VerbosePreference
    }

    $Private:packageInfo = Test-PyModuleInstalled -pyExecutable $path -pyModule $pyWheelModule

    if($Private:packageInfo){
        Write-Output ("Wheel {c:DarkYellow}$($Private:packageInfo.Version.Trim()){f:Normal} is installed ($path)" | Out-AnsiString)
    } else {
        Write-Output ("Wheel {c:DarkRed}has not been installed{f:Normal} ($path). Use -verbose for troubleshooting." | Out-AnsiString)
    }
}

<#
.SYNOPSIS
Uninstalls Wheel module from Python environment

.DESCRIPTION
Uninstall Wheel module to Python by means of Pip.

Note: Wheel module is only needed if you want to build Ledger package from source code (e.g. by means of get-nledger-up.ps1).
.Net Ledger and Ledger module can work without Wheel. 

Note: you can check whether Wheel is installed by means of "discover" command. It will show Wheel version if it is found.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.
#>
function Uninstall-Wheel {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    $null = Uninstall-PyModule -pyExe $path -pyModule $pyWheelModule -Verbose:$VerbosePreference

    Write-Output "Wheel is uninstalled ($path)"
}

<#
.SYNOPSIS
Installs PythonNet module to Python environment

.DESCRIPTION
Note: PythonNet and Ledger modules have to be installed only if you want to use NLedger capabilities in Python session.
NLedger console application does not need for this module.

Ledger module depends on PythonNet, so the latter is usually installed automatically when you install the Ledger module.
However, you may need more granular control over which version of PythonNet is installed. 

In this case, you should execute this command before installing Ledger module:
- 'Install-PythonNet' without '-v3' switch installs a currently available PythonNet from PyPa repository (currently 2.5.x)
- 'Install-PythonNet' with '-v3' switch installs the latest available PythonNet 3 from https://ci.appveyor.com/api/projects/pythonnet repository.

Please, be aware that PythonNet 3 has not been officially released (at the moment 2021-10-18) so this is an experimental option (though it passed tests).
Ledger module does not require PythonNet version 3 specifically; it properly works with any available version.

In case of any issues with this command, you can manually install PythonNet following recommendations on PythonNet resources.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

.PARAMETER v3
Optional switch that forces installing PythonNet version 3 (from AppVeyor PythonNet repository).
If this parameter is omitted, the command installs a currently available PythonNet from PyPa repository.

#>
function Install-PythonNet {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)][string]$path,
        [Switch]$v3 = $False
    )

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    $Private:packageName = "pythonnet"
    if($v3) {
        Write-Output "Looking for the latest PythonNet 3 package on Appveyor"
        $Private:pyNet3Package = Get-PythonNet3 -download
        if (!$Private:pyNet3Package) { throw "Cannot find the latest PythonNet3 package" }
        Write-Output "Found PythonNet 3 package:"
        $Private:pyNet3Package | Format-List
        $Private:packageName = $Private:pyNet3Package.LocalFile
    }

    $null = Uninstall-PyModule -pyExe $path -pyModule "pythonnet" -Verbose:$VerbosePreference
    $null = Install-PyModule -pyExe $path -pyModule $Private:packageName -Verbose:$VerbosePreference
    $Private:packageInfo = Test-PyModuleInstalled -pyExecutable $path -pyModule "pythonnet"

    if($Private:packageInfo){
        Write-Output ("PythonNet {c:DarkYellow}$($Private:packageInfo.Version.Trim()){f:Normal} is installed ($path)" | Out-AnsiString)
    } else {
        Write-Output ("PythonNet {c:DarkRed}has not been installed{f:Normal} ($path). Use -verbose for troubleshooting." | Out-AnsiString)
    }
}

<#
.SYNOPSIS
Uninstalls PythonNet module from Python environment

.DESCRIPTION
Note: Pythonnet and Ledger modules have to be installed only if you want to use NLedger capabilities in Python session.
NLedger console application does not need for this module.

Uninstalls PythonNet module from Python environment. It does not remove the dependent Ledger module, but the latter becomes unusable.
This command might be helpful if you want to re-install PythonNet on previously configured Python environment.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

#>
function Uninstall-PythonNet {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    $null = Uninstall-PyModule -pyExe $path -pyModule "pythonnet" -Verbose:$VerbosePreference

    Write-Output "PythonNet is uninstalled ($path)"
}

<#
.SYNOPSIS
Installs Ledger module to Python environment

.DESCRIPTION
Note: Ledger module should be installed only if you want to use NLedger capabilities in Python session.
NLedger console application does not need for this module.

Ledger module allows you to use NLedger capabilities in Python session. It includes reading journal files, accessing data objects and
executing commands. Ledger module can be distributed as an independent software; it does not need for NLedger installation and does not require any special settings.

The Ledger module is packaged in a Wheel format file. Though it can be installed in a usual way, this command may be useful for simplifying the installation steps.

The Ledger module depends on PythonNet (2.5.x or 3.x). Pip package manager will try to resolve dependencies and automatically install PythonNet, 
so you probably do not need to worry about that. However, in case of any problems with PythonNet installation, 
it is recommended to install PythonNet before Ledger (by means of 'install-pythonnet' command or manually).

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

#>
function Install-Ledger {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)

    $Private:ledgerPackage = (Get-LatestAvailableLedgerModule).FileName
    if (!$Private:ledgerPackage) {throw "Ledger package not found. Build NLedger or correct deployment issues"}

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    $null = Uninstall-PyModule -pyExe $path -pyModule "ledger" -Verbose:$VerbosePreference
    $null = Install-PyModule -pyExe $path -pyModule $Private:ledgerPackage -Verbose:$VerbosePreference
    $Private:packageInfo = Test-PyModuleInstalled -pyExecutable $path -pyModule "ledger"

    if($Private:packageInfo){
        Write-Output ("Ledger {c:DarkYellow}$($Private:packageInfo.Version.Trim()){f:Normal} is installed ($path)" | Out-AnsiString)
    } else {
        Write-Output ("Ledger {c:DarkRed}has not been installed{f:Normal} ($path). Use -verbose for troubleshooting." | Out-AnsiString)
    }
}

<#
.SYNOPSIS
Uninstalls Ledger module from Python environment

.DESCRIPTION
Note: Pythonnet and Ledger modules have to be installed only if you want to use NLedger capabilities in Python session.
NLedger console application does not need for this module.

Uninstalls Ledger module from Python environment. It does not remove the dependent PythonNet module, so you may need to uninstall it separately.
This command might be helpful if you want to re-install Ledger module on previously configured Python environment.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

#>
function Uninstall-Ledger {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }

    $null = Uninstall-PyModule -pyExe $path -pyModule "ledger" -Verbose:$VerbosePreference

    Write-Output "Ledger is uninstalled ($path)"
}

<#
.SYNOPSIS
Runs Ledger module tests

.DESCRIPTION
Ledger module package (ledger-[version].whl) is distributed with a test file (ledger_tests.py) that is based on Python unit tests.
Test coverage is about 100%, so passed tests guarantee that the module functions properly.

You may run the tests in a command line console as usual Python unit tests: [path to python] ledger_tests.py
This command performs the same action but in a shorter notation.

Note: Ledger module should be installed in the Python environment.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

#>
function Test-Ledger {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)

    [string]$Private:ledgerTests = [System.IO.Path]::GetFullPath("$Script:ScriptPath/ledger_tests.py")
    if(!(Test-Path -LiteralPath $Private:ledgerTests -PathType Leaf)) {throw "File $($Private:ledgerTests) not found"}

    if(!$path) {
        $Private:settings = Get-PythonIntegrationSettings
        if (!$Private:settings) { throw "Python path is not specified and no Python connection settings." }
        $path = $settings.PyExecutable
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.status_error) { throw "Python environment by path $path is not valid (Error: $($Private:info.error))" }
    if (!$Private:info.pyLedgerModuleInfo) { throw "Ledger module is not installed by path $path" }

    & $path $Private:ledgerTests
    if ($LASTEXITCODE -ne 0) {throw "Python unit tests failed (Exit code: $LASTEXITCODE)."}
}

function Help {
    [CmdletBinding()]
    Param()

    $Private:commands = [ordered]@{
        ("{c:Yellow}discover{f:Normal}" | Out-AnsiString) = "Find Python deployments on the local machine"
        ("{c:Yellow}status{f:Normal}" | Out-AnsiString) = "Show NLedger Python integration status"
        ("{c:Yellow}connect{f:Normal}" | Out-AnsiString) = "Create a connection file that references to a Python deployment"
        ("{c:Yellow}disconnect{f:Normal}" | Out-AnsiString) = "Remove the connection file"
        ("{c:Yellow}enable{f:Normal}" | Out-AnsiString) = "Enable Python extension in NLedger settings"
        ("{c:Yellow}disable{f:Normal}" | Out-AnsiString) = "Disable Python extension in NLedger settings"
        ("{c:DarkYellow}install-{f:Normal}[target]" | Out-AnsiString) = "Install embedded Python (install-python) or Wheel (install-wheel) or PythonNet (install-pythonnet) or Ledger module (install-ledger)"
        ("{c:DarkYellow}uninstall-{f:Normal}[target]" | Out-AnsiString) = "Uninstall listed targets"
        ("{c:DarkYellow}test-ledger{f:Normal}" | Out-AnsiString) = "Run tests for installed Ledger module"
        ("{c:DarkYellow}help{f:Normal}" | Out-AnsiString) = "Shows this help text"
        ("{c:DarkYellow}get-help{f:Normal} [command]" | Out-AnsiString) = "Get additional information for a specified command"
        ("{c:DarkYellow}exit{f:Normal}" | Out-AnsiString) = "Close console window"
    }

    Write-Output "Available commands:"
    ([PSCustomObject]$Private:commands) | Format-List
}


# Manage parameters

if (!$command) {
    Write-Output ""
    Write-Output ("{c:White}NLedger Python Toolset Console" | Out-AnsiString)
    Write-Output ("******************************{f:Normal}" | Out-AnsiString)
    Write-Output "This script manages NLedger Python Extension settings"
    Write-Output "It can discover environment details, configure and validate NLedger settings, install or configure dependent software."
    Write-Output ""
    Help
    return
}

if ($command -eq 'discover') { 
    if($version){throw "Command 'discover' does not accept 'version' parameter"}
    Discover -path $path 
}

if ($command -eq 'status') { 
    if($path -or $version){throw "Command 'status' does not accept any parameters"}
    Status 
}

if ($command -eq 'connect') {
    Connect -path $path -embed $version
}

if ($command -eq 'disconnect') { 
    if($path -or $version){throw "Command 'disconnect' does not accept any parameters"}
    Disconnect 
}

if ($command -eq 'enable') {
    if($path -or $version){throw "Command 'enable' does not accept any parameters"}
    Enable
}

if ($command -eq 'disable') {
    if($path -or $version){throw "Command 'disable' does not accept any parameters"}
    Disable
}

if ($command -eq 'test-connection') {
    if($path -or $version){throw "Command 'test-connection' does not accept any parameters"}
    Test-Connection
}

if ($command -eq 'install-python') {
    if($path){throw "Command 'install-python' does not accept 'path' parameters"}
    Install-Python -version $version
}

if ($command -eq 'install-wheel') {
    Install-Wheel -path $path
}

if ($command -eq 'install-pythonnet') {
    if($version -and $version -ne "v3"){throw "Command 'install-pythonnet' does not accept 'version' parameter if it not empty and not equal to 'v3'"}
    Install-PythonNet -path $path -v3:($version -eq "v3")
}

if ($command -eq 'install-ledger') {
    if ($version) {throw "Command 'install-ledger' does not accept 'version' parameter"}
    Install-Ledger -path $path
}

if ($command -eq 'uninstall-python') {
    if($path){throw "Command 'uninstall-python' does not accept 'path' parameters"}
    if(!$version){throw "Command 'uninstall-python' requires 'version' parameters"}
    Uninstall-Python -version $version
}

if ($command -eq 'uninstall-wheel') {
    Uninstall-Wheel -path $path
}

if ($command -eq 'uninstall-pythonnet') {
    if($version){throw "Command 'uninstall-pythonnet' does not accept 'version' parameters"}
    Uninstall-PythonNet -path $path
}

if ($command -eq 'uninstall-ledger') {
    if ($version) {throw "Command 'uninstall-ledger' does not accept 'version' parameter"}
    Uninstall-Ledger -path $path
}

if ($command -eq 'test-ledger') {
    if ($version) {throw "Command 'test-ledger' does not accept 'version' parameter"}
    Test-Ledger -path $path
}
