# This scripts runs the installer with elevated priviledges and keeps PS console open to let people read the status of installation activities.
[CmdletBinding()]
Param(
    [Switch][bool]$uninstall = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[string]$Script:InstallScriptPath = Resolve-Path (Join-Path $Script:ScriptPath ".\NLedger.Install.ps1") -ErrorAction Stop

# Detects the deployment profile (prod, dev/debug, dev/release) and adds an appropriate path to NLedger binaries
[string]$Script:nLedgerPath = ".."
if (!(Test-Path "$Script:ScriptPath\$Script:nLedgerPath\NLedger-cli.exe" -PathType Leaf)) { 
    [string]$Script:debugNLedgerPath = "..\..\Source\NLedger.CLI\bin\Debug"
    [bool]$debugExists = Test-Path "$Script:ScriptPath\$Script:debugNLedgerPath\NLedger-cli.exe" -PathType Leaf
    [string]$Script:releaseNLedgerPath = "..\..\Source\NLedger.CLI\bin\Release"
    [bool]$releaseExists = Test-Path "$Script:ScriptPath\$Script:releaseNLedgerPath\NLedger-cli.exe" -PathType Leaf
    if (!$debugExists -and !$releaseExists) { throw "Cannot find NLedger-cli.exe" }
    if ($debugExists -and $releaseExists) {
        $debugDate = (Get-Item  "$Script:ScriptPath\$Script:debugNLedgerPath\NLedger-cli.exe").LastWriteTime
        $releaseDate = (Get-Item  "$Script:ScriptPath\$Script:releaseNLedgerPath\NLedger-cli.exe").LastWriteTime
        if ($debugDate -gt $releaseDate) { $Script:nLedgerPath = $Script:debugNLedgerPath } else { $Script:nLedgerPath = $Script:releaseNLedgerPath }
    } else { if ($debugExists) { $Script:nLedgerPath = $Script:debugNLedgerPath } else { $Script:nLedgerPath = $Script:releaseNLedgerPath } }
}
Write-Verbose "Detected NLedger binaries are: $Script:nLedgerPath"

[string]$Script:nLedgerPath = Resolve-Path (Join-Path $Script:ScriptPath $Script:nLedgerPath) -ErrorAction Stop

[string]$Script:argList = "-NoExit -ExecutionPolicy RemoteSigned -File ""$Script:InstallScriptPath"" -nledgerPath ""$Script:nLedgerPath"" "
if ($uninstall)  { $Script:argList += " -uninstall" }
if ($VerbosePreference -eq "Continue") { $Script:argList += " -verbose" }
Write-Verbose "Command line to execute: $Script:argList"

Start-Process powershell.exe -Verb RunAs -ArgumentList ($Script:argList)