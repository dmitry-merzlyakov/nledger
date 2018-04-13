<#
.SYNOPSIS
    NLedger Testing Framework - test running tool.
.DESCRIPTION
    NLedger Testing Framework is a tool to execute regular Ledger test files and 
    build xml and/or html reports. The latter can be immediately displayed in a default browser.
    The tool can execute either all test files that are in the test folders or 
    you can select some of them by typing a search criteria.
.PARAMETER nledgerExePath
    Relative or absolute path to NLedger executable file (NLedger-cli.exe).
    In case of a relative path, the tool uses the own location folder as a base.
.PARAMETER nledgerTestPath
    Relative or absolute path to a folder containing "test" subfolder with all test files.
    In case of a relative path, the tool uses the own location folder as a base.
.PARAMETER filterRegex
    search criteria in regex format to select files to execute.
    If this parameter is omitted, the tool executes all test files in "test" folder and its subfolders.
    The search criteria is in Regex format.
.PARAMETER xmlReport
    Generate a report in XML format. By default, the report files are added to 
    the current user document folder to subfolder "NLedger".
.PARAMETER htmlReport
    Generate a report in HTML format. By default, the report files are added to
    the current user document folder to subfolder "NLedger".
.PARAMETER showReport
    Immediately shows HTML report in a default browser. 
    This option forces creating HTML report file on a disk.
.PARAMETER reportFileName
    Allows to specify an own location and file names for generated report files.
    Extensions .xml and .html will be added automatically.
    If this parameter is omitted, the files are added to "NLedger" subfolder
    in the default user document folder. 
    The default file name is NLedgerTestReport-[current date and time].
.PARAMETER ignoreListPath
    Relative or absolute path to a file with a list of test to be ignored.
    In case of a relative path, the tool uses the own location folder as a base.
.PARAMETER disableIgnoreList
    Relative or absolute path to a file with a list of test to be ignored.
    In case of a relative path, the tool uses the own location folder as a base.
    This switch forces executing all test files disregarding the content of ignore list.
.EXAMPLE
    C:\PS> .\NLTest.ps1
    Executes all test files and displays short summary in the console.
.EXAMPLE
    C:\PS> .\NLTest.ps1 -filterRegex opt
    Executes all test files that have "opt" in names.
.EXAMPLE
    C:\PS> .\NLTest.ps1 -showReport -filterRegex budget
    Executes all test files that have "budget" in names and shows detail results in browser.
.EXAMPLE
    C:\PS> .\NLTest.ps1 -showReport -xmlReport
    Executes all test files and creates a detail report file in XML format.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   December 22, 2017    
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$nledgerExePath = "",
    [Parameter(Mandatory=$False)][string]$nledgerTestPath = "..",
    [Parameter(Mandatory=$False)][string]$filterRegex = "",
    [Parameter(Mandatory=$False)][string]$ignoreListPath = ".\NLTest.IgnoreList.xml",
    [Switch]$disableIgnoreList = $False,
    [Parameter(Mandatory=$False)][string]$reportFileName = "$([Environment]::GetFolderPath("MyDocuments"))\NLedger\NLedgerTestReport-$(get-date -f yyyyMMdd-HHmmss)",
    [Switch]$xmlReport = $False,
    [Switch]$htmlReport = $False,
    [Switch]$showReport = $False
)

if ($nledgerExePath -eq "") { $nledgerExePath = if ($env:nledgerExePath -eq $null) { "..\NLedger-cli.exe" } else { $env:nledgerExePath } }

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

[string]$Script:absNLedgerExePath = if (![System.IO.Path]::IsPathRooted($nledgerExePath)) { Resolve-Path (Join-Path $Script:ScriptPath $nledgerExePath) -ErrorAction Stop } else { $nledgerExePath }
[string]$Script:absNLedgerTestPath = if (![System.IO.Path]::IsPathRooted($nledgerTestPath)) { Resolve-Path (Join-Path $Script:ScriptPath $nledgerTestPath) -ErrorAction Stop } else { $nledgerTestPath }
[string]$Script:absIgnoreListPath = if (![System.IO.Path]::IsPathRooted($ignoreListPath)) { Resolve-Path (Join-Path $Script:ScriptPath $ignoreListPath) -ErrorAction Stop } else { $ignoreListPath }

