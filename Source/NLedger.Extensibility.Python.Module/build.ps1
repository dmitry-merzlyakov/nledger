<#
.SYNOPSIS
    Builds NLedger Python package

.DESCRIPTION
    Helper script that is responsible to build NLedger Python package.
    Basically, it builds the package using pre-built .Net binaries and puts the output "wheel" file into Contrib/Python folder.
    If build parameters are not specified explicitely, it discovers necessary information on the current environment.

.PARAMETER pyExecutable
    Fully-qualified path to Python executable file.
    Optional. If this parameter is ommitted, the script tries to find it in 
    the local NLedger Python connection setting file ([LocalApplicationData]/NLedger/NLedger.Extensibility.Python.settings.xml)

.PARAMETER packageBinaryFiles
    Semicolon-separated list of .Net binary files need to be included into the package.
    Optional. If this parameter is ommitted, the script tries to find .Net binaries in NLedger.Extensibility.Python bin folder
    (it will take the latets biinaries either in Debug or Release folder)

.PARAMETER packageVersion
    Three-digit package version.
    Optional. If this parameter is ommitted, the script will extract the numbers from .Net binary file.

.PARAMETER targetFolder
    Three-digit package version.
    Optional. If this parameter is ommitted, the script will put the output file into Contrib/Python folder.

.PARAMETER test
    Switch that runs Python unit tests before build

.PARAMETER build
    Switch that starts build

Note: use 'set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process' to run the script in dev terminal
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$pyExecutable,
    [Parameter(Mandatory=$False)][string]$packageBinaryFiles,
    [Parameter(Mandatory=$False)][string]$packageVersion,
    [Parameter(Mandatory=$False)][string]$targetFolder,
    [Switch][bool]$test = $False,
    [Switch][bool]$build = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

Write-Verbose "Validate input parameters"

if (!$pyExecutable) {
    Write-Verbose "Parameter pyExecutable is not specified; extracting it from Python Connection file"

    [string]$Script:localAppData = [Environment]::GetFolderPath("LocalApplicationData")
    [string]$Script:settingsFileName = [System.IO.Path]::GetFullPath($(Join-Path $Script:localAppData "NLedger/NLedger.Extensibility.Python.settings.xml"))
    if (!(Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf)) { throw "Paraneter pyExecutable is not specified and file '$($Script:settingsFileName)' not found. Either specify pyExecutable parameter or create NLedger Python connection file" }

    [xml]$Private:xmlContent = Get-Content $Script:settingsFileName
    $pyExecutable = $Private:xmlContent.'nledger-python-settings'.'py-executable'
}
Write-Verbose "pyExecutable = '$pyExecutable'"
if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) {throw "File $pyExecutable not found"}

if (!$packageBinaryFiles) {
    Write-Verbose "Parameter packageBinaryFiles is not specified; looking for pre-built binaries in NLedger.Extensibility.Python project"

    [string]$Script:binDebug = [System.IO.Path]::GetFullPath($(Join-Path $Script:ScriptPath "../NLedger.Extensibility.Python/bin/Debug/netstandard2.0"))
    [string]$Script:binRelease = [System.IO.Path]::GetFullPath($(Join-Path $Script:ScriptPath "../NLedger.Extensibility.Python/bin/Release/netstandard2.0"))

    [string]$Script:binDebugDll = [System.IO.Path]::GetFullPath($(Join-Path $Script:binDebug "NLedger.Extensibility.Python.dll"))
    [string]$Script:binReleaseDll = [System.IO.Path]::GetFullPath($(Join-Path $Script:binRelease "NLedger.Extensibility.Python.dll"))

    [datetime]$Script:binDebugTime = $(if(Test-Path -LiteralPath $Script:binDebugDll -PathType Leaf){[System.IO.File]::GetLastWriteTimeUtc($Script:binDebugDll)}else{[datetime]::MinValue})
    [datetime]$Script:binReleaseTime = $(if(Test-Path -LiteralPath $Script:binReleaseDll -PathType Leaf){[System.IO.File]::GetLastWriteTimeUtc($Script:binReleaseDll)}else{[datetime]::MinValue})

    if($Script:binDebugTime -eq [datetime]::MinValue -and $Script:binReleaseTime -eq [datetime]::MinValue) {throw ".Net binary files not found. Build .Net solution before building Python package. (Neither $Script:binDebugDll nor $Script:binReleaseTime found)"}
    [string]$Script:binFolder = $(if($Script:binDebugTime -gt $Script:binReleaseTime){$Script:binDebug}else{$Script:binRelease})

    Write-Verbose "Found folder with latest .Net binary files: $Script:binFolder"

    $Script:binaryFiles = Get-ChildItem -Path "$Script:binFolder/*.dll" | Where-Object { $_.Name.StartsWith("NLedger.") -or ($_.Name -eq "Python.Runtime.dll") } | ForEach-Object { $_.FullName }
    $packageBinaryFiles = [string]::Join(";",$Script:binaryFiles)
}
Write-Verbose "packageBinaryFiles = '$packageBinaryFiles'"

