# This scripts runs the installer with elevated priviledges and keeps PS console open to let people read the status of installation activities.
[CmdletBinding()]
Param(
    [Switch][bool]$uninstall = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[string]$Script:InstallScriptPath = Resolve-Path (Join-Path $Script:ScriptPath "./NLedger.Install.ps1") -ErrorAction Stop

[string]$Script:argList = "-NoExit -ExecutionPolicy RemoteSigned -File ""$Script:InstallScriptPath"" "
if ($uninstall)  { $Script:argList += " -uninstall" }
if ($VerbosePreference -eq "Continue") { $Script:argList += " -verbose" }
Write-Verbose "Command line to execute: $Script:argList"

Start-Process powershell.exe -Verb RunAs -ArgumentList ($Script:argList)