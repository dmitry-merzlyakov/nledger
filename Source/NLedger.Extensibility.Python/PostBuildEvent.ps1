<#
.SYNOPSIS
    NLedger.Extensibility.Python post-build event
.DESCRIPTION
    Once NLedger.Extensibility.Python is built, this script checks whether Python environment is configured and build NLedger Python package
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$TargetDir
)

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

[string]$Script:pythonEnv = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Contrib/python/GetPythonEnvironment.ps1")
if(!(Test-Path -LiteralPath $Script:pythonEnv -PathType Leaf)) {throw "Python package build file $Script:pythonEnv not found"}

[string]$Script:pythonBuild = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Contrib/python/NLedgerPackage/build.ps1")
if(!(Test-Path -LiteralPath $Script:pythonBuild -PathType Leaf)) {throw "Python package build file $Script:pythonBuild not found"}

Write-Verbose "Checking python environment"

$settings = (. $Script:pythonEnv)
if (!$settings) {throw "Python environment is not properly configured (GetPythonEnvironment returns empty result)"}

Write-Verbose "Building NLedger Python package"

$packageBinaryFiles = [string]::Join(";",(Get-ChildItem -Path "$TargetDir/NLedger.*" -Exclude "*.pdb" | ForEach-Object { $_.FullName }))
$packageVersion = "0.8.3"

$null = (. $Script:pythonBuild -pyExecutable $settings.PyExecutable -packageBinaryFiles $packageBinaryFiles -packageVersion $packageVersion -installPackage)

Write-Verbose "NLedger Python package is created"
