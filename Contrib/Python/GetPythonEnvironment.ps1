<#
.SYNOPSIS
Getting NLedger Python extension ready to work

.DESCRIPTION
This script manages NLedger Python connection settings and involved external software to make them properly linked. It is responsible for:
- Validating local Python deployment to confirm compliance with NLedger requirements
- Optional installing and configuring embedded Python deployment (any versions; Windows only)
- Installing and verification connectivity software (Python.Net and Ledger modules)
- Producing NLedger Python Extension settings file
- Enabling Python Extension in NLedger configuration settings

The provided commands allow you to configure the connection both automatically and at a more detailed level. The commands are:
- discover - find and show status of local Python deployments
- status - validate and show connection status
- link - create NLedger Python connection settings. Installs missing required software
- enable - enables Python extension in NLedger configuration; creates connection settings if missed
- disable - disables Python extension in NLedger configuration
- unlink - removes NLedger Python connection settings
- install - install embed Python (Windows only)
- uninstall - uninstall embed Python (Windows only)
- testlink - validates and returns NLedger Python connection settings in technical format

Quick guide hint: in most cases, you only need to execute 'enable' command to configure and enable Python integration.
The commands can be executed by running the script with '-command' parameter or in console mode just by typing a command.

.PARAMETER command
Command to execute. Further information for every command you can get in console by typing 'get-help [command]'

.PARAMETER path
Optional path to Python executable file (for 'discover' and 'link' commands)

.PARAMETER embed
Optional version of embedded Python (for commands 'link', 'install', 'uninstall')

Note: use 'set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process' to run the script in dev terminal

#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][ValidateSet("discover","status","link","enable","testlink","disable","unlink","install","uninstall")]$command,
    [Parameter(Mandatory=$False)][string]$path,
    [Parameter(Mandatory=$False)][string]$embed
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

## Default settings

