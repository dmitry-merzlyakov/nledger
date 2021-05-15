<#
.SYNOPSIS
Getting Python environment ready for NLedger Integration

.DESCRIPTION
Configures and validates local Python so that it matches NLedger requirements for Python integration.
If this script passes well, it guarantess that settings NLedger.Extensibility.Python.settings.xml are valid and NLedger can use Python connector.

The script scenario is:
1. If settings do not exist: look for Python on local environment:
    a) If local Python not found:
        i) On Windows: download Embed Python, install pip and wheels
        ii) On other platforms: error message (people should manually install local Python and/or pip)
2. Use Python specified in settings or found or installed;
3. Check that Python has pip and PythonNet installed:
    a) If installed PythonNet has version less than 3.0: error message (people should decide whether they are ready to use PythonNet 3.0 and manually uninstall previous version)
    b) If PythonNet is not installed: install it from local Packages folder (local package is needed until pip is updated with PythonNet 3.0)
4. Collect additional information about Python (Python path, home, dll name and path to PythonNet Runtime dll)
5. Check that settings have actual information about Python and update it if necessary.

.PARAMETER pyExecutable
Path to Python executable file. If settings do not exist, the script will use this pah and do not search a local Python

.PARAMETER pyEmbedVersion
Preferred version of Embed Python. If settings do not exist and no local Python, the script will install this version of Embed python.

.PARAMETER preferEmbedded
On Windows, if settings do not exist, this switch indicates that you prefer installing embed python rather than using a global one.
Ignored on other platforms.

.PARAMETER configurePython
Allows making changes in local Python (installing packages).
This switch is disabled by default so the script does not make any changes but only inform the user about needed modifications.

.PARAMETER settingsUpdate
Allows updating the settings file.

#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][ValidateSet("discover","status","link","enable","testconn","disable","unlink","install","uninstall")]$command,
    [Parameter(Mandatory=$False)][string]$path,
    [Parameter(Mandatory=$False)][string]$embed
)

# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/PyManagement.psm1 -Force
Import-Module $Script:ScriptPath/../NLManagement/NLSetup.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

## Default settings

$appPrefix = "NLedger"
$pyPlatform = "amd64"
$pyNetModule = "PythonNet"
$pyLedgerModule = "Ledger"
$pyEmbedVersion = "3.8.1"
$pyNetPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Packages/pythonnet-3.0.0.dev1-py3-none-any.whl")
if (!(Test-Path -LiteralPath $pyNetPath -PathType Leaf)) { throw "Cannot find required file $pyNetPath" }
$pyLedgerPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Packages/ledger-0.8.3-py3-none-any.whl")

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
        PyNetRuntimeDll = $Private:xmlContent.'nledger-python-settings'.'py-net-runtime-dll'
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

    $Private:elm = $Private:doc.CreateElement("py-net-runtime-dll")
    $Private:elm.InnerText = $settings.PyNetRuntimeDll
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
        $null = Remove-Item -LiteralPath $Private:settingsFileName
        if (Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf) { throw "Cannot delete file $Script:settingsFileName" }
    }
}

function Update-PythonEnvironmentSettings {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pyExecutable,
        [Parameter(Mandatory=$True)][string]$pyHome,
        [Parameter(Mandatory=$True)][string]$pyPath,
        [Parameter(Mandatory=$True)][string]$pyDll,
        [Parameter(Mandatory=$True)][string]$PyNetRuntimeDll
    )

    $Private:settings = Get-PythonIntegrationSettings
    if(!$Private:settings) {
        $Private:settings = [PSCustomObject]@{
            PyExecutable = $pyExecutable
            PyHome = $pyHome
            PyPath = $pyPath
            PyDll = $pyDll
            PyNetRuntimeDll = $PyNetRuntimeDll
        }    
    } else {
        $Private:settings.PyExecutable = $pyExecutable
        $Private:settings.PyHome = $pyHome
        $Private:settings.PyPath = $pyPath
        $Private:settings.PyDll = $pyDll
        $Private:settings.PyNetRuntimeDll = $PyNetRuntimeDll
    }

    $null = Set-PythonIntegrationSettings $Private:settings
    return $Private:settings
}


### Functions ###

