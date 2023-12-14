# Extracts Ledger examples from the original documentation (ledger3.texi) and builds Live Demo files
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$ledgerTexiPath,
    [Parameter(Mandatory=$False)][string]$ledgerHtmlPath,
    [Parameter(Mandatory=$False)][string]$liveTemplatePath,
    [Parameter(Mandatory=$False)][string]$liveHtmlPath,
    [Parameter(Mandatory=$False)][string]$casesExportPath,
    [Parameter(Mandatory=$False)][string]$testsFolder,
    [Parameter(Mandatory=$False)][string]$testSandboxFolder,
    [Parameter(Mandatory=$False)][string]$liveDemoConfig,
    [Switch]$casesExport = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

Import-Module $Script:ScriptPath\..\NLDoc.LiveDemo.psm1 -Force
$Script:demoSettings = Get-DefaultNLedgerLiveDemoSettings

if (!$ledgerTexiPath) { $ledgerTexiPath = "$Script:ScriptPath\ledger3.texi" }
if (!$ledgerHtmlPath) { $ledgerHtmlPath = "$Script:ScriptPath\..\..\ledger3.html" }
if (!$liveTemplatePath) { $liveTemplatePath = "$Script:ScriptPath\NLDoc.LiveDemo.Extractor.templates" }
if (!$liveHtmlPath) { $liveHtmlPath = "$Script:ScriptPath\..\$($Script:demoSettings.demoFile)" }
if (!$casesExportPath) { $casesExportPath = "$Script:ScriptPath\ledger3.texi.xml" }
if (!$testsFolder) { $testsFolder = "$Script:ScriptPath\..\..\test\input\" }
if (!$testSandboxFolder) { $testSandboxFolder = "$Script:ScriptPath\..\..\test\nledger\sandbox\" }
if (!$liveDemoConfig) { $liveDemoConfig = "$Script:ScriptPath\..\$($Script:demoSettings.demoConfig)" }

trap 
{ 
  write-error $_ 
  exit 1 
} 

[int]$Script:current_line = 0
[int]$Script:current_pos = 0

[string]$Script:testin_token = 'command'
[string]$Script:testout_token = 'output'
[string]$Script:testdat_token = 'input'
[string]$Script:testfile_token = 'file'
[string]$Script:validate_token = 'validate'
[string]$Script:validate_cmd_token = 'validate-command'
[string]$Script:validate_dat_token = 'validate-data'
[string]$Script:testwithdat_token = 'with_input'
[string]$Script:testwithfile_token = 'with_file'

$Script:DisabledTests = @(
  "Val-7660",  # --pager /bin/cat - inappropriate path to a pager
  "Val-5420",  # ledger payees @Nic --file sample.dat - inappropriate data file (empty output even with original Leger)
  "Val-7840",  # $ ledger --file drewr3.dat --forecast "T>{\$-500.00}" register ^assets ^liabilities - RTE
  "B4DFB9F")   # $ ledger -f expr.dat --format "%(account) %(roundto(amount, 1))\n" reg - RTE

function find_examples {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$fileName    
  )

  [string]$private:startexample = "^@smallexample\s+@c\s+($Script:testin_token|$Script:testout_token|$Script:testdat_token|$Script:testfile_token)(?::([\dA-Fa-f]+|validate))?(?:,(.*))?"
  [string]$private:endexample = "^@end\s+smallexample\s*$"

  $Script:current_line = 0
  $Script:current_pos = 0
  $private:example = $null
  $private:examples = @()

  foreach($line in Get-Content $fileName -Encoding UTF8) {
    $Script:current_pos += ($line.length + ([Math]::Sign($Script:current_line) * 2))
    $Script:current_line += 1

    # Detect the first line
    if($line -match $private:startexample){
        if ($private:example -ne $null) { throw "Invalid operation: an example is not finished" }

        $private:example = [PsCustomObject]@{
            test_begin_pos = $Script:current_pos
            test_begin_line = $Script:current_line
            test_kind = $Matches[1]
            test_id = $Matches[2]
            test_options = @{}
            example = ""
            test_end_pos = $null
            test_end_line = $null
        }

        if ($Matches[3]) { 
            $Matches[3].Split(",") | ForEach-Object { 
                $private:kv = $_.Split(":") 
                $private:example.test_options[$private:kv[0].Trim()] = $private:kv[1].Trim()
            }
        }
    } else {
        # Detect the last line
        if ($line -match $private:endexample) {
            if ($private:example -ne $null) { # Only if getting an example is in progress
                $private:example.test_end_pos = $Script:current_pos
                $private:example.test_end_line = $Script:current_line
                # Getting an example is finished; verification
                if (!$private:example.test_id) {
                    Write-Warning "Example $($private:example.test_kind) in line $($private:example.test_begin_line) is missing id." | Out-Null
                    $private:example.test_id = get_test_id $private:example.example
                    if ($private:example.test_kind -eq $Script:testin_token) {
                        Write-Warning "Use $($private:example.test_id)" | Out-Null
                    }
                } else {
                    if ($private:example.test_kind -eq $Script:testin_token -and $private:example.test_id -ne $Script:validate_token -and $private:example.test_id -ne (get_test_id $private:example.example)) {
                        Write-Warning "Expected test id $($private:example.test_id) for example $($private:example.test_kind) on line $($private:example.test_begin_line) to be $(get_test_id $private:example.example)" | Out-Null
                    }
                }
                if ($private:example.test_id -eq $Script:validate_token) {
                    $private:example.test_id = "Val-$($private:example.test_begin_line)"
                    if ($private:example.test_kind -eq $Script:testin_token) {
                        $private:example.test_kind = $Script:validate_cmd_token
                    } else {
                        if ($private:example.test_kind -eq $Script:testdat_token) {
                            $private:example.test_kind = $Script:validate_dat_token
                        }
                    }
                }
                # Example is OK
                $private:examples += $private:example
                $private:example = $null
            }
        } else {
            # Detect whether getting an example is in progress
            if ($private:example -ne $null) {
                $private:lineValue = $line -replace '@([@{}])', '$1'
                $private:example.example += "$private:lineValue`n"
            }            
        }        
    }
  }

  return $private:examples
}

function get_test_id {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$example    
  )
  $private:enc = [system.Text.Encoding]::UTF8
  $private:data = $private:enc.GetBytes($example.TrimEnd())
  $private:sha1 = New-Object System.Security.Cryptography.SHA1CryptoServiceProvider
  $private:hash = $private:sha1.ComputeHash($private:data)
  $private:stringHash = [BitConverter]::ToString($private:hash).Replace("-","").ToUpper()
  return $private:stringHash.Substring(0,7)
}