if (!(Test-Path $Script:absNLedgerExePath -PathType Leaf)) { throw "Cannot find NLedger executable file: $Script:absNLedgerExePath" }
if (!(Test-Path $Script:absNLedgerTestPath -PathType Container)) { throw "Cannot find a folder with NLedger test: $Script:absNLedgerTestPath" }
if (!(Test-Path $Script:absIgnoreListPath -PathType Leaf)) { throw "Cannot find Ignore List file: $Script:absIgnoreListPath" }

[string]$Script:absTestRootPath = $Script:absNLedgerTestPath
$Script:absNLedgerTestPath = "$Script:absNLedgerTestPath\test"
if (!(Test-Path $Script:absNLedgerTestPath -PathType Container)) { throw "Cannot find 'test' subfolder in test root folder: $Script:absNLedgerTestPath" }

Write-Verbose "NLedger executable path: $Script:absNLedgerExePath"
Write-Verbose "Test folder path: $Script:absNLedgerTestPath"
Write-Verbose "Source folder path: $Script:absTestRootPath"

[xml]$Script:ignoreListContent = Get-Content $Script:absIgnoreListPath
$Script:ignoreListTable = @{}
$Script:ignoreListContent.SelectNodes("/nltest-ignore-list/ignore") | ForEach { $Script:ignoreListTable[$_.test] = $_.reason }
Write-Verbose "Read IgnoreList; found $($Script:ignoreListTable.Count) test(s) to ignore"

[int]$Script:TimeoutMilliseconds = 30 * 1000 # Execution timeout is 30s

# Tech: writes interactive console output
function ConsoleMessage {
    [CmdletBinding()]
    Param(    
        [Parameter(Mandatory=$True)][string]$text,
        [Switch]$newLine = $False,
        [Switch]$err = $False,
        [Switch]$warn = $False,
        [Switch]$comment = $False
    )
    if ($newLine) {
        if ($err) { Write-Host $text -ForegroundColor Red } else { if ($comment) { Write-Host $text -ForegroundColor Gray } else { if ($warn) { Write-Host $text -ForegroundColor DarkYellow } else { Write-Host $text -ForegroundColor White } } }
    } else {
        if ($err) { Write-Host $text -NoNewline -ForegroundColor Red } else { if ($comment) { Write-Host $text -NoNewline -ForegroundColor Gray } else { if ($warn) { Write-Host $text -ForegroundColor DarkYellow } else {Write-Host -NoNewline $text -ForegroundColor White } } }
    }
}

# Tech: normalizes the text to be ready for comparison
function NormalizeOutput {
    [CmdletBinding()]
    Param(    
        [Parameter(Mandatory=$True)][AllowEmptyString()][string]$text
    )
    return $text.Replace("`r`n", "`n").Trim()
}

