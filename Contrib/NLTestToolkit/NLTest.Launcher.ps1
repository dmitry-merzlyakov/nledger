<#
.SYNOPSIS
Helper script that simplifies running Ledger tests in the current deployment structure.

.DESCRIPTION
This script provides an easy way to run Ledger tests that are located in the current deployment structure.
Basically, it helps with the following:
- It automatically finds and selects NLedger binaries in the current structure and allows switch between them;
- It provides short laconic commands to run tests;
- It can run commands specified as script parameters;
- If command parameter is not specified, it provides an interactive console where the user can run commands and review results.

Note: testing framework can also be run directly by the command './nltest.ps1 ARGUMENTS' (see detail information by 'get-help ./nltest.ps1'.
This script does the same thing, but in a more user-friendly way.

There are three basic commands:
- "run": run all or specified tests and create an HTML report. If GUI is available, the report will be shown in the browser.
- "xrun": run all or specified tests and show results just in teh console.
- "all": run all or specified tests including whose that are disabled by the ignore list and show result in the console.

You can specify a subset of tests by "criteria" parameter. The parameter should contain a regex pattern that is used to select tests with matched names.

Hint: see more information in the interactive console help.

.PARAMETER command
Command to execute. You can get detail information for every command by typing 'get-help [command]' in the console

.PARAMETER criteria
Optional regex pattern that is used to select a subset of tests to execute.
If it is not specified, all available tests will be executed.

.PARAMETER criteria
Optional regex pattern that is used to select a subset of tests to execute.
If it is not specified, all available tests will be executed.

.PARAMETER nledgerExePath
Optional parameter that specifies a fully qualified path to NLedger executable file.
If it is omitted, the script checks TFM code parameter and, if it is not specified either, uses the most appropriate binary file from the current deployment.

.PARAMETER tfmCode
Optional parameter that specifies NLedger executable file by its TFM code.
It also checks "debugMode" switch to make a choice between Release and Debug binaries on development environment.
If it is omitted, the script uses the most appropriate binary file from the current deployment.

.PARAMETER noAnsiColor
Hides ANSI colorization in the script output.

.EXAMPLE
PS>. ./NLTest.Runner.ps1
Runs the script as an interactive console. 
It shows a list of available commands and status information so that the user can run needed tests by entering commands.

.EXAMPLE
PS>. ./NLTest.Runner.ps1 -command xrun
Runs all available tests and shows results in the console

.EXAMPLE
PS>. ./NLTest.Runner.ps1 -nledgerExePath D:\Packages\NLedger\NLedger.exe
Runs the script as an interactive console with explicitly specified path to NLedger executable file

.EXAMPLE
PS>. ./NLTest.Runner.ps1 -tfmCode net5.0 -debugMode
Runs the script as an interactive console with explicitly selected NLedger executable file (Net 5.0/Debug)

#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][ValidateSet("run","xrun","all")]$command,
    [Parameter(Mandatory=$False)][string]$criteria,
    [Parameter(Mandatory=$False)][string]$nledgerExePath,
    [Parameter(Mandatory=$False)][string]$tfmCode,
    [Switch][bool]$debugMode,
    [Switch][bool]$noAnsiColor
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module "$Script:ScriptPath/../Common/SysConsole.psm1" -Force
Import-Module "$Script:ScriptPath/../Common/NLedgerEnvironment.psm1" -Force

if ($noAnsiColor) {$Global:ANSI_Colorization=$False}

[string]$nlTestPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/NLTest.ps1")
if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) {throw "Cannot find '$nlTestPath'"}

$Script:deploymentInfo = Get-NLedgerDeploymentInfo
$env:nledgerExePath = if($nledgerExePath){$nledgerExePath}else{$deploymentInfo.PreferredNLedgerExecutable}

<#
.SYNOPSIS
Run Ledger tests and generate HTML report

.DESCRIPTION

This command runs tests and generates HTML report. If GUI is available, the result is shown in the browser.

By default, the command runs all Ledger tests that are found in the current deployment.
You can specify a regex pattern to select a subset of tests to execute ("filterRegex" parameter).

.PARAMETER filterRegex
Optional parameter that allows you specify which tests to execute by means of a regex pattern.

.EXAMPLE
    C:\PS> run
    Executes all Ledger tests and shows the result in the browser.

.EXAMPLE
    C:\PS> run a1
    Executes tests that contain "a1" in names and shows the result in the browser.
#>
function run {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -showReport -filterRegex $filterRegex)
    }
}

<#
.SYNOPSIS
Run Ledger tests and shows the result in the console

.DESCRIPTION

This command runs tests and shows the result in the current console.

By default, the command runs all Ledger tests that are found in the current deployment.
You can specify a regex pattern to select a subset of tests to execute ("filterRegex" parameter).

.PARAMETER filterRegex
Optional parameter that allows you specify which tests to execute by means of a regex pattern.

.EXAMPLE
    C:\PS> xrun
    Executes all Ledger tests and shows the result in the console.

.EXAMPLE
    C:\PS> xrun a1
    Executes tests that contain "a1" in names and shows the result in the console.
#>
function xrun {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -filterRegex $filterRegex)
    }
}

<#
.SYNOPSIS
Run Ledger tests including disabled and shows the result in the console

.DESCRIPTION

This command runs tests and shows the result in the current console.
Ignored tests are also included.

By default, the command runs all Ledger tests that are found in the current deployment.
You can specify a regex pattern to select a subset of tests to execute ("filterRegex" parameter).