function get_Joined_Example {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$exampleKinds,    
    [Parameter(Mandatory=$True)][string]$exampleKind
  )

  return [string](($exampleKinds[$exampleKind] | %{$_.example}) -join "`n")
}

function composeCases {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$examples
  )

  $private:cases = @()
  $private:examplesDict = $examples | Group-Object -Property "test_id" | ForEach-Object -Begin {$Out=@{}} -Process {$Out[$_.Name]=$_.Group} -End {$Out}

  foreach($private:test_id in $private:examplesDict.Keys) {

    $private:exampleGroup = $private:examplesDict[$private:test_id]
    $private:exampleKinds = $private:exampleGroup | Group-Object -Property "test_kind" | ForEach-Object -Begin {$Out=@{}} -Process {$Out[$_.Name]=$_.Group} -End {$Out}
    
    #??[bool]$validation = $private:exampleKinds.Keys -contains $Script:validate_dat_token -or $private:exampleKinds.Keys -contains $Script:validate_cmd_token

    # Parse command
    [bool]$private:validate_command = $false
    [string]$private:command = $private:exampleKinds[$Script:testin_token].example -replace '\\\n',''
    if (!$private:command) {
        if ($private:exampleKinds.Keys -contains $Script:validate_dat_token) {
            $private:command = '$ ledger bal'
        } else {
            if ($private:exampleKinds.Keys -contains $Script:validate_cmd_token) {
                $private:validate_command = $True
                $private:command = $private:exampleKinds[$Script:validate_cmd_token].example
            } else {
                throw "None"
            }
        }
    }
    $private:command = $private:command.Trim()
    if ($private:command.StartsWith("$")) { $private:command = $private:command.Substring(1).TrimStart() }
    if ($private:command -match "\s\-f\s+([A-Za-z0-9.-]+)") { $private:fileName = $Matches[1] }
    else {
        if ($private:command -match "\s\-file\s+([A-Za-z0-9.-]+)") { $private:fileName = $Matches[1] }
        else {
            $private:fileName = if ($private:validate_command) {'sample.dat'} else {"$($private:test_id).dat"}
            $private:command = "$private:command --file $private:fileName"
        }
    }

    # [DM] My fixes for commands
    $private:command = $private:command.Replace("'d>[2011/04/01]'", '"d>[2011/04/01]"')

    # [DM] My assertions
    if (!$private:command.StartsWith("ledger")) { throw "No ledger command" }
    if ([string]::IsNullOrWhiteSpace($private:fileName)) { throw "No file name" }

    # Getting input/output
    [string]$private:eoutput = get_Joined_Example -exampleKinds $private:exampleKinds -exampleKind $Script:testout_token
    [string]$private:einput = get_Joined_Example -exampleKinds $private:exampleKinds -exampleKind $Script:testdat_token
    if ([string]::IsNullOrWhiteSpace($private:einput)) {
        $private:testinExample = $private:exampleKinds[$Script:testin_token]
        if ($private:testinExample) {
            $private:with_input = $private:testinExample.test_options[$Script:testwithdat_token]
            if ($private:with_input) {
                $private:iExampleKinds = $private:examplesDict[$private:with_input] | Group-Object -Property "test_kind" | ForEach-Object -Begin {$Out=@{}} -Process {$Out[$_.Name]=$_.Group} -End {$Out}
                $private:einput = get_Joined_Example -exampleKinds $private:iExampleKinds -exampleKind $Script:testdat_token
            }
        }
        if ([string]::IsNullOrWhiteSpace($private:einput)) {
            $private:einput = get_Joined_Example -exampleKinds $private:exampleKinds -exampleKind $Script:validate_dat_token
        }
    }

    $private:case = [PsCustomObject]@{
        test_id = $private:test_id
        command = $private:command
        filename = $private:fileName
        input = $private:einput.Trim()
        output = $private:eoutput.Trim()
        src = $private:exampleGroup
        is_active = -not ($Script:DisabledTests -contains $private:test_id)
    }
    $private:cases += $private:case
  }

  return $private:cases
}