function Get-PyInfo {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    Write-Verbose "Getting Python deployment information for path $pyExecutable"

    $Private:info = [PSCustomObject]@{
        error = ""
        warning =""
        pyExecutable = $pyExecutable
        pyVersion = $null
        pipVersion = $null
        pyHome = ""
        pyPath = ""
        pyDll = ""
        pyNetModuleInfo = $null
        pyNetRuntimeDll = ""
        pyLedgerModuleInfo = $null
    }

    if (Test-Path -LiteralPath $pyExecutable -PathType Leaf) {
        $Private:info.pyVersion = Get-PyExpandedVersion -pyExe $pyExecutable
        if ($Private:info.pyVersion) {
            #TODO check Python platform
            $Private:info.pipVersion = Get-PipVersion -pyExe $pyExecutable
            if ($Private:info.pipVersion) {
                $Private:info.pyHome = Split-Path $pyExecutable
                $Private:info.pyPath = [String]::Join(";", $(Get-PyPath -pyExecutable $pyExecutable))
                $Private:info.pyDll = "python$($Private:info.pyVersion.Major)$($Private:info.pyVersion.Minor)"
                #TODO check pyDll
                $Private:info.pyNetModuleInfo = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyNetModule
                if ($Private:info.pyNetModuleInfo) {
                    if ($Private:info.pyNetModuleInfo.MajorVersion -ge 3) {
                        $Private:info.pyNetRuntimeDll = [System.IO.Path]::GetFullPath("$($Private:info.pyNetModuleInfo.Location.Trim())/pythonnet/runtime/Python.Runtime.dll")
                        if(Test-Path -LiteralPath $Private:info.pyNetRuntimeDll -PathType Leaf) {
                            #TODO check PythonNet runtime platform (x86 vs x64)
                            $Private:info.pyLedgerModuleInfo = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyLedgerModule
                        } else { $Private:info.error = "Pythonnet runtime dll not found: $pyNetRuntimeDll" }
                    } else { $Private:info.error = "Required PythonNet version is 3 or higher. Current version is $($pyNetModuleInfo.Version). Please, upgrade it manually" }
                } else { $Private:info.warning = "PythonNet is not installed. Will be installed automatically." }
            } else { $Private:info.error = "Pip is not installed. Please, refer to Python documentation or to https://bootstrap.pypa.io for installation instructions" }
        } else { $Private:info.error = "Cannot get Python version ($pyExecutable)" }
    } else { $Private:info.error = "Python executable file $pyExecutable not found" }

    return $Private:info
}

function Write-PyInfo {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)]$info)

    Write-Output "Python executable path: $($info.pyExecutable)"
    if($info.pyVersion) { Write-Output "Python version: $($info.pyVersion.Major).$($info.pyVersion.Minor).$($info.pyVersion.Patch)"}
    if($info.pipVersion) { Write-Output "Pip version: $($info.pipVersion)"}
    if($info.pyHome) { Write-Output "Python HOME: $($info.pyHome)"}
    if($info.pyHome) { Write-Output "Python PATHS: $($info.pyPath)"}
    if($info.pyHome) { Write-Output "Python dll: $($info.pyDll)"}
    if($info.pyNetModuleInfo) { Write-Output "Python.Net installed. Version: $($info.pyNetModuleInfo.Version)"}
    if($info.pyNetRuntimeDll) { Write-Output "Python.Net runtime dll: $($info.pyNetRuntimeDll)"}
    if($info.pyLedgerModuleInfo) { Write-Output "Ledger Python module installed. Version: $($info.pyLedgerModuleInfo.Version)"}
    if($info.error) { Write-Output "[NOT READY] This Python deployment cannot be configured (manual actions are probably needed): $($info.error)"}
    else {
        if($info.warning) { Write-Output "[WARNING] $($info.warning)"; Write-Output "[READY] This Python can be configured for NLedger."}
        else { Write-Output "[READY] This Python is ready for NLedger" }
    }
}

