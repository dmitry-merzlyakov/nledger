<#
.SYNOPSIS
    Build, verify and optionally install NLedger from source code.
.DESCRIPTION
    This script helps  to get workable NLedger from source code
    on their computer. It performs four basic steps: a) builds binaries;
    b) runs unit tests; c) runs integration tests; d) optionally, installs created binaries
    (adds to PATH and creates 'ledger' hard link). In case of any errors on any steps,
    the script provides troubleshooting information to help people solve environmental
    issues. Switches can regulate this process. The script works on any platform (Windows, Linux, OSX).
.PARAMETER targets
    An optional parameter that specifies one or more target framework versions for binary files.
    The targets should be provided as a semicolon-separated list of target framework moniker (TFM) codes.
    (See more about TFM here: https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
    If this parameters is omitted, the script tries to build two targets: net48 and net6.0 with two exceptions:
    - net48 target is ignored if it is not available (on non-Windows machines or if the current machine does not have .Net Framework 4,8 SDK);
    - If net6.0 SDK is not installed, the script uses the latest available dotnet SDK.
    Targets specified by the user are used as they are (so, you might get a build error if a corresponded SDK is not available).
.PARAMETER debugMode
    Orders to create binaries in Debug mode (Release by default)
.PARAMETER noUnitTests
    Omits xUnit tests
.PARAMETER noNLTests
    Omits NLTest integration tests
.PARAMETER noPython
    Disables Python unit and integration tests; Python module build is skipped
.PARAMETER install
    If this switch is set, the script installs NLedger when binaries are built and verified.
.EXAMPLE
    PS> ./get-nledger-up.ps1
    Build, verify and install NLedger from source code.
.EXAMPLE
    PS> ./get-nledger-up.ps1 -Verbose
    Show detail diagnostic information to troubleshoot build issues.
.EXAMPLE
    PS> ./get-nledger-up.ps1 -targets net5.0;net7.0
    Create binaries for net5.0 and net 7.0 targets.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   October 19, 2021
    ===
    Run the script on Windows: >powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1
    Run the script on other OS: >powershell -File ./get-nledger-up.ps1
#>
[CmdletBinding()]
Param(
    [string]$targets = "",
    [Switch][bool]$debugMode = $False,
    [Switch][bool]$noUnitTests = $False,
    [Switch][bool]$noNLTests = $False,
    [Switch][bool]$noPython = $False,
    [Switch][bool]$install = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

[string]$defaultFrameworkTarget = "net48"
[string]$defaultDotnetTarget = "net6.0"

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Initialization"

function Assert-FileExists {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$fileName)

    $fileName = [System.IO.Path]::GetFullPath($fileName)
    if (!(Test-Path -LiteralPath $fileName -PathType Leaf)) { throw "File '$fileName' does not exist. Check that source code base is in valid state." }
    return $fileName
}

Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/NLManagement/NLCommon.psm1") -Force

# Check environmental information

[bool]$isWindowsPlatform = Test-IsWindows
[bool]$isOsxPlatform = Test-IsOSX
Write-Verbose "Detected: is windows platform=$isWindowsPlatform; is OSX platform: $isOsxPlatform"

if (!(Test-IsDotnetInstalled)) { throw "DotNet Core is not installed (command 'dotnet' is not available)" }

# Validate switchers

function Get-AppropriateTarget {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]$availableTargets,
        [Parameter(Mandatory=$True)][string]$defaultTarget
    )

    if ($availableTargets -contains $defaultTarget) {$defaultTarget} else {$availableTargets | Select-Object -Last 1}
}

if (!$targets) {
    Write-Verbose "Getting default build targets"
    $targetList = @(
        Get-AppropriateTarget (Get-NetFrameworkSdkTargets) $defaultFrameworkTarget
        Get-AppropriateTarget (Get-DotnetSdkTargets) $defaultDotnetTarget
    ) | Where-Object {$_}
    if (!$targetList) { throw "Appropriate build targets not found on the local machine. Specify 'target' parameter explicitely." }
    $targets = [string]::Join(";", $targetList)
}
Write-Verbose "Build targets: $targets"

# Check codebase structure

[string]$solutionPath = Assert-FileExists "$Script:ScriptPath/Source/NLedger.sln"
Write-Verbose "Solution path: $solutionPath"

[string]$nlTestPath = Assert-FileExists "$Script:ScriptPath/Contrib/NLTestToolkit/NLTest.ps1"
Write-Verbose "NLTest path: $nlTestPath"

# Check Python connection availability

[string]$powershell = $(if($Script:isWindowsPlatform){"powershell"}else{"pwsh"})

if (!$noPython) {
    Write-Verbose "Checking NLedger Python extension settings..."
    
    [string]$testConnectionOutput = & $powershell -File $(Assert-FileExists "$Script:ScriptPath/Contrib/Python/GetPythonEnvironment.ps1") -command "test-connection"
    Write-Verbose "GetPythonEnvironment's test-connection returned: $testConnectionOutput"
    $pythonConnectionStatus = $(if($testConnectionOutput){[System.Management.Automation.PSSerializer]::Deserialize($testConnectionOutput)}else{$null})
    
    if (!($pythonConnectionStatus.IsConnectionValid)){
        Write-Warning "NLedger Python Extension settings are not configured or not valid. This does not prevent the build from completing, but unit and integration tests that require Python will be skipped. Building Ledger module will be skipped. Use './get-nledger-tools.ps1 -pythonTools' command to configure settings or disable this warning by adding a switch './get-nledger-up.ps1 -noPython'."
        $noPython = $True
    }
}
if ($noPython){$env:NLedgerPythonConnectionStatus = "Disabled"}