function exportCases {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$fileName,
    [Parameter(Mandatory=$True)]$cases
  )

  [xml]$private:xml = [xml]'<?xml version="1.0" encoding="utf-8" ?><ledger-examples></ledger-examples>'

  foreach($private:case in $cases) {
    $private:xmlCase = $private:xml.CreateElement("example")
    
    $private:xmlCase.SetAttribute("test-id", $private:case.test_id) | Out-Null
    $private:xmlCase.SetAttribute("command", $private:case.command) | Out-Null
    $private:xmlCase.SetAttribute("filename", $private:case.filename) | Out-Null

    if ($private:case.input) {
        $private:xmlInput = $private:xml.CreateElement("input")
        $private:xmlInput.InnerText = (EncodeESC $private:case.input)
        $private:xmlCase.AppendChild($private:xmlInput) | Out-Null
    }

    if ($private:case.output) {
        $private:xmlOutput = $private:xml.CreateElement("output")
        $private:xmlOutput.InnerText = (EncodeESC $private:case.output)
        $private:xmlCase.AppendChild($private:xmlOutput) | Out-Null
    }
    
    $private:xmlSrcCollection = $private:xml.CreateElement("src-collection")
    $private:xmlCase.AppendChild($private:xmlSrcCollection) | Out-Null               
    foreach($private:src in $private:case.src) {
        $private:xmlSrc = $private:xml.CreateElement("src")
        $private:xmlSrc.SetAttribute("test_id", $private:src.test_id) | Out-Null
        $private:xmlSrc.SetAttribute("test_kind", $private:src.test_kind) | Out-Null
        $private:xmlSrc.SetAttribute("begin_pos", $private:src.test_begin_pos) | Out-Null
        $private:xmlSrc.SetAttribute("begin_line", $private:src.test_begin_line) | Out-Null
        $private:xmlSrc.SetAttribute("end_pos", $private:src.test_end_pos) | Out-Null
        $private:xmlSrc.SetAttribute("end_line", $private:src.test_end_line) | Out-Null

        $private:xmlSrcOptions = $private:xml.CreateElement("options")
        foreach($private:optName in $private:src.test_options.Keys) {
            $private:xmlSrcOption = $private:xml.CreateElement("option")
            $private:xmlSrcOption.SetAttribute("name", $private:optName) | Out-Null
            $private:xmlSrcOption.SetAttribute("value", $private:src.test_options[$private:optName]) | Out-Null
            $private:xmlSrcOptions.AppendChild($private:xmlSrcOption) | Out-Null
        }
        $private:xmlSrc.AppendChild($private:xmlSrcOptions) | Out-Null

        $private:xmlSrcText = $private:xml.CreateElement("text")
        $private:xmlSrcText.InnerText = (EncodeESC $private:src.example)
        $private:xmlSrc.AppendChild($private:xmlSrcText)

        $private:xmlSrcCollection.AppendChild($private:xmlSrc)| Out-Null
    }


    $private:xml.DocumentElement.AppendChild($private:xmlCase) | Out-Null
  }

  $private:xml.Save($fileName) | Out-Null
}

