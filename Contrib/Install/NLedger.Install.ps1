<#
.SYNOPSIS
    Installs or uninstall NLedger binaries on any platform (Windows, Linux, OSX).
.DESCRIPTION
    Installing NLedger is an optional operation that may improve it's usability.
    It performs three basic actions:
      1) Adds the path to NLedger binaries to PATH variable so that you can call in any folder;
      2) Creates an alias (hard link) 'ledger' to 'NLedger-cli.exe' to let you use a typical name;
      3) For .Net Framework, it calls NGen to create a very efficient native image for NLedger binaries that
         dramatically improves performance. Note: this action requires administrative priviledges.
    If no switches are specified, this script can play a role of an interactive console where the user
    can enter available commands and check the result status. Available commands are shown when the script is run.
.PARAMETER install
    Installs NLedger binaries. If there are binaries for both platforms (.Net Framework and .Net Core), prefers .Net Framework,
.PARAMETER installPreferCore
    Installs NLedger binaries but prefers .Net Core binaries if there is a choice.
.PARAMETER uninstall
    Uninstalls NLedger (removes from PATH, deletes 'ledger' alias and deletes NGen image for .Net Framework)
.EXAMPLE
    C:\PS> .\NLedger.Install.ps1
    Runs the script as an interactive console. Help script is shown and the user can enter commands.
.EXAMPLE
    C:\PS> .\NLedger.Install.ps1 -uninstall
    Uninstalls NLedger from the system.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   December 21, 2017    
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    

[CmdletBinding()]
Param(
    [Switch][bool]$install = $False,
    [Switch][bool]$installPreferCore = $False,
    [Switch][bool]$uninstall = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/NLInstall.psm1 -Force
Import-Module $Script:ScriptPath/../NLManagement/NLWhere.psm1 -Force

trap 
{ 
  write-error $_ 
  exit 1 
} 

function status{
    [CmdletBinding()]
    Param([Switch][bool]$details = $False)

    $Private:status = Get-NLedgerDeploymentStatus
    
    if ($details) {
        Write-Host "NLedger executable [Framework]: $(if($Private:status.FrameworkNLedger){$Private:status.FrameworkNLedger}else{"[Not found]"})"
        Write-Host "NLedger executable [Core     ]: $(if($Private:status.CoreNLedger){$Private:status.CoreNLedger}else{"[Not found]"})"
        if(!$Private:status.BinaryExist) { Write-Host "NLedger binaries not found."}
    }

    if ($details) {
        if ($Private:status.HasExternalInstalls) {
            Write-Host "Detected other NLedger deployments:"
            foreach($info in $Private:status.ExternalInstalls) { Write-Host "$(if($info.HasExe){"Has 'NLedger-cli'"})$(if($info.HasLink){"[Has 'ledger']"}) $($info.Path)"}
        }
        if ($Private:status.HasCurrentInstall) {
            Write-Host "Current binaries are installed: $(if($Private:status.CurrentInstall.HasExe){"Has 'NLedger-cli'"})$(if($Private:status.CurrentInstall.HasLink){"[Has 'ledger']"}) $($Private:status.CurrentInstall.Path)"
        } else {
            Write-Host "Current binaries are not installed"
        }
    }

    # Finalize status
    if (!$Private:status.BinaryExist) { Write-Host -ForegroundColor Yellow "[Install is not possible] No available binaries to install. Check that they are built." }
    if ($Private:status.HasExternalInstalls) { Write-Host -ForegroundColor Yellow "[Install is not possible] There are other NLedger deployments. Uninstall them first (use uninstall command)" }

    if ($Private:status.HasCurrentInstall) {
        Write-Host -ForegroundColor Yellow "[NLedger is installed] [$(if($Private:status.IsCurrentInstallCore){"Core"}else{"Framework"}) platform] [$(if($Private:status.CurrentInstall.HasLink){"Alias 'ledger'"}else{"No 'ledger' alias"})] [$([System.IO.Path]::GetFileName($Private:status.CurrentInstall.ExePath))]"
        Write-Host -ForegroundColor Yellow "Path: $($Private:status.CurrentInstall.Path)"
    } else {
        Write-Host -ForegroundColor Yellow "[NLedger is not installed] Use 'install' or 'install -core' command to install it."
    }
}

function install{
    Param([Switch][bool]$core = $False)

    $Private:status = Get-NLedgerDeploymentStatus
    if ($Private:status.HasCurrentInstall -and !$Private:status.HasExternalInstalls) { Uninstall-NLedger }

    [string]$Script:nLedgerPath = Get-NLedgerPath -preferCore:$core
    if(!($Script:nLedgerPath)) { "Cannot find NLedger binaries. Make sure that they are properly built."}
    Install-NLeger -path $([System.IO.Path]::GetDirectoryName($Script:nLedgerPath))

    Write-Host "Installation finished. It is recommended to start a new terminal session."
    status
}

function uninstall {
    param ()
    Uninstall-NLedger 

    Write-Host "Uninstalling finished. It is recommended to start a new terminal session."
    status
}

function help {
    param ()

    Write-Host "*** NLedger installer console ***"
    Write-Host
    Write-Host "Available commands: (type in Powershell console)"
    Write-Host ">install [-core]     Install NLedger binaries (add to PATH and create 'ledger' hard link)"
    Write-Host "                     Switch -core prefers Core binaries if both Core and Framework executables are available)"
    Write-Host ">uninstall           Uninstall NLedger (remove from PATH and delete a hard link)"
    Write-Host ">status [-details]   Show current deployment status"
    Write-Host ">help                Show this information again"
    Write-Host ">exit                Close the console window"
    Write-Host
}

if($install) {install}
else {
    if ($installPreferCore) {installPreferCore}
    else {
        if ($uninstall) {uninstall}
        else {
            help
            status
        }
    }
}
