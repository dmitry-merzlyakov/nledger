<#
.SYNOPSIS
NLedger Interactive Demo routines

.DESCRIPTION
Provides helper functions that allows running NLedger interactive demo

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#> 

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\PsHttpListener.psm1
Import-Module $Script:ScriptPath\..\Common\SysPlatform.psm1
Import-Module $Script:ScriptPath\..\Common\SysConsole.psm1
Import-Module $Script:ScriptPath\..\Common\NLedgerConfig.psm1
Import-Module $Script:ScriptPath\..\Common\NLedgerEnvironment.psm1

function Get-DefaultNLedgerLiveDemoSettings {
  [CmdletBinding()]
  Param()

  return @{
    demoConfig = "NLDoc.LiveDemo.config.xml"
    sandbox = "{MyDocuments}/NLedger/DemoSandbox"
    demoURL = "http://localhost:8080/"
    demoFile = "NLDoc.LiveDemo.ledger3.html"
    editor = $(if(Test-IsWindows){"wordpad.exe"}else{"xdg-open"})
    browser = ""
  }
}

function Get-NLedgerLiveDemoConfigExtension {
  [CmdletBinding()]
  Param()

  $private:defaultSettings = Get-DefaultNLedgerLiveDemoSettings

  return [PsCustomObject]@{
    Category=".Net Ledger Live Demo Settings"
    Settings=@(
      [PsCustomObject]@{Name="DemoURL";Description="Live Demo URL. Allows to specify an alternative port or machine name.";Default=$private:defaultSettings.demoURL}
      [PsCustomObject]@{Name="DemoEditor";Description="The name of an editor that is used to modify Live Demo data files.";Default=$private:defaultSettings.editor}
      [PsCustomObject]@{Name="DemoBrowser";Description="The name of a browser to show Live Demo page. Empty value means a default browser. Allowed short names 'edge', 'chrome', 'firefox', 'iexplore' or a fully-qualified executable name.";Default=$private:defaultSettings.browser}
  )}
}

# Register Live Demo Configuration extension in NLSetup
$Global:ConfigDataExtensions["LiveDemo"] = Get-NLedgerLiveDemoConfigExtension

function Get-NLedgerLiveDemoConfig {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$liveDemoConfig = "$Script:ScriptPath\NLDoc.LiveDemo.config.xml"
  )

  $private:settings = Get-DefaultNLedgerLiveDemoSettings
  $private:configData = Get-NLedgerConfigData
  $private:settings["demoURL"] = ($private:configData.Settings | Where-Object{$_.Name -eq "DemoURL"}).EffectiveSettingValue
  $private:settings["editor"] = ($private:configData.Settings | Where-Object{$_.Name -eq "DemoEditor"}).EffectiveSettingValue
  $private:settings["browser"] = ($private:configData.Settings | Where-Object{$_.Name -eq "DemoBrowser"}).EffectiveSettingValue

  [string]$liveDemoConfig = "$Script:ScriptPath\$($private:settings.demoConfig)"

  Write-Verbose "Read Demo Config by path: $liveDemoConfig"
  [xml]$private:demoConfigXml = Get-Content -Encoding UTF8 -LiteralPath $liveDemoConfig
  $private:files = @{}
  $private:demoConfigXml.SelectNodes("/ledger-demo/demo-files/file") | ForEach-Object { $private:files[$_.name] = $_.path }
  $private:commands = @{}
  $private:demoConfigXml.SelectNodes("/ledger-demo/demo-cases/case") | ForEach-Object { $private:commands[$_.'test-id'] = $_.command }
  $private:testFiles = @{}
  $private:demoConfigXml.SelectNodes("/ledger-demo/demo-cases/case") | ForEach-Object { $private:testFiles[$_.'test-id'] = $_.filename }

  return [PsCustomObject] @{
    Files = $private:files
    Settings = $private:settings
    Sandbox = $private:settings["sandbox"].Replace("{MyDocuments}",[Environment]::GetFolderPath("MyDocuments"))
    DemoURL = $private:settings["demoURL"]
    DemoFile = Resolve-Path (Join-Path $Script:ScriptPath $private:settings["demoFile"]) -ErrorAction Stop
    Commands = $private:commands
    TestFiles = $private:testFiles
    Editor = $private:settings["editor"]
    Browser = $private:settings["browser"]
  }
}

function Open-NLedgerLiveDemoPage {
  [CmdletBinding()]
  Param([Parameter(Mandatory)]$demoConfig)

  $private:url = "$($demoConfig.DemoURL)content"

  if ($demoConfig.browser -eq "edge") {
     Start-Process "microsoft-edge:$private:url"
  } else {
     if ($demoConfig.browser -eq "chrome") {
        Start-Process "chrome.exe" $private:url
     } else {
        if ($demoConfig.browser -eq "firefox") {
            Start-Process "firefox.exe" $private:url
        } else {
            if ($demoConfig.browser -eq "iexplore") {
                Start-Process "iexplore.exe" $private:url
            } else {
                if ($demoConfig.browser) {
                    Start-Process $demoConfig.browser $private:url
                } else 
                {
                    Start-Process $private:url
                }
            }
        }
     }
  }
}

