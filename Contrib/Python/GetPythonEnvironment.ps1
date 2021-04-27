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
    [Parameter(Mandatory=$False)][string]$pyExecutable,
    [Parameter(Mandatory=$False)][string]$pyEmbedVersion = "3.8.8",
    [Switch][bool]$preferEmbedded = $False,
    [Switch][bool]$configurePython = $False,
    [Switch][bool]$settingsUpdate = $False
)

# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\PyManagement.psm1 -Force
Import-Module $Script:ScriptPath\Settings.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

## Default settings

$appPrefix = "NLedger"
$pyEmbedPlatform = "amd64"
$pyNetModule = "PythonNet"
$pyNetPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Packages/pythonnet-3.0.0.dev1-py3-none-any.whl")

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
