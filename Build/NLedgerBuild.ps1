# This script opens a console that allows to run the build with various options.
# Use the following command if you need to enable scripts on your machine:
#   set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$msBuildPath,
    [Parameter(Mandatory=$False)][string]$vsTestPath,
    [Parameter(Mandatory=$False)][string]$buildTarget = "All",
    [Switch][bool]$interactive = $False,
    [Switch][bool]$elevate = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
cd $Script:ScriptPath

[string]$Script:projPath = Resolve-Path (Join-Path $Script:ScriptPath ".\NLedgerBuild.proj") -ErrorAction Stop
[string]$Script:outRootPath = Resolve-Path (Join-Path $Script:ScriptPath "..") -ErrorAction Stop

function FindMsBuildLocation {
  [CmdletBinding()]
  Param()
  [string]$Private:msbuildPath = ""
  foreach($Private:version in ("2.0", "3.5", "4.0", "12.0", "14.0")) {
     $Private:value = Get-ItemProperty -path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\$Private:version"
     if ($Private:value) { $Private:msbuildPath = "$($Private:value.MSBuildToolsPath)MSBuild.exe" }
  }
  return $Private:msbuildPath
}

function FindVsTestLocation {
  [CmdletBinding()]
  Param()
  [string]$Private:vsTestPath = ""
  foreach($Private:version in ("110", "120", "140")) {
     $Private:value = [environment]::GetEnvironmentVariable("VS$($Private:version)COMNTOOLS","Process")
     if ($Private:value -and (Test-Path $Private:value -PathType Container)) { $Private:vsTestPath = "$Private:value..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" }
  }
  return $Private:vsTestPath
}

function WrapArg {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][AllowEmptyString()][string]$argText
  )
  if ($argText -and ($argText -match "\s")) { $argText = "`"$argText`"" }
  return $argText
}

if (!$msBuildPath) { $msBuildPath = FindMsBuildLocation }
if (!$vsTestPath) { $vsTestPath = FindVsTestLocation }
if (!(Test-Path $msBuildPath -PathType Leaf)) { throw "Cannot find MSBUILD path" }
if (!(Test-Path $vsTestPath -PathType Leaf)) { throw "Cannot find VS Test Console path" }

function RunBuild {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$private:target,
    [Parameter(Mandatory=$True)][bool]$private:elevate
  )
  [string]$outputDir = "$Script:outRootPath\Build-$(get-date -f yyyyMMdd-HHmmss)\"
  New-Item -Path $outputDir -ItemType Directory | Out-Null
  Write-Host -ForegroundColor Yellow "New build folder: $outputDir"
  Write-Host -ForegroundColor Yellow "Check the content of this folder once build process is finished"

  if ($private:elevate) {
    $Script:argList = @("$(WrapArg $Script:projPath)", "/t:$private:target", "/property:Elevated=True", "/property:VsTestConsolePath=$(WrapArg $vsTestPath)", "/property:OutputDir=$(WrapArg $outputDir)", "/fl1", "/flp1:logfile=$(WrapArg "$($outputDir)NLedgerBuild.log");verbosity=minimal", "/fl2", "/flp2:logfile=$(WrapArg "$($outputDir)NLedgerBuild.detail.log");verbosity=diagnostic" )
    $private:process = (Start-Process $msBuildPath -Wait -Verb RunAs -ArgumentList ($Script:argList))
  } else {
    [string]$private:logfile1 = """$($outputDir)NLedgerBuild.log"";verbosity=minimal"
    [string]$private:logfile2 = """$($outputDir)NLedgerBuild.detail.log"";verbosity=diagnostic"
    & $msBuildPath "$Script:projPath" /t:$private:target /property:VsTestConsolePath="$vsTestPath" /property:OutputDir="$outputDir" /fl1 /flp1:logfile=$private:logfile1 /fl2 /flp2:logfile=$private:logfile2
  }

  Write-Host -ForegroundColor Yellow "Build is finished; check the content of the build folder: $outputDir"
}


function all {
    [CmdletBinding()]
    Param()
    RunBuild "All" $False
}

function all-elevated {
    [CmdletBinding()]
    Param()
    RunBuild "All" $True
}

function build {
    [CmdletBinding()]
    Param()
    RunBuild "Build" $False
}

function codetest {
    [CmdletBinding()]
    Param()
    RunBuild "CodeTests" $False
}

function release {
    [CmdletBinding()]
    Param()
    RunBuild "Release" $False
}

function test {
    [CmdletBinding()]
    Param()
    RunBuild "IntegrationTests" $False
}

function test-elevated {
    [CmdletBinding()]
    Param()
    RunBuild "IntegrationTests" $True
}

function package {
    [CmdletBinding()]
    Param()
    RunBuild "Package" $False
}

function help {
[CmdletBinding()]
Param()
     write-host "Interactive console helps you compile, test and build NLedger with various options`r`n"
     write-host -NoNewline "It supports several short commands that run the build process with typical targets:`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "all"
     write-host -NoNewline "            all build steps (compile, test, package). Does not require admin proviledges (longer build time);`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "all-elevated"
     write-host -NoNewline "   all build steps with admin priviledges (testing is faster because of possibility to create native images);`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "build"
     write-host -NoNewline "          compile NLedger code; no other steps;`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "codetest"
     write-host -NoNewline "       compile and execute unit and internal integration tests;`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "release"
     write-host -NoNewline "        compile and build a package; no testing;`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "test"
     write-host -NoNewline "           compile, create a package and run Ledger tests. Does not require admin priviledges (longer testing time);`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "test-elevated"
     write-host -NoNewline "  compile, create a package and run Ledger tests. Requires admin proviledges (faster testing time);`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "package"
     write-host -NoNewline "        compilea and build a package; no testing;`r`n PS> "
     write-host -NoNewline -ForegroundColor Yellow "help"
     write-host -NoNewline "           show this help page again.`r`n"
     write-host -NoNewline "`r`nNote: some build targets require administrative priviledges because they try to create NLedger native images by means of NGen."
     write-host -NoNewline "`r`nIt significatnly shortens testing time (from several minutes to tens of seconds)."
     write-host -NoNewline "`r`nIf it is inacceptable on this environment, use non-elevating targets.`r`n"
     write-host -NoNewline "`r`nType '"
     write-host -NoNewline -ForegroundColor Yellow "exit"
     write-host "' or close the console window when you finish.`r`n"
}

if (!$interactive) {
    RunBuild $buildTarget $elevate
} else {
    Write-Host -ForegroundColor White "NLedger Build Console"
    help
}