function Get-StatusInfo {
    [CmdletBinding()]
    Param()

    $Private:info = [PSCustomObject]@{
        settings = (Get-PythonIntegrationSettings)
        pyInfo = $null
        extensionProvider = ""
        enabled = $false
        validated = $false
        message = ""
    }

    if ($Private:info.settings) { $Private:info.pyInfo = Get-PyInfo -pyExecutable $Private:info.settings.PyExecutable }

    $Private:info.extensionProvider = ($(GetConfigData).Settings | Where-Object { $_.Name -eq "ExtensionProvider" } | Select-Object -First 1).EffectiveSettingValue
    $Private:info.enabled = $Private:info.extensionProvider -eq "python"

    if ($Private:info.enabled) {
        if (!$Private:info.settings) { $Private:info.message = "Python extension is enabled but connection settings file does not exist or unusable ($Script:settingsFileName). Use 'link' command to actualize settings." }
        else {
            if ($Private:info.pyInfo.error) { $Private:info.message = "Python extension is enabled but connection settings point at unusable Python (Error: $($Private:info.pyInfo.error)). Use 'link' command to actualize settings." }
            else { 
                if (!$Private:info.pyInfo.pyNetModuleInfo) { $Private:info.message = "Python extension is enabled but connected Python does not have PythonNet module"}
                else  {
                    if (!$Private:info.pyInfo.pyLedgerModuleInfo) { $Private:info.message = "Python extension is enabled but connected Python does not have Ledger module"}
                    else { $Private:info.validated = $true }
                }                
            }
        }
    }

    return $Private:info
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

function Discover {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$path)
  
    if($path) {
      Write-Output "Testing Python by path: $path"
      Write-PyInfo (Get-PyInfo -pyExecutable $path)
      
    } else {
        Write-Verbose "Search local Python"
        $path = Search-PyExecutable
        if ($path) {
            Write-Output "[Local Python] Found by path: $path"
            Write-PyInfo (Get-PyInfo -pyExecutable $path)
        } else { Write-Output "[Local Python] Not found"}
        Write-Output ""
  
        Write-Verbose "Search embed Python"
        $embeds = Search-PyEmbedInstalled -appPrefix $appPrefix -pyPlatform $pyPlatform -fullPath
        if (!$embeds) { Write-Output "[Embed Python] $(if($Script:isWindowsPlatform){"Not found"}else{"Not available"})";Write-Output "" }
        $embeds | ForEach-Object {
            Write-Output "[Embed Python] Found by path: $_"
            Write-PyInfo (Get-PyInfo -pyExecutable $_)
            Write-Output ""
        }
  
        Write-Verbose "Check NLedger Python settings"
        $settings = Get-PythonIntegrationSettings
        if($settings) {
            Write-Output "[NLedger Settings] Found ($Script:settingsFileName); configured Python: $($settings.PyExecutable)"
            Write-PyInfo (Get-PyInfo -pyExecutable $settings.PyExecutable)
        } else { Write-Output "[NLedger Settings] No found (no file $Script:settingsFileName)" }
        Write-Output ""
    }  
}

function Link {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)][string]$path,
        [Parameter(Mandatory=$False)][string]$embed
    )

    if ($path -and $embed) { throw "Link command cannot get both 'path' and 'embed' parameters at the same time."}

    $Private:settings = Get-PythonIntegrationSettings

    if (!$path -and !$embed) {
        Write-Verbose "Auto-configuring Link parameters. Checking settings first."
        if ($Private:settings) {
            $path = $settings.PyExecutable
            Write-Output "Auto-linking: use path '$path' specified in NLedger Python settings ($Script:settingsFileName)"
        } else {
            Write-Verbose "No settings file. Searching local Python"
            $path = Search-PyExecutable
            if (!$path) {
                if ($Script:isWindowsPlatform) {
                    $embed = $pyEmbedVersion
                    Write-Output "Auto-linking: use embed Python $pyEmbedVersion (since local Python not found and no settings file)"
                } else { throw "Auto-linking is not possible: local Python not found and no settings file. Specify 'path' parameter." }
            }
        }        
    }

    if ($embed) {
        Write-Verbose "Installing embedded python $embed"
        $path = Search-PyExecutable -pyHome (Get-PyEmbed -appPrefix $appPrefix -pyVersion $embed -pyPlatform $pyEmbedPlatform)
        Write-Output "Link: installed embed Python $embed by path $path"
    }

    $Private:info = Get-PyInfo -pyExecutable $path
    if ($Private:info.error) { throw "Linking error: cannot use Python by path $path (Error: $($Private:info.error))" }

    if (!$Private:info.pyNetModuleInfo) {
        Write-Verbose "PythonNet not installed, installing..."
        $null = Install-PyModule -pyExe $path -pyModule $pyNetPath

        $Private:info = Get-PyInfo -pyExecutable $path
        if ($Private:info.error -or !$Private:info.pyNetModuleInfo) { throw "Linking error: cannot install PythonNet (Error: $($Private:info.error))" }
        Write-Output "PythonNet has been installed"
    }

    Write-Verbose "Update or create settings"
    if (!$settings -or ($settings.PyExecutable -ne $Private:info.pyExecutable -or $settings.PyHome -ne $Private:info.pyHome -or $settings.PyPath -ne $Private:info.pyPath -or $settings.PyDll -ne $Private:info.pyDll -or $settings.PyNetRuntimeDll -ne $Private:info.pyNetRuntimeDll)) {
        Write-Verbose "Settings file is outdated"
        $settings = Update-PythonEnvironmentSettings -pyExecutable $Private:info.pyExecutable -pyHome $Private:info.pyHome -pyPath $Private:info.pyPath -pyDll $Private:info.pyDll -pyNetRuntimeDll $Private:info.pyNetRuntimeDll
        Write-Output "Settings file ($Script:settingsFileName) is updated"
    } else { Write-Verbose "Settings file $Script:settingsFileName is actual"}

    Write-Output "NLedger Python link is active (Settings file: $Script:settingsFileName)"
}