# Parses a test file and returns a collection of test cases
function ParseTestFile {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$absTestFile,  # Path to a test file
        [Parameter(Mandatory=$True)][int]$testIndex        # Index of a test file
    )

    $Private:testCases = @()
    [int]$Private:testCaseIndex = 0

    [int]$Private:startTestLine = -1
    [string]$Private:commandLine = ""
    [string]$Private:output = ""
    [string]$Private:err = ""
    [bool]$Private:directToErr = $False
    $Private:setVariables = @{}

    [int]$Private:lineNum = -1;

    foreach($Private:line in Get-Content $absTestFile -Encoding UTF8) {
        $Private:lineNum++
        if ($Private:line.StartsWith("#>") -and ($Private:line.Length -gt 2)) {
          $Private:line = $Private:line.Substring(2).Trim()
          if (!($Private:line.StartsWith("setvar "))) { throw "Only 'setvar' command is allowed" }
          $Private:line = $Private:line.Substring("setvar ".Length).Trim()
          $Private:pos = $Private:line.IndexOf('=')
          if ($Private:pos -le 0) { throw "Cannot find a variable name" }
          $Private:setVariables.Add($Private:line.Substring(0, $Private:pos).TrimEnd(), $Private:line.Substring($Private:pos + 1).Trim())
        } else {
            if ($Private:line.StartsWith("test ")) {
                $Private:startTestLine = $Private:lineNum
                $Private:commandLine = $Private:line.Substring("test".Length).Trim()
                $Private:output = ""
                $Private:err = ""
                $Private:directToErr = $False
                $Private:testCaseIndex++
            } else {
                if ($Private:line.StartsWith("__ERROR__")) { $Private:directToErr = $True }
                else {
                    if ($Private:line.StartsWith("end test")) {
                        $Private:testCase = [PsCustomObject]@{
                            TestIndex = $testIndex 
                            TestCaseIndex = $Private:testCaseIndex
                            FileName=$absTestFile
                            ShortFileName=$absTestFile.Substring($Script:absNLedgerTestPath.Length + 1)
                            StartTestLine=$Private:startTestLine
                            LineNum=$Private:lineNum
                            CommandLine=$Private:commandLine
                            Output=$Private:output
                            Err=$Private:err
                            SetVariables = $Private:setVariables
                        }
                        $Private:testCases += $Private:testCase
                        $Private:setVariables = @{}
                    } else {
                        # If current line contains a template "$sourcepath" - adopt it to current folder
                        if ($Private:line -match '\"\$sourcepath/([^\"]*)\"') { $Private:line = $Private:line -replace '\"\$sourcepath/([^\"]*)\"',"""$Script:absTestRootPath\$($Matches[1].Replace('/','\'))""" }
                        # The same but w/o quites
                        if ($Private:line -match '\$sourcepath/(.*)') { $Private:line = $Private:line -replace '\$sourcepath/(.*)',"$Script:absTestRootPath\$($Matches[1].Replace('/','\'))" }
                        # If the line contains $FILE template - replace it with current file name
                        $Private:line = $Private:line -replace '\$FILE', $absTestFile
                        # Special case: if error message contains a relative path in a file name, expand it
                        $Private:errFileName = "(Error: File to include was not found: "")(?<path>\.\/)"
                        $Private:line = $Private:line -replace $Private:errFileName, "Error: File to include was not found: ""$Script:absTestRootPath\"
                        # Add the line either to the output or stderr
                        if ($Private:directToErr) { $Private:err += $Private:line+"`r`n" } else { $Private:output += $Private:line+"`r`n" }
                    }
                }
            }
        }
    }
    if ($Private:testCases.Length -eq 0) { throw "None test cases found in $absTestFile" }
    return $Private:testCases
}

# Runs the file with command line arguments and returns stdout, stderr and exit code
function RunExecutable {

    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$filePath,
        [Parameter(Mandatory=$True)][string]$arguments,
        [Parameter(Mandatory=$False)][AllowEmptyString()][string]$stdin = $null
    )

    Write-Verbose "Running an executable '$filePath' with arguments '$arguments'"

    [bool]$isStdin = ![string]::IsNullOrWhiteSpace($stdin)
    if ($isStdin) { Write-Verbose "STDIN is redirected" }

    $Private:pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $Private:pinfo.FileName = $filePath
    $Private:pinfo.RedirectStandardError = $true
    $Private:pinfo.RedirectStandardOutput = $true
    $Private:pinfo.RedirectStandardInput = $isStdin
    $Private:pinfo.UseShellExecute = $false
    $Private:pinfo.Arguments = $arguments
    $Private:pinfo.CreateNoWindow = $true
    $Private:pinfo.WorkingDirectory = $Script:absTestRootPath
    $Private:pinfo.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $Private:pinfo.StandardErrorEncoding = [System.Text.Encoding]::UTF8

    $Private:p = New-Object System.Diagnostics.Process
    $Private:p.StartInfo = $Private:pinfo

    $Private:stopwatch =  [system.diagnostics.stopwatch]::StartNew()
    $Private:p.Start() | Out-Null
    if ($isStdin) { 
        $Private:p.StandardInput.Write($stdin) 
        $Private:p.StandardInput.Close()
    }
    $Private:outTask = $Private:p.StandardOutput.ReadToEndAsync();
    $Private:errTask = $Private:p.StandardError.ReadToEndAsync();
    $Private:ret = $Private:p.WaitForExit($Script:TimeoutMilliseconds)
    if (!$Private:ret) { $Private:p.Kill() }
    $Private:stopwatch.Stop()

    $Private:runResult = "" | Select-Object -Property stdout,stderr,exitcode,exectime
    $Private:runResult.stdout = $Private:outTask.Result
    $Private:runResult.stderr = $Private:errTask.Result
    $Private:runResult.exitcode = $Private:p.ExitCode
    $Private:runResult.exectime = $Private:stopwatch.ElapsedMilliseconds

    if (!$Private:ret) { $Private:runResult.exitcode = -1024 } # timeout indicator

    Write-Verbose "Executable is finished with code '$($Private:p.ExitCode)'"

    return $Private:runResult
}