function Initialize-NLedgerLiveDemoSandbox {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)]$files,
    [Parameter(Mandatory)][string]$sandbox
  )

  if (!(Test-Path $sandbox -PathType Container)) { New-Item -ItemType Directory -Force -Path $sandbox }
  foreach($key in $files.Keys) {
    [string]$sourceFile = Resolve-Path (Join-Path $Script:ScriptPath $files[$key]) -ErrorAction Stop
    [string]$targetFile = [System.IO.Path]::GetFullPath("$sandbox/$key")
    if (!(Test-Path $sourceFile -PathType Leaf)) { throw "Source file does not exist: $sourceFile" }
    if (!(Test-Path $targetFile -PathType Leaf)) { $null = Copy-Item -LiteralPath $sourceFile -Destination $targetFile }
  }

}

function Undo-NLedgerLiveDemoSandboxFile {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)]$files,
    [Parameter(Mandatory)][string]$fileName,
    [Parameter(Mandatory)][string]$sandbox
  )

  if (!(Test-Path $sandbox -PathType Container)) { throw "No sandbox folder" }
  [string]$sourceFile = Resolve-Path (Join-Path $Script:ScriptPath $files[$fileName]) -ErrorAction Stop
  [string]$targetFile = [System.IO.Path]::GetFullPath("$sandbox/$key")
  if (!(Test-Path $sourceFile -PathType Leaf)) { throw "Source file does not exist: $sourceFile" }
  $null = Copy-Item -LiteralPath $sourceFile -Destination $targetFile -Force
}

function Invoke-NLedgerLiveDemoActionRun {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$testid,
    [Parameter(Mandatory)]$demoConfig
  )

  Write-Verbose "Requested app run for test $testid"

  $private:command = $demoConfig.Commands[$testid]
  if ($private:command) {
    $env:CMDTORUN = $private:command
    Write-Verbose "Command to execute (CMDTORUN): $env:CMDTORUN"
    if (Test-IsWindows) {
      $private:cmd = "/k """"$ScriptPath\NLDoc.LiveDemo.Launch.cmd"" $private:command"""
      $env:CMDTOVIEW = $private:command.Replace("^","^^")
      Write-Verbose "Command to view (CMDTOVIEW): $env:CMDTOVIEW"
      Write-Verbose "Start process - working directory: $($demoConfig.Sandbox); file path: $env:comspec; ArgumentList: $private:cmd"
      start-process -workingdirectory $demoConfig.Sandbox -filepath "$env:comspec" -ArgumentList $private:cmd  
    } else {
      if(Test-IsOSX) {
        [string]$Private:osxTempRunFile = "/tmp/nldoclivedemolaunch.sh"
        if(Test-Path -LiteralPath $Private:osxTempRunFile -PathType Leaf) { Remove-Item -LiteralPath $Private:osxTempRunFile }        
        $null = Add-Content $Private:osxTempRunFile "cd $($demoConfig.Sandbox)"
        $null = Add-Content $Private:osxTempRunFile "echo $env:CMDTORUN"
        $null = Add-Content $Private:osxTempRunFile "$env:CMDTORUN"
        $null = (& chmod +x $Private:osxTempRunFile)
        start-process -workingdirectory $demoConfig.Sandbox -filepath "open" -ArgumentList "-a Terminal $Private:osxTempRunFile"
      } else {
        [string]$Private:xargs = "-e `"$ScriptPath/NLDoc.LiveDemo.Launch.sh;bash`""
        start-process -workingdirectory $demoConfig.Sandbox -filepath "xterm" -ArgumentList $Private:xargs 
      }
    }
    return "OK: $testid"
  } else {
    return "FAIL: command not found - $testid"
  }
}

function Invoke-NLedgerLiveDemoActionEdit {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$testid,
    [Parameter(Mandatory)]$demoConfig
  )

  Write-Verbose "Requested edit data file for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {
    if(Test-IsOSX) {
      Start-process -FilePath "open" -ArgumentList "-t $($demoConfig.Sandbox)/$private:file"
    } else {
      start-process -WorkingDirectory $demoConfig.Sandbox -FilePath $demoConfig.Editor $private:file
    }
    return "OK: $testid"
  } else {
    return "FAIL: file not found - $testid"
  }
}

function Invoke-NLedgerLiveDemoActionOpen {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$testid,
    [Parameter(Mandatory)]$demoConfig
  )

  Write-Verbose "Requested opening containing folder for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {    
    if (Test-IsWindows) {
      start-process -WorkingDirectory $demoConfig.Sandbox -FilePath "explorer" "/e, /select, `"$($demoConfig.Sandbox.Replace('/','\'))\$private:file`""
    }
    else {
      if(Test-IsOSX) {
         start-process -FilePath "open" -ArgumentList $demoConfig.Sandbox
      } else {
         start-process -WorkingDirectory $demoConfig.Sandbox -FilePath "xdg-open" -ArgumentList $demoConfig.Sandbox
      }
    }
    return "OK: $testid"
  } else {
    return "FAIL: file not found - $testid"
  }
}

