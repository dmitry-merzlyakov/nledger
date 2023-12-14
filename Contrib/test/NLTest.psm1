<#
.SYNOPSIS
NLedger testing framework

.DESCRIPTION
Provides functions for running Ledger test files

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#> 

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../Common/NLedgerEnvironment.psm1

<#
.SYNOPSIS
Runs Ledger tests

.DESCRIPTION
The command runs the original Ledger tests and displays the result in the current 
console. Additionally, it can generate an HTML report with detailed test results and 
performance metrics.

Test files and the corresponding testing environment are stored 
in the '/Contrib/test' folder.

By default, the command runs all available test files. You can select a subset of tests 
by entering a filter condition in the 'filter' parameter.

The command launches the currently installed NLedger (or the preferred one 
if it is not installed). You can select a specific version of NLedger by providing 
the TFM code for the available binary.

.PARAMETER filter
Filtering criteria in regular expression format, allowing the user to select a subset of test files.

.PARAMETER report
The switch generates a test report in HTML format. Report files are added to 
the current user's documents folder in the 'NLedger' subfolder.

.PARAMETER includeIgnored
Some Ledger tests are excluded from the scope of testing (see the 'ignore' elements and 
associated explanations in the NLTest.Meta.xml file). This switch forces these tests 
to be included in the current test scope.

.PARAMETER tfmCode
The testing framework defaults to the installed or preferred version of NLedger. 
This option allows you to specify which NLedger (in other words, for which runtime) you want to test.

.PARAMETER profile
If you explicitly specify the NLedger version using the 'tfmCode' parameter, 
you can also specify a profile (Debug or Release) using the 'profile' parameter.

.EXAMPLE
PS> test

Runs all available Ledger tests and displays test results in the console.

.EXAMPLE
PS> test py

Runs all Ledger tests with 'py' in their names and displays test results in the console.

.EXAMPLE
PS> test py -report

Runs all available Ledger tests and creates an html report.

.EXAMPLE
PS> test -tfmCode net6

Runs all available Ledger tests using NLedger binaries built for the 'net6' runtime with 
the 'Release' profile. The list of available binaries is displayed by the status command.

.NOTES
For more information about Ledger tests, see the 'Testing Framework' section of the Ledger documentation.
#>
function Invoke-NLedgerTests {
  [CmdletBinding()]
  [Alias("test")]
  Param(
    [Parameter()][string]$filter,
    [switch]$report,
    [switch]$includeIgnored,
    [Parameter()][string]$tfmCode,
    [Parameter()][ValidateSet("debug","release",IgnoreCase=$true)][string]$profile
  )

  [string]$nlTestPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath/NLTest.ps1")
  if (!(Test-Path -LiteralPath $nlTestPath -PathType Leaf)) { throw "File $nlTestPath not found."}

  [string]$nledgerExePath = (Get-NLedgerDeploymentInfo | Select-NLedgerPreferrableInfo $tfmCode $profile).NLedgerExecutable

  $params = @{
    nledgerExePath = $nledgerExePath
    filterRegex = $filter
    showReport = $report
    disableIgnoreList = $includeIgnored
  }

  $null = (& $nlTestPath @params)
}