function Status {
    [CmdletBinding()]
    Param()

    $Private:info = Get-StatusInfo
    Write-Output "NLedger Python extension: $(if($Private:info.enabled){'[ENABLED]'}else{'[DISABLED]'}) ('ExtensionProvider' setting value in NLedger configuration: '$($Private:info.extensionProvider)')"
    if ($Private:info.enabled -and !$Private:info.validated) { Write-Output "[WARNING] NLedger Python extension is unusable: $($Private:info.message)" }
    Write-Output "NLedger Python connection settings: $(if($Private:info.settings){'[EXISTS]'}else{'[DO NOT EXIST]'}) File: $Script:settingsFileName"
    if ($Private:info.settings) {
        Write-Output "Configured Python path: $($Private:info.pyInfo.pyExecutable)"
        if ($Private:info.pyInfo.error) { Write-Output "Python deployment status: [ERROR] - $($Private:info.pyInfo.error)" }
        else {
            if ($Private:info.pyInfo.warning) { Write-Output "Python deployment status: [WARNING] - $($Private:info.pyInfo.warning)" }
            else { Write-Output "Python deployment status: [OK]" }
        }
    }
    Write-Output ""
}

function Enable {
    [CmdletBinding()]
    Param()
    
    $Private:info = Get-StatusInfo
    if ($Private:info.extensionProvider -and $Private:info.extensionProvider -ne "python") {throw "Cannot enable Python extension because 'ExtensionProvider' setting value in NLedger configuration already configured for another provider: $($Private:info.extensionProvider)"}

    Link

    Install-LedgerModule
    Write-Output "Ledger module is re-installed."

    if ($Private:info.extensionProvider -ne "python") { $null = SetConfigValue -scope "user" -settingName "ExtensionProvider" -settingValue "python" }

    $Private:info = Get-StatusInfo
    if (!$Private:info.validated) { Write-Output "Cannot enable Python extension; validation message: $($Private:info.message)" }
    else { Write-Output "NLedger Python extension is enabled" }
}

function Disable {
    [CmdletBinding()]
    Param()
    
    $Private:info = Get-StatusInfo
    if ($Private:info.extensionProvider -and $Private:info.extensionProvider -ne "python") {throw "Cannot disable extension because 'ExtensionProvider' setting value in NLedger configuration already configured for another provider: $($Private:info.extensionProvider)"}

    if ($Private:info.extensionProvider -eq "python") { $null = RemoveConfigValue -scope "user" -settingName "ExtensionProvider" }

    $Private:info = Get-StatusInfo
    if ($Private:info.enabled) { Write-Output "Cannot disable extension; use NLedger Configuration console to sort out the issue manually" }
    else { Write-Output "NLedger Python extension is disabled" }
}

function Install {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$version = $pyEmbedVersion)
    Write-Output "Embedded Python is available by path: $(Get-PyEmbed -appPrefix $appPrefix -pyVersion $version -pyPlatform $pyPlatform)"
}
  




if (!$command) {
    Write-Output "Print promt and help here"
    return
}

if ($command -eq 'discover') {
    if ($embed) { throw "'embed' parameter is not allowed for 'discover' command" }
    Discover -path $path
}

if ($command -eq 'link') {
    if ($path -and $embed) { throw "'link' command can be specified with either 'path' or 'embed' parameter but noth both at the same time" }
    if ($embed) {Link -embed $embed} else {
        if ($path) {Link -path $path} else { Link }
    }
}

if ($command -eq 'status') {
    if ($path -and $embed) { throw "'status' command cannot be specified with either 'path' or 'embed' parameter" }
    Status
}

if ($command -eq 'enable') {
    if ($path -and $embed) { throw "'enable' command cannot be specified with either 'path' or 'embed' parameter" }
    Enable
}

if ($command -eq 'disable') {
    if ($path -and $embed) { throw "'disable' command cannot be specified with either 'path' or 'embed' parameter" }
    Disable
}

if ($command -eq 'install') {
    if ($path) { throw "'path' parameter is not allowed for 'install' command" }
    if ($embed) {Install -version $embed}else{Install-Embed}
}


