# This scripts runs the installer with elevated priviledges and keeps PS console open to let people read the status of installation activities.
[CmdletBinding()]
Param(
    [Switch][bool]$uninstall = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\..\NLManagement\NLWhere.psm1 -Force

[string]$Script:InstallScriptPath = Resolve-Path (Join-Path $Script:ScriptPath ".\NLedger.Install.ps1") -ErrorAction Stop

[string]$Script:nLedgerPath = Get-NLedgerPath -preferCore:$false # Powershell install script is only for .Net Framework binaries
if (!$Script:nLedgerPath) { throw "Cannot find NLedger executable file" }
Write-Verbose "Detected NLedger binaries are: $Script:nLedgerPath"

[string]$Script:argList = "-NoExit -ExecutionPolicy RemoteSigned -File ""$Script:InstallScriptPath"" -nledgerPath ""$([System.IO.Path]::GetDirectoryName($Script:nLedgerPath))"" "
if ($uninstall)  { $Script:argList += " -uninstall" }
if ($VerbosePreference -eq "Continue") { $Script:argList += " -verbose" }
Write-Verbose "Command line to execute: $Script:argList"

Start-Process powershell.exe -Verb RunAs -ArgumentList ($Script:argList)