<#
.SYNOPSIS
    Builds NLedger Python package
.DESCRIPTION
    Helper script that is responsible to build and install NLedger Python package.
    It takes a list of biinary files and version as parameters, sends to python build, copies the created package to packages folder and (re)installs it.
    Requires path to python executable file.
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$pyExecutable,
    [Parameter(Mandatory=$True)][string]$packageBinaryFiles,
    [Parameter(Mandatory=$True)][string]$packageVersion,
    [Switch][bool]$installPackage = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
if(!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) {throw "Python executable $pyExecutable does not exist"}

[string]$Script:setupPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/setup.py")
if(!(Test-Path -LiteralPath $Script:setupPath -PathType Leaf)) {throw "Package setup file $Script:setupPath does not exist"}

Write-Verbose "Starting Ledger Python package build; pyExecutable: $pyExecutable; packageBinaryFiles: $packageBinaryFiles; packageVersion: $packageVersion; installPackage: $installPackage"

function Remove-Folder {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$folderName)
    if(Test-Path -LiteralPath $folderName -PathType Container) {
        $null = Remove-Item -LiteralPath $folderName -Force -Recurse
        if(Test-Path -LiteralPath $folderName -PathType Container) {throw "Cannot delete folder $folderName"}
    }
}

Write-Verbose "Cleaning package folder before build"

[string]$Script:buildPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/build")
$null = Remove-Folder -folderName $Script:buildPath

[string]$Script:distPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/dist")
$null = Remove-Folder -folderName $Script:distPath

[string]$Script:eggPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/src/ledger.egg-info")
$null = Remove-Folder -folderName $Script:eggPath

Write-Verbose "Build package"

$null = Push-Location -LiteralPath $Script:ScriptPath

$env:NLedgerPackageBuildBinaries = $packageBinaryFiles
$env:NLedgerPackageBuildVersion = $packageVersion

$null = $(. $pyExecutable $Script:setupPath sdist)
[string]$Script:sdistFile = (Get-ChildItem -Path "$Script:ScriptPath/dist/ledger-*.tar.gz" | Select-Object -First 1).FullName
if(!(Test-Path -LiteralPath $Script:sdistFile -PathType Leaf)) {throw "Cannot build sdist package"}
Write-Verbose "Created $Script:sdistFile"

$null = $(. $pyExecutable $Script:setupPath bdist_wheel)
[string]$Script:wheelFile = (Get-ChildItem -Path "$Script:ScriptPath/dist/ledger-*.whl" | Select-Object -First 1).FullName
if(!(Test-Path -LiteralPath $Script:wheelFile -PathType Leaf)) {throw "Cannot build wheel package"}
Write-Verbose "Created $Script:wheelFile"

$null = Pop-Location

Write-Verbose "Copy package"

Copy-Item -LiteralPath $Script:sdistFile -Destination "$Script:ScriptPath/../packages"
Copy-Item -LiteralPath $Script:wheelFile -Destination "$Script:ScriptPath/../packages"

if ($installPackage) {

    Write-Verbose "Installing created package"

    $null = $(. $pyExecutable -m pip uninstall ledger -y) 2>&1
    [string]$outp = $(. $pyExecutable -m pip show ledger) 2>&1
    if ($outp -notmatch "not found: ledger") {throw "Cannot uninstall the old package"}

    $null = $(. $pyExecutable -m pip install $Script:wheelFile) 2>&1
    [string]$outp = $(. $pyExecutable -m pip show ledger) 2>&1
    if ($outp -match "not found: ledger") {throw "Cannot install created package"}

    Write-Verbose "Package is re-installed"
}