# Executes a test case
function RunTestCase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]$testCase,
        [Parameter(Mandatory=$True)][int]$testsCount
    )
    ConsoleMessage "[Test $($testCase.TestIndex) out of $testsCount; Case $($testCase.TestCaseIndex)] $($testCase.ShortFileName). "

    $Private:testCaseResult = "" | Select-Object -Property testCase,exitcode,stdin,arguments,runResult,DiffOut,DiffErr,DiffCode,ExceptionMessage,HasException,IsFailed,IsIgnored,ReasonToIgnore
    $Private:testCaseResult.testCase = $testCase
    $Private:testCaseResult.exitcode = 0
    $Private:testCaseResult.arguments = $testCase.CommandLine
    $Private:ignoreStdErr = $False

    if (!$disableIgnoreList -and $Script:ignoreListTable.ContainsKey($testCase.ShortFileName)) {
       $Private:testCaseResult.ReasonToIgnore = $Script:ignoreListTable[$testCase.ShortFileName]
       $Private:testCaseResult.IsIgnored = $True
       ConsoleMessage -newLine -warn "[IGNORED]"
       return $Private:testCaseResult
    }

    # Finalize command line arguments and expected exit code

    # Find expected exit code if exists
    if ($Private:testCaseResult.arguments -match "(.*) -> ([0-9]+)") { 
        $Private:testCaseResult.exitcode = [int]$Matches[2]
        $Private:testCaseResult.arguments = $Private:testCaseResult.arguments -replace "(.*) -> ([0-9]+)",$Matches[1]
    }
    # Check whether -f option is already presented; add it otherwise
    if ($Private:testCaseResult.arguments -cnotmatch "(^|\s)-f\s") { $Private:testCaseResult.arguments += " -f '$($testCase.FileName)'" }
    # Check whether "--pager" option is presented to add extra options for paging test
    if ($Private:testCaseResult.arguments.Contains("--pager")) {
        $Private:testCaseResult.arguments += "  --force-pager" # Pager test requires this option to override IsAtty=false that is default for other tests
        $env:PATH += ";$(Resolve-Path (Join-Path $Script:ScriptPath '..\Extras'))"
    }
    # Check whether output is redirected to null; remove it if so
    if ($Private:testCaseResult.arguments.Contains("2>/dev/null")) { 
        $Private:testCaseResult.arguments = $Private:testCaseResult.arguments.Replace("2>/dev/null", "")
        $Private:ignoreStdErr = $True
    }
    # Check whether input pipe is needed
    if ($Private:testCaseResult.arguments -match "-f (-|/dev/stdin)(\s|$)") { $Private:testCaseResult.stdin = (Get-Content $testCase.FileName -Encoding UTF8 | Out-String) }
    # Check whether arguments have escaped dollar sign; remove it if so
    $Private:testCaseResult.arguments = $Private:testCaseResult.arguments.Replace(" \$ ", " $ ")

    # Prepare environment variables

    # Tests are configured for 80 symbols in row
    $env:COLUMNS = "80"
    # Simulating pipe redirection in original tests
    $env:nledgerIsAtty = "false"
    # Equals to TZ=America/Chicago
    $env:nledgerTimeZoneId = "Central Standard Time"
    # Force setting output encoding to UTF8 (powershell console issue)
    $env:nledgerOutputEncoding = "utf-8"
    # Disable colored Ansi Terminal emulation to pass output Ansi control codes that some tests validate
    $env:nledgerAnsiTerminalEmulation = "false"
    # Set custom environment variables
    $private:originalVariables = @{}
    foreach($private:name in $testCase.SetVariables.Keys) { 
        $private:originalVariables.Add($private:name, [Environment]::GetEnvironmentVariable($private:name))
        [Environment]::SetEnvironmentVariable($private:name, $testCase.SetVariables[$private:name])
    }

    try {        
        $Private:testCaseResult.runResult = RunExecutable $Script:absNLedgerExePath $Private:testCaseResult.arguments $Private:testCaseResult.stdin

        [string]$Private:nrmExpectedOut = NormalizeOutput $testCase.Output
        [string]$Private:nrmActualOut = NormalizeOutput $Private:testCaseResult.runResult.stdout

        [string]$Private:nrmExpectedErr = NormalizeOutput $testCase.Err
        [string]$Private:nrmActualErr = NormalizeOutput $Private:testCaseResult.runResult.stderr
        if ($Private:ignoreStdErr) { $Private:nrmActualErr = "" }

        $Private:testCaseResult.DiffOut = $Private:nrmExpectedOut -ne $Private:nrmActualOut
        $Private:testCaseResult.DiffErr = $Private:nrmExpectedErr -ne $Private:nrmActualErr
        $Private:testCaseResult.DiffCode = $Private:testCaseResult.exitcode -ne $Private:testCaseResult.runResult.exitcode
    }
    catch {
        $Private:testCaseResult.ExceptionMessage = $_
        $Private:testCaseResult.HasException = $true
    }
    finally {
        foreach($private:name in $private:originalVariables.Keys) { [Environment]::SetEnvironmentVariable($private:name, $private:originalVariables[$private:name]) }
    }
    $Private:testCaseResult.IsFailed = $Private:testCaseResult.DiffOut -or $Private:testCaseResult.DiffErr -or $Private:testCaseResult.DiffCode -or $Private:testCaseResult.HasException

    # Show test case results
    ConsoleMessage "Done ($($Private:testCaseResult.runResult.exectime) msec) "
    if ($Private:testCaseResult.IsFailed) { 
        ConsoleMessage -err -newLine "[FAIL]" | Out-Null
        [string]$Private:errDesc = ""
        if ($Private:testCaseResult.DiffOut) { $Private:errDesc += "(different output) "}
        if ($Private:testCaseResult.DiffErr) { $Private:errDesc += "(different error messages) "}
        if ($Private:testCaseResult.DiffCode) { $Private:errDesc += "(different result codes) "}
        ConsoleMessage -newLine -comment " >Reason: $Private:errDesc" | Out-Null
    } else { ConsoleMessage -newLine "[OK]" }

    return $Private:testCaseResult
}