$appPrefix = "NLedger"
$pyPlatform = "amd64"
$pyNetModule = "PythonNet"
$pyLedgerModule = "Ledger"
$pyEmbedVersion = "3.8.1"
#$pyNetPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Packages/pythonnet-3.0.0.dev1-py3-none-any.whl")
#if (!(Test-Path -LiteralPath $pyNetPath -PathType Leaf)) { throw "Cannot find required file $pyNetPath" }
#$pyLedgerPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Packages/ledger-0.8.3-py3-none-any.whl")

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
        PyHome = $Private:xmlContent.'nledger-python-settings'.'py-home'
        PyPath = $Private:xmlContent.'nledger-python-settings'.'py-path'
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

    $Private:elm = $Private:doc.CreateElement("py-home")
    $Private:elm.InnerText = $settings.PyHome
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-path")
    $Private:elm.InnerText = $settings.PyPath
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
        [Parameter(Mandatory=$True)][string]$pyHome,
        [Parameter(Mandatory=$True)][string]$pyPath,
        [Parameter(Mandatory=$True)][string]$pyDll
    )

    $Private:settings = Get-PythonIntegrationSettings
    if(!$Private:settings) {
        $Private:settings = [PSCustomObject]@{
            PyExecutable = $pyExecutable
            PyHome = $pyHome
            PyPath = $pyPath
            PyDll = $pyDll
        }    
    } else {
        $Private:settings.PyExecutable = $pyExecutable
        $Private:settings.PyHome = $pyHome
        $Private:settings.PyPath = $pyPath
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
                $Private:info["pyHome"] = Split-Path $pyExecutable
                $Private:info["pyPath"] = [String]::Join(";", $(Get-PyPath -pyExecutable $pyExecutable))
                $Private:info["pyDll"] = "python$($Private:info.pyVersion.Major)$($Private:info.pyVersion.Minor)"
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


function Test-Link {
    [CmdletBinding()]
    Param()

    $Private:settings = Get-PythonIntegrationSettings
    if ($Private:settings) {
        $Private:pyInfo = Get-PyInfo -pyExecutable $Private:settings.PyExecutable
        if ($Private:pyInfo) {
            if(!$Private:pyInfo.error) {
                if(!$Private:pyInfo.warning) {
                    return $Private:settings
                } else { Write-Verbose "Test-link: settings exist but validating Python envirnment exposed a warning: $($Private:pyInfo.warning)." }
            } else { Write-Verbose "Test-link: settings exist but validating Python envirnment exposed an error: $($Private:pyInfo.error)." }
        } else { Write-Verbose "Test-link: settings exist but cannot validate Python environment." }
    }

    return $null
}

function Install-LedgerModule {
    [CmdletBinding()]
    Param()

    $Private:info = Get-StatusInfo
    if (!$Private:info.pyInfo -or $Private:info.pyInfo.error -or $Private:info.pyInfo.warning) { throw "Python is not ready for re-installing Ledger module ($($Private:info.pyInfo.error)$($Private:info.pyInfo.warning))" }

    if(!(Test-Path -LiteralPath $pyLedgerPath -PathType Leaf)) { throw "File $pyLedgerPath not found."}

    Write-Verbose "Uninstall Ledger package"
    if (Test-PyModuleInstalled -pyExecutable $Private:info.pyInfo.pyExecutable -pyModule $pyLedgerModule) {
        $null = Uninstall-PyModule -pyExecutable $Private:info.pyInfo.pyExecutable -pyModule $pyLedgerModule
        if (Test-PyModuleInstalled -pyExecutable $Private:info.pyInfo.pyExecutable -pyModule $pyLedgerModule) { throw "Cannot uninstall Python module '$pyLedgerModule'" }
    }

    Write-Verbose "Install Ledger package"
    $null = Install-PyModule -pyExecutable $Private:info.pyInfo.pyExecutable -pyModule $pyLedgerPath
    if (!(Test-PyModuleInstalled -pyExecutable $Private:info.pyInfo.pyExecutable -pyModule $pyLedgerModule)) { throw "Cannot uninstall Python module '$pyLedgerPath'" }

    Write-Verbose "Package is re-installed"
}


### Commands ###

<#
.SYNOPSIS
Discovering available local Python deployments

.DESCRIPTION
Looks for local Python deployments and validates whether they are ready for NLedger Python integration.
Observable sources:
- folders listed in PATH environment variable (looks for a local Python deployment)
- folders that might contain embed Python deployments for NLedger (in user data folder)
- folder that is specified in current NLedger Python settings (if exists)

If you would like to check Python status in another folder, use 'path' parameter.

.PARAMETER path
Optional parameter containing a full path to Python executable file.

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
  
        Write-Verbose "Search embed Python"
        $embeds = Search-PyEmbedInstalled -appPrefix $appPrefix -pyPlatform $pyPlatform -fullPath
        if (!$embeds) { 
            Write-Output ("{c:DarkCyan}[Embed Python]{f:Normal} $(if($Script:isWindowsPlatform){"Not found"}else{"Not available"})" | Out-AnsiString )
        } else {
            Write-Output ("{c:DarkCyan}[Embed Python]{f:Normal} Found $(($embeds | Measure-Object).Count) deployment(s)" | Out-AnsiString )
            $embeds | ForEach-Object { Get-PyInfo -pyExecutable $_ } | Write-PyInfo
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
Creating or verifying NLedger Python extension settings

.DESCRIPTION
The command is responsible to create NLedger Python extension settings or validate/reconfigure them if they exist.
If it finishes well, it guarantees that the settings are valid and the environment ready for integration.

As the first step, the command determines the Python location that will be used for integration:
- explicitely, if 'path' parameter is specified - will use Python environment located by this path
- explicitely, if 'embed' parameter is specified - will install and use the specified version of embedded Python
- implicitely, if no parameters specified. 

In the latter case:
- it uses the currently configured Python by checking NLedger Python extension settings (if it exists)
- otherwise, it looks for a local Python checking folders in PATH variable
- if a local Python not found, it installs an embed Python (on Windows) or raises an exception (on other platforms)

When Python path is found, it verifies:
- whether Python is functioning (by checking its version)
- whether Pip is installed
- whether Python.Net is installed (and installs otherwise)

If all validations passed, it actualizes NLedger Python extension settings - the environment is ready for integration

.PARAMETER path
Optional parameter containing a full path to Python executable file.
The command will use the specified Python deployment to establish the connection.

.PARAMETER embed
Optional parameter containing a version of embedded Python.
The command will install and use the specified embedded Python to establish the connection.

#>
function Connect {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)][string]$path,
        [Parameter(Mandatory=$False)][string]$embed
    )

    if ($path -and $embed) { throw "Link command cannot get both 'path' and 'embed' parameters at the same time."}

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
    if (!$settings -or ($settings.PyExecutable -ne $Private:info.pyExecutable -or $settings.PyHome -ne $Private:info.pyHome -or $settings.PyPath -ne $Private:info.pyPath -or $settings.PyDll -ne $Private:info.pyDll)) {
        Write-Verbose "Settings file is outdated"
        $settings = Update-PythonEnvironmentSettings -pyExecutable $Private:info.pyExecutable -pyHome $Private:info.pyHome -pyPath $Private:info.pyPath -pyDll $Private:info.pyDll
        Write-Output "Settings file ($Script:settingsFileName) is updated"
    } else { Write-Verbose "Settings file $Script:settingsFileName is actual"}

    Write-Output ("NLedger Python connection is {c:DarkYellow}active{f:Normal} (Settings file: $Script:settingsFileName)`n" | Out-AnsiString)
}