.PARAMETER filterRegex
Optional parameter that allows you specify which tests to execute by means of a regex pattern.

.EXAMPLE
    C:\PS> all
    Executes all Ledger tests and shows the result in the console.

.EXAMPLE
    C:\PS> all a1
    Executes tests that contain "a1" in names and shows the result in the console.
#>
function all {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -disableIgnoreList -filterRegex $filterRegex)
    }
}

<#
.SYNOPSIS
Shows information about the current execution context and deployment structure.

.DESCRIPTION

This command shows the full path to the NLedger executable that will run the Ledger tests.
It also shows additional information (TFM code, Debug/Release, whether the target dotnet framework is available etc).
If file is not selected, not found or probably cannot be run because of no target framework - corresponded warnings are shown.

.PARAMETER compact
Optional parameter to display information in a compact form.

.EXAMPLE
    C:\PS> status
    Shows information about current execution context and deployment structure.

.EXAMPLE
    C:\PS> status -compact
    Shows information about current execution context and deployment structure in a compact form.
#>
function status {
    [CmdletBinding()]
    Param([Switch][bool]$compact)

    Out-NLedgerDeploymentStatus -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath -compact:$compact
}

<#
.SYNOPSIS
Selects an NLedger executable from the available binaries in the current deployment structure.

.DESCRIPTION

This command allows you to choose which NLedger executable will be used to run tests.
You should use TFM code to specify your selection (e.g. net48, net6.0 etc).
The command selects a file from the existing ones in the current deployment structure.
Use "status" command to check which binaries are currently available.
By default, it looks for Release binaries; use "debugMode" switch to select Debug binary.
If entered code not found, you will get a corresponded message.

.PARAMETER tfmCode
TFM code that indicates which NLedger executable you would like to use.

.PARAMETER debugMode
Optional switch indicating that you want to use Debug binary (not Release).

.EXAMPLE
    C:\PS> platform net48
    Selects NLedger executable built for .Net Framework 4.8

.EXAMPLE
    C:\PS> platform net5.0 -debugMode
    Selects NLedger executable built for Net 5.0 in Debug configuration
#>
function platform {
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Switch][bool]$debugMode = $false)

    $nledgerExecutable = Select-NLedgerExecutable -deploymentInfo $Script:deploymentInfo -tfmCode $tfmCode -isDebug $debugMode
    if ($nledgerExecutable) {
        $env:nledgerExePath = $nledgerExecutable
    } else {
        Write-Output "{c:Red}[ERROR] NLedger binary for [$tfmCode|$(if($debugMode){"Debug"}else{"Release"})] not found.{f:Normal}" | Out-AnsiString
    }

    status -compact
}

<#
.SYNOPSIS
Specifies an NLedger executable by fully qualified path and name.

.DESCRIPTION

If you want to use a specific NLedger executable that is not in the current deployment structure,
you can specify a full path and name to this file by means of this command.
If the file is not found by the specified path, you will get a corresponded message.

.PARAMETER nledgerFile
Fully qualified path and name of an NLedger executable.

.EXAMPLE
    C:\PS> file D:\Packages\NLedger\NLedger.exe
    Tests will be run by means of the specified NLedger file.
#>
function file {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$nledgerFile)

    if (Test-Path -LiteralPath $nledgerFile -PathType Leaf) {
        $env:nledgerExePath = $nledgerFile
    } else {
        Write-Output "{c:Red}[ERROR] NLedger file '$nledgerFile' not found.{f:Normal}" | Out-AnsiString
    }

    status -compact
}

function help {
[CmdletBinding()]
Param()

    $Private:commands = [ordered]@{
        ("{c:Yellow}run{f:Normal} [criteria]" | Out-AnsiString) = "Run all or specified by criteria tests and show the report in the browser"
        ("{c:Yellow}xrun{f:Normal} [criteria]" | Out-AnsiString) = "Run all or specified by criteria tests and show the report in the console"
        ("{c:Yellow}all{f:Normal} [criteria]" | Out-AnsiString) = "Run tests including disabled by the ignore list and show the report in the console"
        ("{c:DarkYellow}platform{f:Normal} [TFM]" | Out-AnsiString) = "Select NLedger binary file from the list of available targets"
        ("{c:DarkYellow}file{f:Normal} [name]" | Out-AnsiString) = "Specify NLedger binary file by fully qualified path and name"
        ("{c:DarkYellow}status{f:Normal}" | Out-AnsiString) = "Show the selected NLedger binary file and the list of available targets"
        ("{c:DarkYellow}help{f:Normal}" | Out-AnsiString) = "Shows this help text"
        ("{c:DarkYellow}get-help{f:Normal} [command]" | Out-AnsiString) = "Get additional information for a specified command"
        ("{c:DarkYellow}exit{f:Normal}" | Out-AnsiString) = "Close console window"
    }

    Write-Output "`n{c:White}Available commands:{f:Normal}`n" | Out-AnsiString
    ([PSCustomObject]$Private:commands | Format-List | Out-String).Trim()
}

if ($tfmCode) {platform -tfmCode $tfmCode -debugMode:$debugMode}

if ($command -eq "run") {run -filterRegex $criteria}
elseif ($command -eq "xrun") {xrun -filterRegex $criteria}
elseif ($command -eq "all") {all -filterRegex $criteria}
else {
 
    Write-Output "{c:White}NLedger Testing Toolkit - Interactive console{f:Normal}`n" | Out-AnsiString
    Write-Output "Interactive testing console helps you execute Ledger test files that are available in the current deployment." | Out-AnsiString
    
    help
    status -compact
        
}