# Compose summary info
function BuildSummaryInfo {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]$testCaseResults,
        [Parameter(Mandatory=$True)][int]$testsCount
    )

    $Private:totalTime = [timespan]::FromMilliseconds(($testCaseResults | %{$_.runResult.exectime} | Measure-Object -Sum).Sum)

    $Private:totalCases = $testCaseResults.Length
    $Private:totalFail = ($testCaseResults | ?{$_.IsFailed} | %{1} | Measure-Object -Sum).Sum
    $Private:totalIgnored = ($testCaseResults | ?{$_.IsIgnored} | %{1} | Measure-Object -Sum).Sum
    $Private:totalPassed = $Private:totalCases - $Private:totalIgnored - $Private:totalFail
    if ($Private:totalFail -ne 0) { $Private:totalFailPercent = ($Private:totalFail / $Private:totalCases) * 100 } else { $Private:totalFailPercent = 0 }
    if ($Private:totalIgnored -ne 0) { $Private:totalIgnoredPercent = ($Private:totalIgnored / $Private:totalCases) * 100 } else { $Private:totalIgnoredPercent = 0 }
    $Private:totalPassedPercent = ($Private:totalPassed / $Private:totalCases) * 100

    $Private:summaryInfo = [PsCustomObject]@{
        Date = (Get-Date)
        TotalTests = $testsCount
        TotalTestCases = $Private:totalCases
        TotalTime = $Private:totalTime
        TotalTimeStr = "{0:HH:mm:ss,fff}" -f [datetime]($Private:totalTime).Ticks
        FailedTests = $Private:totalFail
        FailedTestsPercent = $Private:totalFailPercent
        IgnoredTests = $Private:totalIgnored
        IgnoredTestsPercent = $Private:totalIgnoredPercent
        PassedTests = $Private:totalPassed
        PassedTestsPercent = $Private:totalPassedPercent
        HasIgnored = $Private:totalIgnored -ne 0
        HasFailed = $Private:totalFail -ne 0        
    }

    return $Private:summaryInfo
}