function Invoke-NLedgerLiveDemoActionRevert {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$testid,
    [Parameter(Mandatory)]$demoConfig
  )

  Write-Verbose "Requested reverting data file for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {
    $null = Undo-NLedgerLiveDemoSandboxFile -fileName $private:file -sandbox $demoConfig.Sandbox -files $demoConfig.Files
    return "OK: $testid"
  } else {
    return "FAIL: file not found - $testid"
  }
}

function Write-NLedgerLiveDemoConfig {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$demoFiles,
    [Parameter(Mandatory=$True)]$demoCases,
    [Parameter(Mandatory=$False)][string]$fileName
  )

  [xml]$private:xml = [xml]'<?xml version="1.0" encoding="utf-8" ?><ledger-demo><demo-files /><demo-cases /></ledger-demo>'

  $private:xmlFiles = ($private:xml.'ledger-demo').GetElementsByTagName("demo-files")
  foreach($private:file in $demoFiles.Keys) {
    $private:xmlFile = $private:xml.CreateElement("file")
    $null = $private:xmlFile.SetAttribute("name", $private:file)
    $null = $private:xmlFile.SetAttribute("path", $demoFiles[$private:file])
    $null = $private:xmlFiles.AppendChild($private:xmlFile)
  }

  $private:xmlCases = ($private:xml.'ledger-demo').GetElementsByTagName("demo-cases")
  foreach($private:case in $demoCases) {
    $private:xmlCase = $private:xml.CreateElement("case")
    $null = $private:xmlCase.SetAttribute("test-id", $private:case.TestID)
    $null = $private:xmlCase.SetAttribute("command", $private:case.Command)
    $null = $private:xmlCase.SetAttribute("filename", $private:case.FileName)
    $null = $private:xmlCases.AppendChild($private:xmlCase)
  }

  $null = $private:xml.Save($fileName)
}

<#
.SYNOPSIS
Shows interactive Ledger documentation

.DESCRIPTION
The command is intended to display interactive Ledger documentation. 
The interactivity of the documentation means that you can use your local NLedger to run the examples.

The documentation is shown in your web browser. Each example in the documentation is
accompanied by a pop-up menu where you can select actions. They allow you to run NLedger 
for a given example, copy command line arguments, and view or edit a sample data file.

The main goal is to give a general idea of how Ledger works in action.

.NOTES
The command requires that NLedger be installed with a hard link (ledger).
#>
function Invoke-NLedgerLiveDemo {
  [CmdletBinding()]
  [Alias("live-demo")]
  Param()

  Write-Output "NLedger Live Demo"

  if (!(Get-NLedgerDeploymentInfo).Installed) { throw "NLedger is not installed."}

  $demoConfig = Get-NLedgerLiveDemoConfig

  $actions = @{
    "\/content" = { Get-Content -Encoding UTF8 -LiteralPath $demoConfig.DemoFile | Out-String }
    "\/shutdown" = { Invoke-HttpListenerShutdown }
    "\/act\.actRun\.(.+)" = { Invoke-NLedgerLiveDemoActionRun -testid $_[1] -demoConfig $demoConfig }
    "\/act\.actEdit\.(.+)" = { Invoke-NLedgerLiveDemoActionEdit -testid $_[1] -demoConfig $demoConfig }
    "\/act\.actOpen\.(.+)" = { Invoke-NLedgerLiveDemoActionOpen -testid $_[1] -demoConfig $demoConfig }
    "\/act\.actRevert\.(.+)" = { Invoke-NLedgerLiveDemoActionRevert -testid $_[1] -demoConfig $demoConfig }
  }

  Write-Output "The web browser with the demo content should open automatically."
  Write-Output "You can also open the demo manually using the link: {c:Yellow}$($demoConfig.DemoURL){f:Normal}." | Out-AnsiString
  Write-Output "When you close the demo page, this process shuts down."
  Write-Output "You can also stop this process at any time by pressing {c:Yellow}CTRL-C{f:Normal}." | Out-AnsiString
  
  $null = Initialize-NLedgerLiveDemoSandbox -files $demoConfig.Files -sandbox $demoConfig.Sandbox
  $null = Start-HttpListener -actions $actions -prefix $demoConfig.DemoURL -onListenerStart { $null = Open-NLedgerLiveDemoPage $demoConfig } 
}

