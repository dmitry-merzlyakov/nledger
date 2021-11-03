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
.PARAMETER coreOnly
    Orders to create .Net Core binaries only. By default, the script creates both Framework
    and Core binaries. This switch is selected automatically on non-Windows platforms.
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
    PS> ./get-nledger-up.ps1 -coreOnly
    Create .Net Core binaries only.
    Note: this switch is set automatically on non-windows platforms.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   October 19, 2021
    ===
    Run the script on Windows: >powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1
    Run the script on other OS: >powershell -File ./get-nledger-up.ps1
#>
[CmdletBinding()]
Param(
    [Switch][bool]$coreOnly = $False,
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
[Version]$minDotnetVersion = "3.1"

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Initialization"

# Check environmental information

[bool]$isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
[bool]$isOsxPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
Write-Verbose "Detected: is windows platform=$isWindowsPlatform; is OSX platform: $isOsxPlatform"

[bool]$isDotNetInstalled = -not(-not($(get-command dotnet -ErrorAction SilentlyContinue)))
if (!$isDotNetInstalled) { throw "DotNet Core is not installed (command 'dotnet' is not available)" }

[Version]$dotnetVersion = $(dotnet --version)
Write-Verbose "Detected: dotnet version=$dotnetVersion"
if ($dotnetVersion -lt $minDotnetVersion) { throw "Detected dotnet version is $dotnetVersion but minimal required is $minDotnetVersion" }

# Validate switchers

if (!$isWindowsPlatform -and !$coreOnly) {
    $coreOnly = $true
    Write-Verbose "Since it is not windows platform, switch 'coreOnly' is changed to 'True'."
}

# Check codebase structure

[string]$solutionPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Source/NLedger.sln")
Write-Verbose "Expected solution path: $solutionPath"
if (!(Test-Path -LiteralPath $solutionPath -PathType Leaf)) { "File '$solutionPath' does not exist. Check that source code base is in valid state." }

[string]$nlTestPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Contrib/NLTestToolkit/NLTest.ps1")
Write-Verbose "Expected NLTest path: $nlTestPath"
if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) { "File '$nlTestPath' does not exist. Check that source code base is in valid state." }

# Check Python connection availability

[string]$powershell = $(if($Script:isWindowsPlatform){"powershell"}else{"pwsh"})

if (!$noPython) {
    Write-Verbose "Checking NLedger Python extension settings..."
    
    [string]$testConnectionOutput = & $powershell -File $("$Script:ScriptPath/Contrib/Python/GetPythonEnvironment.ps1") -command "test-connection"
    Write-Verbose "GetPythonEnvironment's test-connection returned: $testConnectionOutput"
    $pythonConnectionStatus = $(if($testConnectionOutput){[System.Management.Automation.PSSerializer]::Deserialize($testConnectionOutput)}else{$null})
    
    if (!($pythonConnectionStatus.IsConnectionValid)){
        Write-Warning "NLedger Python Extension settings are not configured or not valid. This does not prevent the build from completing, but unit and integration tests that require Python will be skipped. Building Ledger module will be skipped. Use './get-nledger-tools.ps1 -pythonTools' command to configure settings or disable this warning by adding a switch './get-nledger-up.ps1 -noPython'."
        $noPython = $True
    }
}
if ($noPython){$env:NLedgerPythonConnectionStatus = "Disabled"}

# First step: build sources

Write-Progress -Activity "Building, testing and installing NLedger" -Status "Building source code [$(if($coreOnly){".Net Core"}else{".Net Framework,.Net Core"})] [$(if($debugMode){"Debug"}else{"Release"})]"

[string]$buildCommandLine = "dotnet build '$solutionPath'"
if ($coreOnly) { $buildCommandLine += " /p:CoreOnly=True"}
if ($debugMode) { $buildCommandLine += " --configuration Debug"} else { $buildCommandLine += " --configuration Release" }
if ($isOsxPlatform) { $buildCommandLine += " -r osx-x64"}
Write-Verbose "Build sources command line: $buildCommandLine"