[string]$Script:Esc = [System.Text.Encoding]::UTF8.GetString(27)
function EncodeESC {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string][AllowEmptyString()]$inputText
  )

  if (!$inputText) { return $inputText }

  return $inputText.Replace($Script:Esc, "")  # Since ESC code in not rendered in HTML, it is OK to remove it from export
}

function buildFindCaseIndex {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$cases
  )

  $private:index = new-object System.Collections.Hashtable # @{} is not appropriate because of case insensitivity

  foreach($private:case in $cases) {
    foreach($private:srcItem in $private:case.src) {
      if ($private:srcItem.example) {
          $private:key = $private:srcItem.example.Replace("`r`n", "`n").Trim()
          if ($private:key) {
              if (!$private:index.ContainsKey($private:key)) {
                 $private:index.Add($private:key, @( [PsCustomObject] @{ Case=$private:case; TestKind=$private:srcItem.test_kind } ))
              } else {
                 $private:entry = $private:index[$private:key]
                 $private:entry += [PsCustomObject] @{ Case=$private:case; TestKind=$private:srcItem.test_kind }
              }
          }
      }
    }
  }

  return $private:index
}

function findCase {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$caseIndex,
    [Parameter(Mandatory=$True)][string]$example,
    [Parameter(Mandatory=$True)][string]$test_kind
  )

  $example = [System.Net.WebUtility]::HtmlDecode($example).Replace("`r`n", "`n").Replace("�<samp>$</samp>�","@samp{$}").Trim()
  $private:entry = $caseIndex[$example]
  if ($private:entry) {
        foreach($private:entryItem in $private:entry) {
            if ($private:entryItem.TestKind -eq $test_kind) {
                return $private:entryItem.Case
            }
        }
  }

  return $null
}

function readTemplates {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$fileName
  )

  $private:content = Get-Content -Encoding UTF8 $fileName

  $private:templates = @{}
  $private:templateName = $null
  $private:template = ""

  foreach($private:row in $private:content) {
    if ($private:row -match "^#TPL:\s*(\w+)\s*$") {
        if ($private:templateName) {
            $private:templates.Add($private:templateName,$private:template)
        }
        $private:templateName = $Matches[1]
        $private:template = ""
    } else {
        $private:template += "$private:row`n"
    }
  }
  if ($private:templateName) {
      $private:templates.Add($private:templateName,$private:template)
  }
  return $private:templates
}

function readHTML {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$fileName
  )

  return (Get-Content $fileName -Encoding UTF8 | Out-String)
}

function saveHTML {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$html,
    [Parameter(Mandatory=$True)][string]$fileName
  )

  Out-File -LiteralPath $fileName -Encoding utf8 -InputObject $html | Out-Null
}