# First step: build sources

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Building source code [$targets] [$(if($debugMode){"Debug"}else{"Release"})]"

function Get-DotnetBuildCommandLine {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$command)

    $env:LocalTargetFrameworks = $targets # Because of msbuild issue, use env variable instead of parameter -p:LocalTargetFrameworks='$targets'. See https://github.com/dotnet/msbuild/issues/2999
    [string]"dotnet $command '$solutionPath' --configuration $(if($debugMode){"Debug"}else{"Release"}) $(if($isOsxPlatform){" -r osx-x64"})"
}

[string]$buildCommandLine = Get-DotnetBuildCommandLine "build"
Write-Verbose "Build sources command line: $buildCommandLine; Environment variable LocalTargetFrameworks: $env:LocalTargetFrameworks"

$null = (Invoke-Expression $buildCommandLine | Write-Verbose)
if ($LASTEXITCODE -ne 0) { throw "Build failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }

# Second step: run unit tests

if(!$noUnitTests) {
    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running unit tests [$targets] [$(if($debugMode){"Debug"}else{"Release"})]"

    [string]$unittestCommandLine = Get-DotnetBuildCommandLine "test"
    Write-Verbose "Run unit tests command line: $unittestCommandLine; Environment variable LocalTargetFrameworks: $env:LocalTargetFrameworks"

    $null = (Invoke-Expression $unittestCommandLine | Write-Verbose)
    if ($LASTEXITCODE -ne 0) { throw "Unit tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

# Third step: run integration tests

$targetList = $targets -split ";"
$targetBins = Get-NLedgerBinaryInfos | Where-Object { $targetList -contains $_.TfmCode -and $_.IsDebug -eq $debugMode }
if (($targetList | Measure-Object).Count -ne ($targetBins | Measure-Object).Count) { throw "Detected unrecognisable issue: the number of build targets does not equal to the number of built binaries. Please, verify verbose log and find out the cause of this issue." }

if (!$noNLTests)
{
    [string]$ignoreCategories = $(if($noPython){"python"}else{""})

    foreach ($binInfo in $targetBins) {
        Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running integration tests [$($binInfo.TfmCode)] [$(if($debugMode){"Debug"}else{"Release"})]"

        if (Test-IsFrameworkTfmCode $binInfo.TfmCode) {
            Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Install/SysNGen.psm1")
            if (Test-CanCallNGen) {$null = Add-NGenImage $binInfo.NLedgerExecutable }
        }

        $null = (& $nlTestPath -nledgerExePath $binInfo.NLedgerExecutable -noconsole -showProgress -ignoreCategories $ignoreCategories | Write-Verbose)
        if ($LASTEXITCODE -ne 0) { throw "Integration tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
    }
}


throw "stop"


# Fourth step: build Python module

if (!$noPython){
    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Building Python Ledger module"
    if(!($pythonConnectionStatus.IsWheelInstalled)){Write-Warning "Wheel is not installed on the connected Python environment. Ledger module will not be built. You can install wheel module by means of './Contrib/Python/GetPythonEnvironment.ps1 -command uninstall-wheel'."}
    else {
       if(!($pythonConnectionStatus.IsPythonNetInstalled)){Write-Warning "PythonNet is not installed on the connected Python environment. Ledger module will be built but not tested. It is not a critical problem since module code is tested by Ledger unit tests."}

       [string]$pyBuildPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Source/NLedger.Extensibility.Python.Module/build.ps1")
       Write-Verbose "Expected Python Ledger module build script path: $pyBuildPath"
       if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) { "File '$pyBuildPath' does not exist. Check that source code base is in valid state." }

       $( $(& "$pyBuildPath" -build -test:$pythonConnectionStatus.IsPythonNetInstalled) 2>&1 | Out-String ) | Write-Verbose     
       if ($LASTEXITCODE -ne 0) { throw "Python module build failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
       $pythonModuleBuilt = $True
    }
}

# Fifth step: install built binaries

if ($install) {    
    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Installing NLeger binaries"
    $null = (& $powershell -File $("$Script:ScriptPath/Contrib/Install/NLedger.Install.ps1") -install | Write-Verbose)
    if ($LASTEXITCODE -ne 0) { throw "Installation failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Completed" -Completed

# Print summary

Write-Host "*** NLedger Build succeeded ***"
Write-Host

Write-Host "Build source code: OK [$(if($coreOnly){".Net Core"}else{".Net Framework,.Net Core"})] [$(if($debugMode){"Debug"}else{"Release"})]"
if (!($coreOnly)) {Write-Host "   .Net Framework binary: $(composeNLedgerExePath -core $false)"}
Write-Host "   .Net Core binary: $(composeNLedgerExePath -core $true)"
Write-Host

if (!($noUnitTests)) {
    Write-Host "Unit tests: OK [$(if($coreOnly){".Net Core"}else{".Net Framework,.Net Core"})] [$(if($debugMode){"Debug"}else{"Release"})]"
} else {
    Write-Host "Unit tests: IGNORED"
}
Write-Host

if (!($noNLTests)) {
    Write-Host "NLedger tests: OK [$(if($coreOnly){".Net Core"}else{".Net Framework,.Net Core"})] [$(if($debugMode){"Debug"}else{"Release"})]"
} else {
    Write-Host "NLedger tests: IGNORED"
}
Write-Host

if ($pythonModuleBuilt) {
    Write-Host "Python module: BUILT (see /Contrib/Python)"
} else {
    Write-Host "Python module: IGNORED"
}
Write-Host

if ($install) {
    Write-Host "NLedger install: OK (Note: run a new console session to get effect of changes in environment variables)"
} else {
    Write-Host "NLedger is ready to be installed (use ./get-nledger-tools -install)"
}
Write-Host