if (!$packageVersion) {
    Write-Verbose "Parameter packageVersion is not specified; extracting version from NLedger.Extensibility.Python.dll"
    [string]$Script:dllName = ($packageBinaryFiles -split ";") | Where-Object { $_.EndsWith("NLedger.Extensibility.Python.dll") } | Select-Object -First 1
    if (!(Test-Path -LiteralPath $Script:dllName -PathType Leaf)) { throw "Cannot find NLedger.Extensibility.Python.dll in binary files" }
    $Script:dllVersion = (Get-Command $Script:dllName).Version
    $packageVersion = "$($Script:dllVersion.Major).$($Script:dllVersion.Minor).$($Script:dllVersion.Build)"
}
Write-Verbose "packageVersion = '$packageVersion'"

if (!$targetFolder) {$targetFolder = [System.IO.Path]::GetFullPath($(Join-Path $Script:ScriptPath "../../Contrib/Python"))}
Write-Verbose "targetFolder = '$targetFolder'"
if (!(Test-Path -LiteralPath $targetFolder -PathType Container)) {throw "Folder $targetFolder not found"}

Write-Verbose "Validate input parameters"

[string]$Script:testFile = [System.IO.Path]::GetFullPath($(Join-Path $Script:ScriptPath "./tests/ledger_tests.py"))
if (!(Test-Path -LiteralPath $Script:testFile -PathType Leaf)) {throw "File $Script:testFile not found"}

[string]$Script:testDrewrFile = [System.IO.Path]::GetFullPath($(Join-Path $Script:ScriptPath "./tests/drewr3.dat"))
if (!(Test-Path -LiteralPath $Script:testDrewrFile -PathType Leaf)) {throw "File $Script:testDrewrFile not found"}

[string]$Script:setupPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/setup.py")
if(!(Test-Path -LiteralPath $Script:setupPath -PathType Leaf)) {throw "Package setup file $Script:setupPath does not exist"}

Write-Verbose "Process verbs"

if ($test) {
    Write-Verbose "Run Python unit tests"

    Write-Verbose "Running tests: $pyExecutable $Script:testFile"
    $null = (. $pyExecutable $Script:testFile)
    [int]$Script:testExitCode = $LastExitCode
    Write-Verbose "Test exit code: $Script:testExitCode"

    if ($Script:testExitCode -ne 0) {throw "Python unit tests failed (exit code $Script:testExitCode). Please, check the output above to sort out the issue."}
    Write-Verbose "Python unit tests passed"
}

function Remove-Folder {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$folderName)
    if(Test-Path -LiteralPath $folderName -PathType Container) {
        $null = Remove-Item -LiteralPath $folderName -Force -Recurse
        if(Test-Path -LiteralPath $folderName -PathType Container) {throw "Cannot delete folder $folderName"}
    }
}

if ($build) {
    Write-Verbose "Run Python module build"

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

    # sdist file is not needed
    $null = Copy-Item -LiteralPath $Script:wheelFile -Destination $targetFolder
    $null = Copy-Item -LiteralPath $Script:testFile -Destination $targetFolder
    $null = Copy-Item -LiteralPath $Script:testDrewrFile -Destination $targetFolder

    Write-Verbose "Build finishes"
}

if (!$test -and !$build) { Write-Warning "Neither 'test' nor 'build' switches are specified. Specify one or both switches to run testing and/or build."}