##############################
return

if (!(Test-Path -LiteralPath $pyNetPath -PathType Leaf)) {throw "Fatal: '$pyNetPath' not found"}

## Main routince

$settings = Get-PythonIntegrationSettings
if ($settings) {
    $pyExecutable = $settings.PyExecutable
    Write-Verbose "Settings file exists ($(Get-PythonIntegrationSettingsFileName)). pyExecutable is set to $pyExecutable"
} else {
    if ($pyExecutable) {
        Write-Verbose "pyExecutable is specified as input parameter: $pyExecutable"
    } else {
        Write-Verbose "pyExecutable is not specified."
        if ($preferEmbedded -and $Script:isWindowsPlatform) {
            Write-Verbose "Installing embedded python $pyEmbedVersion"
            $pyExecutable = Search-PyExecutable -pyHome (Get-PyEmbed -appPrefix $appPrefix -pyVersion $pyEmbedVersion -pyPlatform $pyEmbedPlatform)
            Write-Verbose "Embed Python is installed; pyExecutable is: $pyExecutable"
        } else {
            Write-Verbose "Looking for local Python"
            $pyExecutable = Search-PyExecutable
            if (!$pyExecutable) {throw "Python not found (neither 'py' nor 'python' nor 'python3' commands work)"}
            Write-Verbose "Local Python found; pyExecutable is: $pyExecutable"
        }
    }
}

$pyVersion = Get-PyExpandedVersion -pyExe $pyExecutable
if(!$pyVersion) {throw "Cannot get Python version ($pyExecutable)"}
Write-Verbose "Python version: $pyVersion"

$pipVersion = Get-PipVersion -pyExe $pyExecutable
if(!$pipVersion) {throw "Pip is not installed. Please, install it to continue integration setup (see documentation if you have any questions)"}
Write-Verbose "Pip version: $pipVersion"

$pyHome = Split-Path $pyExecutable
Write-Verbose "Found pyHome: $pyHome"
$pyPath=[String]::Join(";", $(Get-PyPath -pyExecutable $pyExecutable))
Write-Verbose "Found pyPath: $pyPath"
$pyDll="python$($pyVersion.Major)$($pyVersion.Minor)"
Write-Verbose "Found pyDll: $pyDll"

$pyNetModuleInfo = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyNetModule
if(!$pyNetModuleInfo) {
    Write-Verbose "PythonNet is not installed; installing $pyNetPath"
    $null = Install-PyModule -pyExe $pyExecutable -pyModule $pyNetPath    
    $pyNetModuleInfo = Test-PyModuleInstalled -pyExe $pyExecutable -pyModule $pyNetModule
    if (!$pyNetModuleInfo) {throw "Pythonnet installation error"}
    Write-Verbose "Installed PythonNet version is $($pyNetModuleInfo.Version)"
} else {
    if($pyNetModuleInfo.Version -match "^(?<major>\d+)\.") {
        [int]$pyNetMajorVersion = $Matches["major"]
            if($pyNetMajorVersion -lt 3) {throw "Required PythonNet version is 3 or higher. Current version is $($pyNetModuleInfo.Version)"}
            Write-Verbose "Current PythonNet version is $($pyNetModuleInfo.Version)"
    } else {throw "Incorrect package version $($pyNetModuleInfo.Version)"}
}

$pyNetRuntimeDll = [System.IO.Path]::GetFullPath("$($pyNetModuleInfo.Location.Trim())/pythonnet/runtime/Python.Runtime.dll")
if(!(Test-Path -LiteralPath $pyNetRuntimeDll -PathType Leaf)) {throw "Pythonnet runtime dll not found: $pyNetRuntimeDll"}
#TODO check PythonNet runtime platform (x86 vs x64)
Write-Verbose "Found pyNetRuntimeDll: $pyNetRuntimeDll"

# Update settings if necessary
if (!$settings -or ($settings.PyExecutable -ne $pyExecutable -or $settings.PyHome -ne $pyHome -or $settings.PyPath -ne $pyPath -or $settings.PyDll -ne $pyDll -or $settings.PyNetRuntimeDll -ne $pyNetRuntimeDll)) {
    Write-Verbose "Settings file is outdated"
    $settings = Update-PythonEnvironmentSettings -pyExecutable $pyExecutable -pyHome $pyHome -pyPath $pyPath -pyDll $pyDll -pyNetRuntimeDll $pyNetRuntimeDll
} else { Write-Verbose "Settings file $(Get-PythonIntegrationSettingsFileName) is actual"}

# Done, return actual Python settings
Write-Output $settings