function RemoveInvalidXmlCharacters {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][AllowEmptyString()]$text
    )
    return $text -replace "[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]","#"
}

# Build XML log file
function GenerateResultXML {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]$testCaseResults,
        [Parameter(Mandatory=$True)]$summaryInfo
    )

    [xml]$Private:doc = New-Object System.Xml.XmlDocument
    $Private:doc.AppendChild($Private:doc.CreateXmlDeclaration("1.0","UTF-8",$null)) | Out-Null
    $Private:doc.AppendChild($Private:doc.CreateComment("NLedger Test execution results")) | Out-Null
    $Private:root = $Private:doc.CreateElement("nltest-results")
    # build summary
    $Private:summary = $Private:doc.CreateElement("summary")
    $Private:summary.SetAttribute("date", $summaryInfo.Date.ToString("s")) | Out-Null
    $Private:summary.SetAttribute("date-str", $summaryInfo.Date.ToString()) | Out-Null
    $Private:summary.SetAttribute("total-tests", $summaryInfo.TotalTests) | Out-Null
    $Private:summary.SetAttribute("total-test-cases", $summaryInfo.TotalTestCases) | Out-Null
    $Private:summary.SetAttribute("total-time", $summaryInfo.TotalTimeStr) | Out-Null
    $Private:summary.SetAttribute("passed-tests", $summaryInfo.PassedTests) | Out-Null
    $Private:summary.SetAttribute("passed-tests-percent", $summaryInfo.PassedTestsPercent) | Out-Null
    $Private:summary.SetAttribute("failed-tests", $summaryInfo.FailedTests) | Out-Null
    $Private:summary.SetAttribute("failed-tests-percent", $summaryInfo.FailedTestsPercent) | Out-Null
    $Private:summary.SetAttribute("ignored-tests", $summaryInfo.IgnoredTests) | Out-Null
    $Private:summary.SetAttribute("ignored-tests-percent", $summaryInfo.IgnoredTestsPercent) | Out-Null
    $Private:summary.SetAttribute("has-failed", $summaryInfo.HasFailed) | Out-Null
    $Private:summary.SetAttribute("has-ignored", $summaryInfo.HasIgnored) | Out-Null
    $Private:root.AppendChild($Private:summary) | Out-Null
    # write test cases and results
    $Private:testCases = $Private:doc.CreateElement("test-cases")
    foreach($Private:testCaseResult in $testCaseResults) {
        # add test case info
        $Private:testCase = $Private:doc.CreateElement("test-case")
        $Private:testCase.SetAttribute("test-index", $Private:testCaseResult.testCase.TestIndex) | Out-Null
        $Private:testCase.SetAttribute("test-case-index", $Private:testCaseResult.testCase.TestCaseIndex) | Out-Null
        $Private:testCase.SetAttribute("test-file", $Private:testCaseResult.testCase.FileName) | Out-Null
        $Private:testCase.SetAttribute("short-file-name", $Private:testCaseResult.testCase.ShortFileName) | Out-Null
        $Private:testCase.SetAttribute("cmd-line", $Private:testCaseResult.testCase.CommandLine) | Out-Null
        # add stdout and stderr only if the test fails
        if ($Private:testCaseResult.IsFailed) {
            $Private:testCaseOutput = $Private:doc.CreateElement("test-case-output")
            $Private:testCaseOutput.InnerText = (RemoveInvalidXmlCharacters $Private:testCaseResult.testCase.Output)
            $Private:testCase.AppendChild($Private:testCaseOutput)  | Out-Null
            $Private:testCaseErr = $Private:doc.CreateElement("test-case-err")
            $Private:testCaseErr.InnerText = (RemoveInvalidXmlCharacters $Private:testCaseResult.testCase.Err)
            $Private:testCase.AppendChild($Private:testCaseErr)  | Out-Null
        }
        # add test case execution results
        $Private:testResult = $Private:doc.CreateElement("test-result")
        $Private:testResult.SetAttribute("is-failed", $Private:testCaseResult.IsFailed) | Out-Null
        $Private:testResult.SetAttribute("is-ignored", $Private:testCaseResult.IsIgnored) | Out-Null
        $Private:testResult.SetAttribute("reason-to-ignore", $Private:testCaseResult.ReasonToIgnore) | Out-Null
        $Private:testResult.SetAttribute("diff-in-code", $Private:testCaseResult.DiffCode) | Out-Null
        $Private:testResult.SetAttribute("diff-in-err", $Private:testCaseResult.DiffErr) | Out-Null
        $Private:testResult.SetAttribute("diff-in-out", $Private:testCaseResult.DiffOut) | Out-Null
        $Private:testResult.SetAttribute("has-exception", $Private:testCaseResult.HasException) | Out-Null
        $Private:testResult.SetAttribute("exception-message", $Private:testCaseResult.ExceptionMessage) | Out-Null
        $Private:testResult.SetAttribute("expected-exit-code", $Private:testCaseResult.exitcode) | Out-Null
        $Private:testResult.SetAttribute("arguments", $Private:testCaseResult.arguments) | Out-Null
        $Private:testResult.SetAttribute("exectime", "{0:HH:mm:ss,fff}" -f [datetime]([timespan]::FromMilliseconds($Private:testCaseResult.runResult.exectime)).Ticks) | Out-Null
        # add stdout and stderr only if the test fails
        if ($Private:testCaseResult.IsFailed) {
            $Private:actualExitCode = $Private:doc.CreateElement("actual-exit-code")
            $Private:actualExitCode.SetAttribute("value", $Private:testCaseResult.runResult.exitcode) | Out-Null
            $Private:testResult.AppendChild($Private:actualExitCode)  | Out-Null
            $Private:actualOutput = $Private:doc.CreateElement("actual-output")
            if ($Private:testCaseResult.runResult.stdout) { $Private:actualOutput.InnerText = (RemoveInvalidXmlCharacters $Private:testCaseResult.runResult.stdout) }
            $Private:testResult.AppendChild($Private:actualOutput)  | Out-Null
            $Private:actualErr = $Private:doc.CreateElement("actual-err")
            if ($Private:testCaseResult.runResult.stderr) { $Private:actualErr.InnerText = (RemoveInvalidXmlCharacters $Private:testCaseResult.runResult.stderr) }
            $Private:testResult.AppendChild($Private:actualErr)  | Out-Null
        }
        $Private:testCase.AppendChild($Private:testResult) | Out-Null
        #
        $Private:testCases.AppendChild($Private:testCase) | Out-Null
    }
    $Private:root.AppendChild($Private:testCases) | Out-Null
    # finishing
    $Private:doc.AppendChild($Private:root) | Out-Null
    return $Private:doc
}