function findExamplesInHTML {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$html,
    [Parameter(Mandatory=$True)]$cases
  )
  $private:regexPattern = '(?<=(<pre class="smallexample">))(.|\r|\n)+?(?=(<\/pre>))'
  $private:allMatches = (select-string -Pattern $private:regexPattern -InputObject $html -AllMatches | %{$_.Matches})

  $private:foundExamples = @()
  $private:foundCases = @()

  $private:caseIndex = buildFindCaseIndex $cases

  foreach($private:matchItem in $private:allMatches) {
      $private:value = $private:matchItem.Value

      $private:test_kind = $Script:testin_token
      $private:case = findCase -caseIndex $private:caseIndex -example $private:value -test_kind $private:test_kind
      if (!$private:case) { 
           $private:test_kind = $Script:validate_cmd_token 
           $private:case = findCase -caseIndex $private:caseIndex -example $private:value -test_kind $private:test_kind
           if (!$private:case) { 
                $private:test_kind = $Script:validate_dat_token 
                $private:case = findCase -caseIndex $private:caseIndex -example $private:value -test_kind $private:test_kind
                if (!$private:case) {
                    $private:test_kind = $Script:testdat_token 
                    $private:case = findCase -caseIndex $private:caseIndex -example $private:value -test_kind $private:test_kind
                }
           }
      }

      if ($private:case) {
          $private:foundExamples += @{ Case=$private:case; TestKind=$private:test_kind; HtmlPre=$private:matchItem.Groups[1] }
          $private:foundCases += $private:case
      }
  }

  # Warn about not found cases
  $private:notFoundCount = 0
  foreach($private:notFoundCase in ($cases | ?{$private:foundCases -notcontains $_})) {
    Write-Warning "Case $($private:notFoundCase.test_id) not found"
    $private:notFoundCount++
  }

  Write-Verbose "All cases: $($cases.Count)"
  Write-Verbose "Found: $($private:foundExamples.Count)"
  Write-Verbose "Not Found: $private:notFoundCount"

  return $private:foundExamples
}

function modifyHTML {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$html,
    [Parameter(Mandatory=$True)]$foundExamples,
    [Parameter(Mandatory=$True)][string]$templateName
  )

  $private:templates = readTemplates $templateName
  $foundExamples = $foundExamples | ?{$_.Case.is_active}

  #Replace found elements

  $private:htmlPre = $private:templates["htmlPre"].Trim()
  for($private:index=$foundExamples.Count-1;$private:index -ge 0; $private:index--) {
    $private:found = $foundExamples[$private:index]
    $private:pre = $private:htmlPre.Replace('$test_id', $private:found.Case.test_id).Replace('$test_kind', $private:found.TestKind)
    $html = $html.Remove($private:found.HtmlPre.Index,$private:found.HtmlPre.Length).Insert($private:found.HtmlPre.Index,$private:pre)
  }

  # add extra styles
  $private:styles = $private:templates["styles"].Trim()
  $html = $html -replace "<\/style>","$private:styles`n</style>"

  # add java scripts

  $private:javascript = $private:templates["javascript"].Trim()
  $html = $html -replace "<\/head>","<script>`n$private:javascript`n</script>`n</head>"

  # add html

  $private:foreachExample = $private:templates["foreachExample"].Trim()
  [string]$private:foreachExamples = ""
  foreach($private:foundExample in $foundExamples) { 
    $private:foundExampleItem = $private:foreachExample.Replace('$test_id', $private:foundExample.Case.test_id).Replace('$test_kind', $private:foundExample.TestKind).Replace('$command', [System.Net.WebUtility]::HtmlEncode($private:foundExample.Case.command).Trim())
    $private:foreachExamples += "$private:foundExampleItem`n"
  }

  $private:htmlBody = $private:templates["htmlBody"].Replace('$foreachExample', $private:foreachExamples)
  if ($html -match '<body([A-Za-z0-9\s=\"#]*)>') { $html = $html -replace '<body([A-Za-z0-9\s=\"#]*)>',"$($Matches[0])$private:htmlBody" } else { throw "expected htmlBody" }

  $private:htmlEndBody = $private:templates["htmlEndBody"].Trim()
  $html = $html -replace '<\/body>',"$private:htmlEndBody`n</body>"

  return $html
}

