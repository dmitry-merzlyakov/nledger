# This script updates the source code files with the latest product information (product name, version, licensing).
# It takes this information from ProductInfo.xml and updates all the source code files in case of any changes.
# Use the following command if you need to enable scripts on your machine:
#   set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param([Switch]$verify = $False)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $ScriptPath\ProductInfo.psm1 -Force 

if ($verify) {
   write-host "Verifying that all NLedger source files have correct version and licensing information (as it is defined in ProductInfo.xml)"
   write-host "NO CHANGES IN FILES. If an issue is detected, you will see an error message"
} else {
   write-host "Updating all NLedger source files with the latest version and licensing information (as it is defined in ProductInfo.xml)"
   write-host "NO CHANGES IF ALL FILES ARE UP TO DATE. Otherwise, you will see the name of an updated file" 
}

Write-Output (Update-ProjectFiles -verify:$verify)
Write-Output (Update-LicenseNotes -verify:$verify)

Write-Host "Done"