<#
.SYNOPSIS
Checks current status of NLedger Python integration

.DESCRIPTION
The command observes local environments and shows the follwoing indicators:
- whether Python extension is enabled in NLedger configuration
- whether Python extension is usable (shows validation errors otherwise)
- whether NLedger Python extesion settings file exist
- whether configured settings are valid (shows validation errors otherwise)
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
The command validates Python connection settings and enabled Python extension
if Python connection settings are not specified yet, it executes 'link' command with default parameters
When Python connection settings are valid, it installs Ledger module and enables Python extension.
NLedger is completely ready to interop with Python 
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
Disables Python extension in NLedger settings; all other settings are kept unchanged
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
Removes NLedger Python connection settings

.DESCRIPTION
If Python extension is disabled, removes settings file
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
Downloads and installs embedded Python for NLedger in user data folder

.PARAMETER embed
Optional parameter containing a version of embedded Python.
Will use a default (3.8.1) version otherwise
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
Removes embedded Python for NLedger if it is not in use in current Python connection settings

.PARAMETER embed
Optional parameter containing a version of embedded Python.
Will use a default (3.8.1) version otherwise
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
        ("{c:DarkYellow}install-{f:Normal}[target]" | Out-AnsiString) = "Install embedded Python (install-python) or PythonNet (install-pythonnet) or Ledger module (install-ledger)"
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

if ($path -and $command -in @("status","disable","unlink","install","uninstall","testlink")) { throw "'path' parameter is not allowed for command '$command'"}
if ($embed -and $command -in @("discover","status","disable","unlink","testlink")) { throw "'embed' parameter is not allowed for command '$command'"}
if ($path -and $embed) { throw "'path' and 'embed' cannot be specified both at the same time" }

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

if ($command -eq 'discover') { Discover -path $path }
if ($command -eq 'status') { Status }

if ($command -eq 'link') {
    if ($embed) {Link -embed $embed} else {
        if ($path) {Link -path $path} else { Link }
    }
}

if ($command -eq 'enable') {
    if ($embed) {Link -embed $embed} else {
        if ($path) {Link -path $path} else { Link }
    }
    Enable
}

if ($command -eq 'disable') { Disable }
if ($command -eq 'unlink') { Unlink }
if ($command -eq 'install') { if ($embed) {Install -version $embed}else{Install} }
if ($command -eq 'uninstall') { if ($embed) {Uninstall -version $embed}else{Unnstall} }
if ($command -eq 'testlink') { Test-Link }
