<#
.SYNOPSIS
Builds, verifies, and optionally installs NLedger from source code.

.DESCRIPTION
This script will help you get a working NLedger from source on your computer.
It performs five main steps:
 a) creates binary files;
 b) runs unit tests;
 c) runs integration tests;
 d) builds and tests the NLedger Python module;
 d) optionally installs the generated binaries (adds them to PATH and creates a hard link to the "ledger").
If errors occur at any stage, the script provides troubleshooting information 
to help you build environment issues. Switches can regulate this process. 
The script works on any platform (Windows, Linux, OSX).

.PARAMETER targets
An optional parameter that specifies one or more target framework versions for binary files.
The targets should be provided as a semicolon-separated list of target framework moniker (TFM) codes.
(See more about TFM here: https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
If this parameters is omitted, the script tries to build three targets: net48, net6.0 and net8.0.
If any of these targets are not available on the machine, they are skipped.
User-specified targets are used as is (so you may get a build error if the corresponding SDK is not available).

.PARAMETER debugMode
Forces building binaries in Debug mode (default Release).

.PARAMETER noUnitTests
Skips xUnit tests

.PARAMETER noNLTests
Skips Ledger integration tests

.PARAMETER noPython
Skips building and testing the Python module.

.PARAMETER install
Installs NLedger and creates a hard link. If multiple targets are specified, it installs the latest one.

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
Date:   December 20, 2023

Run the script on Windows: >powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1
Run the script on other OS: >pwsh -File ./get-nledger-up.ps1
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

[string]$defaultTargets = "net48;net6.0;net8.0"

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Initialization"

function Assert-FileExists {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$fileName)

    $fileName = [System.IO.Path]::GetFullPath($fileName)
    if (!(Test-Path -LiteralPath $fileName -PathType Leaf)) { throw "File '$fileName' does not exist. Check that source code base is in valid state." }
    return $fileName
}

Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Common/SysCommon.psm1")
Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Common/SysPlatform.psm1")
Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Common/SysDotnet.psm1")
Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Common/NLedgerEnvironment.psm1")
Import-Module (Assert-FileExists "$Script:ScriptPath/Contrib/Python/NLedgerPyEnvironment.psm1")

# Check environmental information

Write-Verbose "Detected: is windows platform=$(Test-IsWindows); is OSX platform: $(Test-IsOSX)"
if (!(Test-IsDotnetInstalled)) { throw "DotNet Core is not installed (command 'dotnet' is not available)" }

# Validate switchers

if (!$targets) { 
    Write-Verbose "Getting default build targets"
    $targets = ($defaultTargets -split ";" | Where-Object { Test-HasCompatibleSdk $_ }) -join ";"
    if (!$targets) { throw "Appropriate build targets not found on the local machine. Specify 'target' parameter explicitely." }
}
Write-Verbose "Build targets: $targets"

$libraryTargets = "netstandard2.0;$(($targets -split ";" | Where-Object { !(Expand-TfmCode $_).IsFramework }) -join ";")"

Write-Verbose "Library build targets: $libraryTargets"

# Check codebase structure

[string]$solutionPath = Assert-FileExists "$Script:ScriptPath/Source/NLedger.sln"
Write-Verbose "Solution path: $solutionPath"

[string]$nlTestPath = Assert-FileExists "$Script:ScriptPath/Contrib/test/NLTest.ps1"
Write-Verbose "NLTest path: $nlTestPath"

# Check Python connection availability

if (!$noPython) {
    Write-Verbose "Checking NLedger Python extension settings..."
    $pythonConnectionStatus = Get-PythonConnectionInfo
    if (!($pythonConnectionStatus.IsConnectionValid)){
        Write-Warning "NLedger Python Extension settings are not configured or not valid. This does not prevent the build from completing, but unit and integration tests that require Python will be skipped. Building Ledger module will be skipped. Use './Contrib/nledger-tools.ps1 python-connect' command to configure settings or disable this warning by adding a switch './get-nledger-up.ps1 -noPython'."
        $noPython = $True
    }
}
if ($noPython){$env:NLedgerPythonConnectionStatus = "Disabled"}

# First step: build sources

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Building source code [$targets] [$(if($debugMode){"Debug"}else{"Release"})]"

function Get-DotnetBuildCommandLine {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$command)

    # Due to a msbuild issue, use env variable instead of parameter -p:LocalTargetFrameworks='$targets'. See https://github.com/dotnet/msbuild/issues/2999
    $env:LocalLibraryTargetFrameworks = $libraryTargets 
    $env:LocalTargetFrameworks = $targets 
    [string]"dotnet $command '$solutionPath' --configuration $(if($debugMode){"Debug"}else{"Release"}) $(if($isOsxPlatform){" -r osx-x64"})"
}

[string]$buildCommandLine = Get-DotnetBuildCommandLine "build"
Write-Verbose "Build sources command line: $buildCommandLine; Env variables LocalTargetFrameworks: $env:LocalTargetFrameworks; LocalLibraryTargetFrameworks: $env:LocalLibraryTargetFrameworks"

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

if (!$noNLTests)
{
    [string]$ignoreCategories = $(if($noPython){"python"}else{""})

    $targetBins = Select-NLedgerDeploymentInfos (Get-NLedgerDeploymentInfo) $targets $debugMode
    foreach ($binInfo in $targetBins) {
        Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running integration tests [$(Out-TfmProfile $binInfo.TfmCode $debugMode)]"
        if ((Test-IsFrameworkTfmCode $binInfo.TfmCode) -and (Test-CanCallNGen)) { $null = Add-NGenImage $binInfo.NLedgerExecutable }
        $null = (& $nlTestPath -nledgerExePath $binInfo.NLedgerExecutable -noconsole -showProgress -ignoreCategories $ignoreCategories | Write-Verbose)
        if ($LASTEXITCODE -ne 0) { throw "Integration tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
    }
}

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

    $deploymentInfo = Get-NLedgerDeploymentInfo
    $installInfo = Select-NLedgerDeploymentInfos $deploymentInfo $targets $debugMode | Select-Object -Last 1 | Assert-IsNotEmpty "Unrecoverable issue: cannot find files for installation"
    if ($deploymentInfo.EffectiveInstalled) { $null = Uninstall-NLedgerExecutable }
    $installResponse = Install-NLedgerExecutable $installInfo.TfmCode $installInfo.IsDebug -link
    if (!$installResponse.NLedgerExecutable) { throw "Installation failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Completed" -Completed

# Print summary

Write-Output "*** NLedger build completed successfully ***`n"

Write-Output "Build source code: OK [$targets]"
Select-NLedgerDeploymentInfos (Get-NLedgerDeploymentInfo) $targets $debugMode | ForEach-Object { Write-Output " > $($_.NLedgerExecutable)"}

Write-Output "Unit tests: $(if(!$noUnitTests){ "OK" }else{ "SKIPPED" })"
Write-Output "Ledger tests: $(if(!$noNLTests){ "OK" }else{ "SKIPPED" })"
Write-Output "Python module: $(if($pythonModuleBuilt){ "BUILT (see ./Contrib/Python)" }else{ "SKIPPED" })"

if ($install) { Write-Output "NLedger install: OK (Installed $($installResponse.NLedgerExecutable))`nNote: run a new console session to get effect of changes in environment variables" }
else { Write-Output "NLedger is ready to be installed (use ./Contrib/nledger-tools.ps1 install)" }
Write-Output ""