ConsoleMessage -newLine "NLedger Testing Framework Console"
ConsoleMessage -comment "Getting test information... "

$Script:selectedTests = Get-ChildItem -Path $Script:absNLedgerTestPath -Filter *.test -Recurse -ErrorAction Stop -Force | %{$_.FullName.Substring($Script:absNLedgerTestPath.Length) } | ?{if ($filterRegex) {$_ -match $filterRegex} else {$true}}

Write-Verbose "Filter regex: '$filterRegex'; Selected $($Script:selectedTests.Length) tests"
Write-Verbose "Parsing test cases"

$Script:testCases = @()
[int]$Script:testCounter = 0
foreach($Script:relTestFile in $Script:selectedTests) {
    $Script:testCounter++
    [string]$Script:absTestFile = $Script:absNLedgerTestPath + $Script:relTestFile
    Write-Verbose "Processing test file: $Script:absTestFile" 
    $Private:foundTestCases = ParseTestFile $Script:absTestFile $Script:testCounter
    Write-Verbose "Found $($Private:foundTestCases.Length) test cases" 
    $Script:testCases += $Private:foundTestCases
}

ConsoleMessage -newLine -comment "Collected $($Script:testCases.Length) test cases in $($Script:selectedTests.Length) tests"
Write-Verbose "Collected $($Script:testCases.Length) test cases"