$null = (Invoke-Expression $buildCommandLine | Write-Verbose)
if ($LASTEXITCODE -ne 0) { throw "Build failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }

# Second step: run unit tests

if(!$noUnitTests) {
    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running unit tests [$(if($coreOnly){".Net Core"}else{".Net Framework,.Net Core"})] [$(if($debugMode){"Debug"}else{"Release"})]"

    [string]$unittestCommandLine = "dotnet test '$solutionPath'"
    if ($coreOnly) { $unittestCommandLine += " /p:CoreOnly=True"}
    if ($debugMode) { $unittestCommandLine += " --configuration Debug"} else { $buildCommandLine += " --configuration Release" }
    if ($isOsxPlatform) { $buildCommandLine += " -r osx-x64"}
    Write-Verbose "Run unit tests command line: $unittestCommandLine"

    $null = (Invoke-Expression $unittestCommandLine | Write-Verbose)
    if ($LASTEXITCODE -ne 0) { throw "Unit tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

# Third step: run integration tests

function composeNLedgerExePath {
    Param([Parameter(Mandatory=$True)][bool]$core)
    [string]$private:config = $(if($debugMode){"Debug"}else{"Release"})
    [string]$private:framework = $(if($core){"netcoreapp3.1"}else{"net472"})
    [string]$private:osxBin = $(if($isOsxPlatform){"/osx-x64"})
    [string]$private:extension = $(if($isWindowsPlatform){".exe"}else{""})
    return [System.IO.Path]::GetFullPath("$Script:ScriptPath/Source/NLedger.CLI/bin/$private:config/$private:framework$($private:osxBin)/NLedger-cli$private:extension")
}    

if (!$noNLTests)
{
    [string]$ignoreCategories = $(if($noPython){"python"}else{""})

    if (!$coreOnly){
        Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running integration tests [.Net Framework] [$(if($debugMode){"Debug"}else{"Release"})]"

        [string]$nledgerFrameworkExeFile = composeNLedgerExePath -core $false
        if (!(Test-Path -LiteralPath $nledgerFrameworkExeFile -PathType Leaf)) { throw "Cannot find $nledgerFrameworkExeFile" }

        Import-Module "$Script:ScriptPath/Contrib/Install/SysNGen.psm1"
        if (Test-CanCallNGen) {$null = Add-NGenImage $nledgerFrameworkExeFile}

        $null = (& $nlTestPath -nledgerExePath $nledgerFrameworkExeFile -noconsole -showProgress -ignoreCategories $ignoreCategories | Write-Verbose)
        if ($LASTEXITCODE -ne 0) { throw "Integration tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
    }

    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Running integration tests [.Net Core] [$(if($debugMode){"Debug"}else{"Release"})]"

    [string]$nledgerCoreExeFile = composeNLedgerExePath -core $true
    if (!(Test-Path -LiteralPath $nledgerCoreExeFile -PathType Leaf)) { throw "Cannot find $nledgerCoreExeFile" }

    $null = (& $nlTestPath -nledgerExePath $nledgerCoreExeFile -noconsole -showProgress -ignoreCategories $ignoreCategories | Write-Verbose)
    if ($LASTEXITCODE -ne 0) { throw "Integration tests failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

if (!$noPython){
    Write-Progress -Activity "Building, testing and installing NLedger" -Status "Building Python Ledger module"
    if(!($pythonConnectionStatus.IsPythonNetInstalled)){Write-Warning "PythonNet is not installed on the connected Python environment. Ledger module will be built but not tested. It is not a critical problem since module code is tested by Ledger unit tests."}

    [string]$pyBuildPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/Source/NLedger.Extensibility.Python.Module/build.ps1")
    Write-Verbose "Expected Python Ledger module build script path: $pyBuildPath"
    if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) { "File '$pyBuildPath' does not exist. Check that source code base is in valid state." }

    $( $(& "$pyBuildPath" -build -test:$pythonConnectionStatus.IsPythonNetInstalled) 2>&1 | Out-String ) | Write-Verbose     
    if ($LASTEXITCODE -ne 0) { throw "Python module build failed for some reason. Run this script again with '-Verbose' to get more information about the cause." }
}

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

if (!($noPython)) {
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
