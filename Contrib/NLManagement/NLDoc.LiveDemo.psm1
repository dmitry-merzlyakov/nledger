# If you run this script in ISE, you may want to configure Execution Policy by this command:
# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\NLSetup.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
[bool]$Script:isOsxPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
[string]$Script:dsp = [System.IO.Path]::DirectorySeparatorChar

function GetDefaultDemoSettings {
  [CmdletBinding()]
  Param()

  return @{
    demoConfig = "NLDoc.LiveDemo.config.xml"
    sandbox = "{MyDocuments}/NLedger/DemoSandbox"
    demoURL = "http://localhost:8080/"
    demoFile = "NLDoc.LiveDemo.ledger3.html"
    editor = $(if($Script:isWindowsPlatform){"wordpad.exe"}else{"xdg-open"})
    browser = ""
  }
}

function GetLiveDemoConfigExtension {
  [CmdletBinding()]
  Param()

  $private:defaultSettings = GetDefaultDemoSettings

  return [PsCustomObject]@{
    Category=".Net Ledger Live Demo Settings"
    Settings=@(
      [PsCustomObject]@{Name="DemoURL";Description="Live Demo URL. Allows to specify an alternative port or machine name.";Default=$private:defaultSettings.demoURL}
      [PsCustomObject]@{Name="DemoEditor";Description="The name of an editor that is used to modify Live Demo data files.";Default=$private:defaultSettings.editor}
      [PsCustomObject]@{Name="DemoBrowser";Description="The name of a browser to show Live Demo page. Empty value means a default browser. Allowed short names 'edge', 'chrome', 'firefox', 'iexplore' or a fully-qualified executable name.";Default=$private:defaultSettings.browser}
  )}
}

# Register Live Demo Configuration extension in NLSetup
$Global:ConfigDataExtensions["LiveDemo"] = GetLiveDemoConfigExtension

function GetDemoConfig {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$liveDemoConfig = "$Script:ScriptPath\NLDoc.LiveDemo.config.xml"
  )

  $private:settings = GetDefaultDemoSettings
  $private:configData = GetConfigData
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

function openPage {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$demoConfig
  )

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

function InitDemoSandbox {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$files,
    [Parameter(Mandatory=$True)][string]$sandbox
  )

  if (!(Test-Path $sandbox -PathType Container)) { New-Item -ItemType Directory -Force -Path $sandbox }
  foreach($key in $files.Keys) {
    [string]$sourceFile = Resolve-Path (Join-Path $Script:ScriptPath $files[$key]) -ErrorAction Stop
    [string]$targetFile = "$sandbox$Script:dsp$key"
    if (!(Test-Path $sourceFile -PathType Leaf)) { throw "Source file does not exist: $sourceFile" }
    if (!(Test-Path $targetFile -PathType Leaf)) { Copy-Item -LiteralPath $sourceFile -Destination $targetFile | Out-Null }
  }

}

function RevertDemoSandboxFile {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$files,
    [Parameter(Mandatory=$True)][string]$fileName,
    [Parameter(Mandatory=$True)][string]$sandbox
  )

  if (!(Test-Path $sandbox -PathType Container)) { throw "No sandbox folder" }
  [string]$sourceFile = Resolve-Path (Join-Path $Script:ScriptPath $files[$fileName]) -ErrorAction Stop
  [string]$targetFile = "$sandbox$Script:dsp$key"
  if (!(Test-Path $sourceFile -PathType Leaf)) { throw "Source file does not exist: $sourceFile" }
  Copy-Item -LiteralPath $sourceFile -Destination $targetFile -Force | Out-Null
}

function GetContent {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$demoFile
  )

  if (!(Test-Path -LiteralPath $demoFile -PathType Leaf)) { throw "File not found: $demoFile" }
  return Get-Content -Encoding UTF8 -LiteralPath $demoFile | Out-String
}

function actRun {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$testid,
    [Parameter(Mandatory=$True)]$demoConfig
  )

  Write-Verbose "Requested app run for test $testid"

  $private:command = $demoConfig.Commands[$testid]
  if ($private:command) {
    $env:CMDTORUN = $private:command
    Write-Verbose "Command to execute (CMDTORUN): $env:CMDTORUN"
    if ($Script:isWindowsPlatform) {
      $private:cmd = "/k """"$ScriptPath\NLDoc.LiveDemo.Launch.cmd"" $private:command"""
      $env:CMDTOVIEW = $private:command.Replace("^","^^")
      Write-Verbose "Command to view (CMDTOVIEW): $env:CMDTOVIEW"
      Write-Verbose "Start process - working directory: $($demoConfig.Sandbox); file path: $env:comspec; ArgumentList: $private:cmd"
      start-process -workingdirectory $demoConfig.Sandbox -filepath "$env:comspec" -ArgumentList $private:cmd  
    } else {
      if($isOsxPlatform) {
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

function actEdit {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$testid,
    [Parameter(Mandatory=$True)]$demoConfig
  )

  Write-Verbose "Requested edit data file for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {
    if($isOsxPlatform) {
      Start-process -FilePath "open" -ArgumentList "-t $($demoConfig.Sandbox)/$private:file"
    } else {
      start-process -WorkingDirectory $demoConfig.Sandbox -FilePath $demoConfig.Editor $private:file
    }
    return "OK: $testid"
  } else {
    return "FAIL: file not found - $testid"
  }
}

function actOpen {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$testid,
    [Parameter(Mandatory=$True)]$demoConfig
  )

  Write-Verbose "Requested opening containing folder for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {    
    if ($Script:isWindowsPlatform) {
      start-process -WorkingDirectory $demoConfig.Sandbox -FilePath "explorer" "/e, /select, `"$($demoConfig.Sandbox.Replace('/','\'))\$private:file`""
    }
    else {
      if($isOsxPlatform) {
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

function actRevert {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$testid,
    [Parameter(Mandatory=$True)]$demoConfig
  )

  Write-Verbose "Requested reverting data file for test $testid"

  $private:file = $demoConfig.TestFiles[$testid]
  if ($private:file) {
    RevertDemoSandboxFile -fileName $private:file -sandbox $demoConfig.Sandbox -files $demoConfig.Files | Out-Null
    return "OK: $testid"
  } else {
    return "FAIL: file not found - $testid"
  }
}

function WriteDemoConfig {
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
    $private:xmlFile.SetAttribute("name", $private:file) | Out-Null
    $private:xmlFile.SetAttribute("path", $demoFiles[$private:file]) | Out-Null
    $private:xmlFiles.AppendChild($private:xmlFile) | Out-Null
  }

  $private:xmlCases = ($private:xml.'ledger-demo').GetElementsByTagName("demo-cases")
  foreach($private:case in $demoCases) {
    $private:xmlCase = $private:xml.CreateElement("case")
    $private:xmlCase.SetAttribute("test-id", $private:case.TestID) | Out-Null
    $private:xmlCase.SetAttribute("command", $private:case.Command) | Out-Null
    $private:xmlCase.SetAttribute("filename", $private:case.FileName) | Out-Null
    $private:xmlCases.AppendChild($private:xmlCase) | Out-Null
  }

  $private:xml.Save($fileName) | Out-Null
}


Export-ModuleMember -function * -alias *