$Script:testCaseResults = @()
foreach($Script:testCase in $Script:testCases) {
    $Private:testCaseResult = RunTestCase $Script:testCase $Script:testCounter
    $Script:testCaseResults += $Private:testCaseResult
}

Write-Verbose "Building summary information"

$Script:SummaryInfo = BuildSummaryInfo $Script:testCaseResults $Script:testCounter

ConsoleMessage -newLine "Executed $($Script:SummaryInfo.TotalTestCases) test cases in $($Script:SummaryInfo.TotalTests) tests; total execution time is $($Script:SummaryInfo.TotalTimeStr)"
if ($Script:SummaryInfo.PassedTests -eq 0) { ConsoleMessage -newLine "None test cases passed" } else { ConsoleMessage -newLine "$($Script:SummaryInfo.PassedTests) test case(s) passed ($($Script:SummaryInfo.PassedTestsPercent)%)" }
if (!$Script:SummaryInfo.HasFailed) { ConsoleMessage -newLine "None test cases failed" } else { ConsoleMessage -newLine -err "$($Script:SummaryInfo.FailedTests) test case(s) failed ($($Script:SummaryInfo.FailedTestsPercent)%)" }
if (!$Script:SummaryInfo.HasIgnored) { ConsoleMessage -newLine "None test cases ignored" } else { ConsoleMessage -newLine -warn "$($Script:SummaryInfo.IgnoredTests) test case(s) ignored ($($Script:SummaryInfo.IgnoredTestsPercent)%)" }

# generate XML/HTML reports

if ($xmlReport -or $htmlReport -or $showReport) {
    
    Write-Verbose "Ensure that the target folder exists"
    $Private:reportDir = [System.IO.Path]::GetDirectoryName("$reportFileName.xml")
    if (!(Test-Path $Private:reportDir -PathType Container)) { New-Item -ItemType Directory -Force -Path $Private:reportDir }

    Write-Verbose "Generating an xml report"
    [xml]$xml = GenerateResultXML $Script:testCaseResults $Script:SummaryInfo
    if ($xmlReport) { 
        $xml.Save("$reportFileName.xml") 
        ConsoleMessage -newLine "Created XML report file: $reportFileName.xml"
    }

    if ($htmlReport -or $showReport) {

        Write-Verbose "Generating an html report"
        $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
        $xslt.Load("$Script:ScriptPath\NLTest.xslt");
        [System.Xml.XmlReader]$xmlReader = [System.Xml.XmlNodeReader]::new($xml);
        $writer= [System.IO.StreamWriter] "$reportFileName.html"
        $rd = $xslt.Transform($xmlReader, $null, $writer);
        $writer.Close()
        ConsoleMessage -newLine "Created HTML report file: $reportFileName.html"

        Write-Verbose "Showing an html report"
        if ($showReport) { Invoke-Item "$reportFileName.html" }
    }
}

# Return non-zero exit code in case of any failed tests
if ($Script:SummaryInfo.HasFailed) { exit 2 } else { exit 0 }