function collectDataFiles {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$foundExamples,
    [Parameter(Mandatory=$True)][string]$testFolder,
    [Parameter(Mandatory=$True)][string]$localFolder
  )

  $private:files = @{}

  foreach($private:case in ($foundExamples | %{$_.Case})) {
    [string]$private:fileName = $private:case.filename
    [string]$private:test_id = $private:case.test_id

    [string]$private:filePath = "$testFolder\$private:fileName"
    if (!(Test-Path -LiteralPath $private:filePath -PathType Leaf)) {
        $private:filePath = "$localFolder\$private:fileName"
        if (!(Test-Path -LiteralPath $private:filePath -PathType Leaf)) {
              if ([string]::IsNullOrWhiteSpace($private:case.input)) {
                    Write-Warning "Attempt to write an empty input [$private:filePath $private:test_id]"
                    $private:case.is_active = $False
                    continue
              }
              Out-File -LiteralPath $private:filePath -Encoding utf8 -InputObject $private:case.input | Out-Null
        } else {
            $private:existingContent = (Get-Content -LiteralPath $private:filePath -Encoding utf8 | Out-String).Replace("`r`n", "`n").Trim()
            $private:caseContent = $private:case.input.Trim()
            if ($private:existingContent -ne $private:caseContent) {
                for($private:index=0; $private:index -lt $private:existingContent.Length -and $private:index -lt $private:caseContent.Length; $private:index++) {
                    if ($private:existingContent[$private:index] -ne $private:caseContent[$private:index]) {
                        Write-Warning "'$([int]$private:existingContent[$private:index])' vs '$([int]$private:caseContent[$private:index])' - index $private:index [$private:filePath $private:test_id]"
                        break
                    }
                }
                if ($private:existingContent.Length -ne $private:caseContent.Length) { Write-Warning "Different file sizes [$private:filePath $private:test_id]" }
                # It is necessary to set another file name for given case
                $private:fileName = "$($private:case.test_id).dat"
                $private:filePath = "$localFolder\$private:fileName"
                $private:case.command = $private:case.command.Replace($private:case.filename, $private:fileName)
                $private:case.filename = $private:fileName
                Out-File -LiteralPath $private:filePath -Encoding utf8 -InputObject $private:case.input | Out-Null
            }
        }
    }

    $private:files[$private:case.filename] = $private:filePath
  }

  return $private:files
}

function saveLiveDemoConfiguration {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$files,
    [Parameter(Mandatory=$True)]$foundExamples,
    [Parameter(Mandatory=$True)][string]$liveDemoConfig
  )

  $private:contribRootPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..\..")

  $private:demoFiles= @{}
  foreach($private:file in $files.Keys) {
    $private:absFilePath = [System.IO.Path]::GetFullPath($files[$private:file])
    $private:relativePath = $private:absFilePath.Remove(0, $private:contribRootPath.Length).Insert(0, "..")
    $private:demoFiles.Add($private:file, $private:relativePath)
  }

  $private:demoCases = $foundExamples | ?{$_.Case.is_active} | %{ [PsCustomObject] @{
    TestID = $_.Case.test_id
    Command = $_.Case.command
    FileName = $_.Case.filename
  } }

  Write-NLedgerLiveDemoConfig -demoFiles $private:demoFiles -demoCases $private:demoCases -fileName $liveDemoConfig | Out-Null
}

$examples = find_examples -fileName $ledgerTexiPath
$cases = composeCases $examples
if ($casesExport) { exportCases -fileName $casesExportPath -cases $cases | Out-Null }
$htmlSource = readHTML $ledgerHtmlPath
$foundExamples = findExamplesInHTML -html $htmlSource -cases $cases
$files = collectDataFiles -foundExamples $foundExamples -testFolder $testsFolder -localFolder $testSandboxFolder
$htmlSource = modifyHTML -html $htmlSource -foundExamples $foundExamples -templateName $liveTemplatePath
saveHTML -html $htmlSource -fileName $liveHtmlPath | Out-Null
saveLiveDemoConfiguration -files $files -foundExamples $foundExamples -liveDemoConfig $liveDemoConfig | Out-Null