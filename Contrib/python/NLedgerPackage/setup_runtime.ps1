<#
.SYNOPSIS
    Prepares dotnet library for including into Python package
.DESCRIPTION
    Helper script for Python package build. Checks whether dotnet library is built; runs dotnet build otherwise
    (It is basically expected that all binaries are already built as part of general dotnet build).
    Returns library version extracted from dotnet binaries and list of binary files.
#>

[CmdletBinding()]
Param()

trap 
{ 
  write-error $_ 
  exit 1 
}

# Default dotnet build target is Release until it is specified in env variable
[string]$Script:target = $(if($env:debugTarget -eq "Debug"){"Debug"}else{"Release"})

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[string]$Script:solutionFolder = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../dotNet/MyPyTestSolution")
if(!(Test-Path -LiteralPath $Script:solutionFolder -PathType Container)) {throw "Solutioin folder $Script:solutionFolder does not exist"}
[string]$Script:solutionFile = [System.IO.Path]::GetFullPath("$Script:solutionFolder/MyPyTestSolution.sln")
if(!(Test-Path -LiteralPath $Script:solutionFile -PathType Leaf)) {throw "Solutioin file $Script:solutionFile does not exist"}
[string]$Script:binaryFolder = [System.IO.Path]::GetFullPath("$Script:solutionFolder/MyPyTestConsole/bin/$($Script:target)/netcoreapp3.1")

$Script:binaryFileNames = @( "MyPyTestLib.dll", "MyPyTestConsole.exe", "MyPyTestConsole.dll", "MyPyTestConsole.deps.json", "MyPyTestConsole.runtimeconfig.json")
$Script:binaryFilePaths = $Script:binaryFileNames | ForEach-Object { [System.IO.Path]::GetFullPath("$Script:binaryFolder/$_") }

if (($Script:binaryFilePaths | Test-Path -PathType Leaf) -contains $false) {
    Write-Verbose "Some of binary files not found. Running dotnet build (target $Script:target)"
    $null = $(. dotnet build $Script:solutionFile --configuration $Script:target)
    if (($Script:binaryFilePaths | Test-Path -PathType Leaf) -contains $false) { throw "dotnet library files not found" }
}

[string]$Script:libVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo([System.IO.Path]::GetFullPath("$Script:binaryFolder/MyPyTestLib.dll")).FileVersion
if(!$Script:libVersion) {throw "Library version not found"}

Write-Output $Script:libVersion
$Script:binaryFilePaths | ForEach-Object { Write-Output $_ }
