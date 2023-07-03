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

function Assert-NLedgerExecutableIsSelected {
    [CmdletBinding()]
    Param()
    
    if (!$env:nledgerExePath) { Write-Warning "NLedger executable file is not selected. Please, specify which NLedger you would like to test (use 'select' command for it)"; return $false}
    if (!(Test-Path -LiteralPath $env:nledgerExePath -PathType Leaf)) { Write-Warning "Selected NLedger executable file does not exist. Please, use 'select' command to specify a correct path to the file"; return $false}
    return $true
}

function run {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    if (Assert-NLedgerExecutableIsSelected) {
        $null = (& $nlTestPath -showReport -filterRegex $filterRegex)
    }
}

function xrun {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    if (Assert-NLedgerExecutableIsSelected) {
        $null = (& $nlTestPath -filterRegex $filterRegex)
    }
}

function all {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][AllowEmptyString()][string]$filterRegex = "")

    if (Assert-NLedgerExecutableIsSelected) {
        $null = (& $nlTestPath -disableIgnoreList -filterRegex $filterRegex)
    }
}

function status {
    [CmdletBinding()]
    Param()
    
    "NLedger binary file: $env:nledgerExePath"
    (Out-NLedgerDeploymentInfo -deploymentInfo $Script:deploymentInfo -selectedNLedgerExecutable $env:nledgerExePath) #| Out-AnsiString
}

function platform {
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Switch][bool]$debugMode = $false)

    $nlBinInfo = Get-NLedgerBinaryInfo -tfmCode $tfmCode -isDebug $debugMode
    if ($nlBinInfo) {
        $env:nledgerExePath = $nlBinInfo.NLedgerExecutable
    } else {
        Write-Error "NLedger binary for [$tfmCode|$(if($debugMode){"Debug"}else{"Release"})] not found."
    }

}


function help {
[CmdletBinding()]
Param()
     write-host "Interactive testing console helps you execute NLedger test files that are available on the disk.`r`n"
     write-host -NoNewline "It supports several short commands that perform typical activities:`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "run"
     write-host -NoNewline "              execute all test files and show a report in the browser;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "run CRITERIA"
     write-host -NoNewline "     execute tests that match the criteria and show a report;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "xrun"
     write-host -NoNewline "             execute all test files and show a summary in the console;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "xrun CRITERIA"
     write-host -NoNewline "    execute matched test files and show a summary in the console;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "all"
     write-host -NoNewline "              execute all test including disabled by the ignore list; the summary is in the console;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "all CRITERIA"
     write-host -NoNewline "     execute matched test files including disabled; the summary is in the console;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "platform [-core]"
     write-host -NoNewline " select .Net platform to execute tests; .Net Framework by default or .Net Core with '-core' switch;`r`n PS>"
     write-host -NoNewline -ForegroundColor Yellow "help"
     write-host -NoNewline "             show help page again.`r`n"
     write-host -NoNewline "`r`nTesting script can be called directly:`r`n PS>"
     write-host -ForegroundColor Yellow ".\nltest.ps1 OPTIONAL ARGUMENTS"
     write-host -NoNewline "Use 'get-help' to get detail information about testing script arguments:`r`n PS>"
     write-host -ForegroundColor Yellow "get-help .\nltest.ps1"
     write-host -NoNewline "`r`nType '"
     write-host -NoNewline -ForegroundColor Yellow "exit"
     write-host "' or close the console window when you finish.`r`n"
}


write-host -ForegroundColor White "NLedger Testing Toolkit - Interactive console"
write-host ""
help
status

