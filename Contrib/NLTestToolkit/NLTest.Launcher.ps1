# This script opens PS console to execute NLedger test interactively.
[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module "$Script:ScriptPath/../Common/SysConsole.psm1" -Force
Import-Module "$Script:ScriptPath/../Common/NLedgerEnvironment.psm1" -Force

[string]$nlTestPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/NLTest.ps1")
if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) {throw "Cannot find '$nlTestPath'"}

$Script:deploymentInfo = Get-NLedgerDeploymentInfo
$env:nledgerExePath = $deploymentInfo.PreferredNLedgerExecutable

<#
.SYNOPSIS
Run all or specified by criteria tests and show the report in the browser

.DESCRIPTION

Ledger module package (ledger-[version].whl) is distributed with a test file (ledger_tests.py) that is based on Python unit tests.
Test coverage is about 100%, so passed tests guarantee that the module functions properly.

You may run the tests in a command line console as usual Python unit tests: [path to python] ledger_tests.py
This command performs the same action but in a shorter notation.

Note: Ledger module should be installed in the Python environment.

.PARAMETER path
Optional parameter containing a full path to Python executable file.
If this parameter is omitted, the command uses path from current Python extension settings.

.EXAMPLE
    C:\PS> .\NLTest.ps1
    Executes all test files and displays short summary in the console.

#>
function run {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -showReport -filterRegex $filterRegex)
    }
}

function xrun {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -filterRegex $filterRegex)
    }
}

function all {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    Assert-NLedgerExecutableIsValid -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath {
        $null = (& $nlTestPath -disableIgnoreList -filterRegex $filterRegex)
    }
}

function status {
    [CmdletBinding()]
    Param()

    Write-Output "`n{c:White}Selected NLedger binary file{f:Normal}`n" | Out-AnsiString
    (Out-NLedgerExecutableInfo -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath | Out-String).Trim()
    Write-Output "`n{c:White}Available NLedger binary files{f:Normal}" | Out-AnsiString
    (Out-NLedgerDeploymentInfo -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath)
}

function platform {
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Switch][bool]$debugMode = $false)

    $nledgerExecutable = Select-NLedgerExecutable -deploymentInfo $Script:deploymentInfo -tfmCode $tfmCode -isDebug $debugMode
    if ($nledgerExecutable) {
        $env:nledgerExePath = $nledgerExecutable
    } else {
        Write-Error "NLedger binary for [$tfmCode|$(if($debugMode){"Debug"}else{"Release"})] not found."
    }

    status
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

    Write-Output "Available commands:`n"
    ([PSCustomObject]$Private:commands | Format-List | Out-String).Trim()
}

Write-Output "{c:White}NLedger Testing Toolkit - Interactive console{f:Normal}`n" | Out-AnsiString
Write-Output "Interactive testing console helps you execute NLedger test files that are available in the current deployment." | Out-AnsiString
Write-Output "Note: testing script can also be run directly by command '{c:DarkYellow}./nltest.ps1 ARGUMENTS{f:Normal}' (see detail information by '{c:DarkYellow}get-help ./nltest.ps1{f:Normal}')`n" | Out-AnsiString

help
status

