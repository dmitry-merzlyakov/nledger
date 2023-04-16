<#
.SYNOPSIS
    Gets access to NLedger toolkit on any platform (Windows, Linux, OSX).
.DESCRIPTION
    NLedger toolkit helps to manage NLedger deployments on the current environment.
    It covers the following areas:
        1) Installing and uninstalling NLedger (that means adding to PATH variable, creating 'ledger' hard link and some other activities);
        2) Executing tests from Ledger testing framework (that ensures that existing binaries behaves exactly as the original Ledger);
        3) Managing NLedger settings;
        4) Showing an interactive demo web application.
    Every action has a corresponded switch that needs to be specified. 
    If no switches are specified, this script just prints a help text.
.PARAMETER install
    Installs NLedger binaries on the current environment (adds to PATH variable and creates 'ledger' hard link).
    If the current deployment contains binaries for both platforms (.Net Framework and .Net Core) - installs .Net Framework binaries.
    If you run the script with administrative privileges, it will also create NGen image for .Net Framework binaries.
.PARAMETER installPreferCore
    Installs NLedger binaries but prefers .Net Core binaries if there is a choice.
.PARAMETER uninstall
    Uninstalls NLedger (removes from PATH, deletes 'ledger' alias and deletes NGen image for .Net Framework).
    If other NLedger installations are detected in PATH, they are removed as well.
.PARAMETER installConsole
    Runs NLedger Install console where you can install or uninstall NLedger by means of interactive commands.
.PARAMETER testConsole
    Runs NLedger Test Toolkit interactive console.
.PARAMETER settings
    Runs NLedger Setup interactive console that allows to manage settings on the local environment (app, user and common scope).
.PARAMETER pythonConnect
    Creates Python Extension settings file.
.PARAMETER pythonTools
    Runs Python Toolset console that allows to manage Python Extension settings on the local environment.
.PARAMETER demo
    Runs NLedger demo web application where you can observe documentation and interatively run described commands.
    GUI is needed (since this demo runs a browser).
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -install
    Installs NLedger binaries (prefers .Net Framework is there is choice).
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -installPreferCore
    Installs NLedger binaries (prefers .Net Core is there is choice).
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -uninstall
    Uninstalls NLedger from the system.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -installConsole
    Runs NLedger Install interactive console where you can check installation status and install or uninstall NLedger.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -testConsole
    Runs NLedger Test Toolkit interactive console where you can execute ledger tests for current binaries.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -pythonConnect
    Creates Python Extension settings file that specifies where local Python is installed.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -pythonTools
    Runs Python Toolset console where you can change or remove Python Extension settings.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -settings
    Runs NLedger Setup interactive console where you can manage local NLedger settings.
.EXAMPLE
    PS> ./get-nledger-tools.ps1 -demo
    Runs NLedger demo application.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   September 09, 2020    
    ==
    Run the script on Windows: >powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-tools.ps1
    Run the script on other OS: >powershell -File ./get-nledger-tools.ps1
#>    

[CmdletBinding()]
Param(
    [Switch][bool]$install = $False,
    [Switch][bool]$installPreferCore = $False,
    [Switch][bool]$uninstall = $False,
    [Switch][bool]$installConsole = $False,
    [Switch][bool]$testConsole = $False,
    [Switch][bool]$settings = $False,
    [Switch][bool]$pythonConnect = $False,
    [Switch][bool]$pythonTools = $False,
    [Switch][bool]$demo = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
[string]$Script:powershell = $(if($Script:isWindowsPlatform){"powershell"}else{"pwsh"})

if ($install) {
    & $Script:powershell -File $("$Script:ScriptPath/Contrib/Install/NLedger.Install.ps1") -install
    return
}

if ($installPreferCore) {
    & $Script:powershell -File $("$Script:ScriptPath/Contrib/Install/NLedger.Install.ps1") -installPreferCore
    return
}

if ($uninstall) {
    & $Script:powershell -File $("$Script:ScriptPath/Contrib/Install/NLedger.Install.ps1") -uninstall
    return
}

if ($installConsole) {
    & $Script:powershell -NoExit -File $("$Script:ScriptPath/Contrib/Install/NLedger.Install.ps1")
    return
}

if ($testConsole) {
    & $Script:powershell -NoExit -File $("$Script:ScriptPath/Contrib/NLTestToolkit/NLTest.Launcher.ps1")
    return
}

if ($settings) {
    & $Script:powershell -NoExit -File $("$Script:ScriptPath/Contrib/NLManagement/NLSetup.Console.ps1")
    return
}

if ($pythonConnect) {
    & $Script:powershell -File $("$Script:ScriptPath/Contrib/Python/GetPythonEnvironment.ps1") -command connect
    return
}

if ($pythonTools) {
    & $Script:powershell -NoExit -File $("$Script:ScriptPath/Contrib/Python/GetPythonEnvironment.ps1")
    return
}

if ($demo) {
    & $Script:powershell -File $("$Script:ScriptPath/Contrib/NLManagement/NLDoc.LiveDemo.WebConsole.ps1")
    return
}

Write-Host -ForegroundColor White "*** NLedger toolkit launcher ***"
Write-Host
Write-Host "Use one of the following switches:"
Write-Host
Write-Host -NoNewline -ForegroundColor Yellow "-install           "
Write-Host "Installs NLedger binaries. Prefers .Net Framework if it is available."
Write-Host -NoNewline -ForegroundColor Yellow "-installPreferCore "
Write-Host "Installs NLedger binaries. Prefers .Net Core."
Write-Host -NoNewline -ForegroundColor Yellow "-uninstall         "
Write-Host "Uninstalls NLedger binaries."
Write-Host -NoNewline -ForegroundColor Yellow "-installConsole    "
Write-Host "Runs interactive installer."
Write-Host -NoNewline -ForegroundColor Yellow "-testConsole       "
Write-Host "Runs NLedger Test Toolkit console."
Write-Host -NoNewline -ForegroundColor Yellow "-settings          "
Write-Host "Runs NLedger settings manager."
Write-Host -NoNewline -ForegroundColor Yellow "-pythonConnect     "
Write-Host "Creates Python Extension settings file."
Write-Host -NoNewline -ForegroundColor Yellow "-pythonTools       "
Write-Host "Runs Python Toolset console that manages Python Extension settings."
Write-Host -NoNewline -ForegroundColor Yellow "-demo              "
Write-Host "Runs NLedger interactive demo. Requires GUI."
