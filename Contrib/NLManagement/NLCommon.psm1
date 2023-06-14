# NLedger Management Module that shares a common routine functions with others

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../NLManagement/NLWhere.psm1 -Force

# ANSI Terminal colorization

[bool]$Global:ANSI_Colorization = !([System.Environment]::OSVersion.Platform -eq "Win32NT" -and [System.Environment]::OSVersion.Version.Major -lt 10)

[string]$ESC = [char]27
[string]$Global:NormalColor = "$($ESC)[0m"
[string]$Global:ForegroundBold = "$($ESC)[1m"
[string]$Global:ForegroundUnderline = "$($ESC)[4m"
[string]$Global:ForegroundBlink = "$($ESC)[5m"

[string]$Global:BackgroundColorBlack = "$($ESC)[40m"
[string]$Global:BackgroundColorRed = "$($ESC)[41m"
[string]$Global:BackgroundColorGreen = "$($ESC)[42m"
[string]$Global:BackgroundColorYellow = "$($ESC)[43m"
[string]$Global:BackgroundColorBlue = "$($ESC)[44m"
[string]$Global:BackgroundColorMagenta = "$($ESC)[45m"
[string]$Global:BackgroundColorCyan = "$($ESC)[46m"
[string]$Global:BackgroundColorWhite = "$($ESC)[47m"

[string]$Global:BackgroundColorBoldBlack = "$($ESC)[40;1m"
[string]$Global:BackgroundColorBoldRed = "$($ESC)[41;1m"
[string]$Global:BackgroundColorBoldGreen = "$($ESC)[42;1m"
[string]$Global:BackgroundColorBoldYellow = "$($ESC)[43;1m"
[string]$Global:BackgroundColorBoldBlue = "$($ESC)[44;1m"
[string]$Global:BackgroundColorBoldMagenta = "$($ESC)[45;1m"
[string]$Global:BackgroundColorBoldCyan = "$($ESC)[46;1m"
[string]$Global:BackgroundColorBoldWhite = "$($ESC)[47;1m"

[string]$Global:ForegroundColorBlack = "$($ESC)[30m"
[string]$Global:ForegroundColorRed = "$($ESC)[31m"
[string]$Global:ForegroundColorGreen = "$($ESC)[32m"
[string]$Global:ForegroundColorYellow = "$($ESC)[33m"
[string]$Global:ForegroundColorBlue = "$($ESC)[34m"
[string]$Global:ForegroundColorMagenta = "$($ESC)[35m"
[string]$Global:ForegroundColorCyan = "$($ESC)[36m"
[string]$Global:ForegroundColorWhite = "$($ESC)[37m"

[string]$Global:ForegroundColorBoldBlack = "$($ESC)[30;1m"
[string]$Global:ForegroundColorBoldRed = "$($ESC)[31;1m"
[string]$Global:ForegroundColorBoldGreen = "$($ESC)[32;1m"
[string]$Global:ForegroundColorBoldYellow = "$($ESC)[33;1m"
[string]$Global:ForegroundColorBoldBlue = "$($ESC)[34;1m"
[string]$Global:ForegroundColorBoldMagenta = "$($ESC)[35;1m"
[string]$Global:ForegroundColorBoldCyan = "$($ESC)[36;1m"
[string]$Global:ForegroundColorBoldWhite = "$($ESC)[37;1m"

$Script:AnsiBackgroundColorMapping = @{
  Black       = $Global:BackgroundColorBlack
  DarkBlue    = $Global:BackgroundColorBlue
  DarkGreen   = $Global:BackgroundColorGreen
  DarkCyan    = $Global:BackgroundColorCyan
  DarkRed     = $Global:BackgroundColorRed
  DarkMagenta = $Global:BackgroundColorMagenta
  DarkYellow  = $Global:BackgroundColorYellow
  Gray        = $Global:BackgroundColorWhite
  DarkGray = $Global:BackgroundColorBoldBlack
  Blue     = $Global:BackgroundColorBoldBlue
  Green    = $Global:BackgroundColorBoldGreen
  Cyan     = $Global:BackgroundColorBoldCyan
  Red      = $Global:BackgroundColorBoldRed
  Magenta  = $Global:BackgroundColorBoldMagenta
  Yellow   = $Global:BackgroundColorBoldYellow
  White    = $Global:BackgroundColorBoldWhite
}

$Script:AnsiForegroundColorMapping = @{
  Black       = $Global:ForegroundColorBlack
  DarkBlue    = $Global:ForegroundColorBlue
  DarkGreen   = $Global:ForegroundColorGreen
  DarkCyan    = $Global:ForegroundColorCyan
  DarkRed     = $Global:ForegroundColorRed
  DarkMagenta = $Global:ForegroundColorMagenta
  DarkYellow  = $Global:ForegroundColorYellow
  Gray        = $Global:ForegroundColorWhite
  DarkGray = $Global:ForegroundColorBoldBlack
  Blue     = $Global:ForegroundColorBoldBlue
  Green    = $Global:ForegroundColorBoldGreen
  Cyan     = $Global:ForegroundColorBoldCyan
  Red      = $Global:ForegroundColorBoldRed
  Magenta  = $Global:ForegroundColorBoldMagenta
  Yellow   = $Global:ForegroundColorBoldYellow
  White    = $Global:ForegroundColorBoldWhite
}

$Script:AnsiFormatColorMapping = @{
  Normal    = $Global:NormalColor
  Bold      = $Global:ForegroundBold
  Underline = $Global:ForegroundUnderline
  Blink     = $Global:ForegroundBlink
}

$Script:AnsiColorMapping = @{
  b = $Script:AnsiBackgroundColorMapping
  c = $Script:AnsiForegroundColorMapping
  f = $Script:AnsiFormatColorMapping
}

function Convert-ColorTokensToAnsi {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][AllowEmptyString()][string]$inputString
  )

  $private:foundMatches = @((Select-String -InputObject $inputString '{(?<type>c|b|f):(?<color>\w+)}' -AllMatches).Matches)
  [array]::Reverse($private:foundMatches)
  foreach($private:match in $private:foundMatches) {
    if ($private:match.Groups) {
      [string]$private:type = $private:match.Groups[1].Value
      [string]$private:color = $private:match.Groups[2].Value
      [int]$private:pos = $private:match.Groups[0].Index
      [int]$private:length = $private:match.Groups[0].Length
  
      if ($Global:ANSI_Colorization) {
        $Private:mapping = $Script:AnsiColorMapping[$private:type]
        $Private:ansiCode = $Private:mapping[$private:color]
        if (!($Private:ansiCode)) { throw "Invalid color in token: $($Private:color)" }  
      } else { $Private:ansiCode = "" }
  
      $inputString = $inputString.Remove($private:pos, $private:length)
      $inputString = $inputString.Insert($private:pos, $Private:ansiCode)  
    }
  }

  return $inputString
}

function Out-AnsiString {
  Process { Convert-ColorTokensToAnsi $_ }
}

# Helper function that returns a structure indicating a failure
function Get-Fault {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$message
    )

    return @{ Status=$False; Message=$message }
}

# Helper function that returns a structure indicating a success
function Get-Success {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$message
    )

    return @{ Status=$True; Message=$message }
}

# Helper method that print a structure indicator either success or failure
function PrintStatusResponse {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$response
  )
  if ($response.Status) { Write-Console "{c:green}[OK]{c:white} $($response.Message)" | Out-Null } else { Write-Console "{c:red}[ERROR]{c:white} $($response.Message)" | Out-Null }
}


# Helper method that wraps up "write-console"; it can manage nulls in color values.
function Write-ConsoleColors {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)][AllowEmptyString()][string]$inputString,
     [Parameter(Mandatory=$False)]$foregroundColor,
     [Parameter(Mandatory=$False)]$backgroundColor,
     [Switch]$NoNewLine = $False
  )
  $private:colors = @{}
  if ($foregroundColor) { $private:colors.ForegroundColor = $foregroundColor }
  if ($backgroundColor) { $private:colors.BackgroundColor = $backgroundColor }

  Write-Host $inputString -NoNewline:$NoNewLine @private:colors
}

<#
.SYNOPSIS
    Powershell enhanced colorization of console output.
.DESCRIPTION
    Enhanced version of 'write-host' command that allows adding
    colorization tokens into the input string.
    Token format is: '{c:console_color_name}' or '{b:console_color_name}'
    The former changes foreground color of further string; the latter changes the background.
    Console color name matches the names specified in System.ConsoleColor enum. Case insensitive.
.PARAMETER inputString
    Input string that can contain colorization tokens.
.PARAMETER NoNewLine
    Switcher that indicates whether the output should end up with a new line symbol.
.EXAMPLE
    C:\PS> Write-Console "This text {c:yellow}can{c:white} be colorized"
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 04, 2018    
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    
function Write-Console {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)][AllowEmptyString()][string]$inputString,
     [Switch]$NoNewLine = $False
  )

  $private:BackgroundColor = $null
  $private:ForegroundColor = $null
  [int]$private:beginPos = 0
  
  $private:foundMatches = (Select-String -InputObject $inputString '{(?<type>c|b):(?<color>\w+)}' -AllMatches)
  foreach($private:match in $private:foundMatches.Matches) {

    [string]$private:type = $private:match.Groups[1].Value
    [string]$private:color = $private:match.Groups[2].Value
    [int]$private:pos = $private:match.Groups[0].Index
    [int]$private:length = $private:match.Groups[0].Length
    [string]$private:completeValue = $private:match.Groups[0].Value
    Write-Verbose "Found color token '$private:completeValue': type:'$private:type' color:'$private:color' pos:'$private:pos' length:'$private:length'"

    [string]$private:previousPart = $inputString.Substring($private:beginPos,$private:pos-$private:beginPos)
    Write-ConsoleColors -NoNewline -inputString $private:previousPart -backgroundColor $private:BackgroundColor -foregroundColor $private:ForegroundColor

    if ($private:type -eq "c") { $private:ForegroundColor = [ConsoleColor]::Parse([ConsoleColor], $private:color, $True) }
    if ($private:type -eq "b") { $private:BackgroundColor = [ConsoleColor]::Parse([ConsoleColor], $private:color, $True) }
    $private:beginPos = $private:pos + $private:length
  }

  [string]$private:previousPart = $inputString.Substring($private:beginPos)
  Write-ConsoleColors -NoNewline:$NoNewLine -inputString $private:previousPart -backgroundColor $private:BackgroundColor -foregroundColor $private:ForegroundColor
}

function Get-ColoredStringLength {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)][AllowEmptyString()][string]$inputString
  )
  $inputString = $inputString -replace '{(?<type>c|b):(?<color>\w+)}',""
  return $inputString.Length
}


function Add-TextTableColumn {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)][string]$property,
     [Parameter(Mandatory=$False)][string]$header = "",
     [Parameter(Mandatory=$False)][int]$width = 0
  )
  return [PsCustomObject]@{ Property=$property; Header=$header; Width=$width }
}

function Write-TextTable {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)]$inputData,
     [Parameter(Mandatory=$True)]$columns,
     [Parameter(Mandatory=$False)]$headerBackground = $null,
     [Parameter(Mandatory=$False)]$separatorColor = $null
  )

  foreach($private:column in $columns | ?{$_.Width -eq 0}) {
    # Get max value length
    $private:lengths = ($inputData | select -ExpandProperty $private:column.Property | %{ (Get-ColoredStringLength $_) })
    $private:lengths += (Get-ColoredStringLength $private:column.Header)
    $private:column.Width = ( $private:lengths | measure-object -Maximum).Maximum
  }

  $private:headerBackgroundPrefix = if ($headerBackground) { "{b:$headerBackground}"} else { "" }  
  $private:separatorColorPrefix = if ($separatorColor) { "{c:$separatorColor}"} else { "" }  

  # Print header
  [int]$private:lastIndex = $columns.count - 1
  [string]$private:output = ""
  for($private:index=0; $private:index -le $private:lastIndex; $private:index++) {
    [bool]$private:isLast = $private:index -eq $private:lastIndex
    $private:column = $columns[$private:index]
    $private:width = $private:column.Width + ($private:column.Header.length - (Get-ColoredStringLength $private:column.Header))
    #Write-Console -NoNewLine:$(!$private:isLast) "$private:headerBackgroundPrefix $($private:column.Header.PadRight($private:width)) $(if($private:isLast) {''} else {"$private:separatorColorPrefix|"} )"
    $private:output += "$private:headerBackgroundPrefix $($private:column.Header.PadRight($private:width)) $(if($private:isLast) {''} else {"$private:separatorColorPrefix|"} )"
  }
  Write-Console $private:output

  # Print data rows
  foreach($private:data in $inputData) {
      [int]$private:lastIndex = $columns.count - 1
      [string]$private:output = ""
      for($private:index=0; $private:index -le $private:lastIndex; $private:index++) {        
        [bool]$private:isLast = $private:index -eq $private:lastIndex
        $private:column = $columns[$private:index]
        [string]$private:colValue = ($private:data |  select -ExpandProperty $private:column.Property)
        $private:width = $private:column.Width + ($private:colValue.length - (Get-ColoredStringLength $private:colValue))
        #Write-Console -NoNewLine:$(!$private:isLast) " $($private:colValue.PadRight($private:width)) $(if($private:isLast) {''} else {"$private:separatorColorPrefix|"} )"
        $private:output += " $($private:colValue.PadRight($private:width)) $(if($private:isLast) {''} else {"$private:separatorColorPrefix|"} )"
      }
      Write-Console $private:output
  }

}


function Write-Columns {
  [CmdletBinding()]
  Param(
     [Parameter(Mandatory=$True)]$inputData,
     [Parameter(Mandatory=$False)][int]$leftMargin = 4,
     [Parameter(Mandatory=$False)][int]$rightBoundary = 80,
     [Parameter(Mandatory=$False)][int]$interval = 2,
     [Parameter(Mandatory=$False)][int]$firstRowLeftMargin = 0
  )

  if (!$inputData) { return }

  $private:width = $rightBoundary - $leftMargin
  if ($private:width -le 0) { throw "Too small width; check rightBoundary and/or leftMargin" }

  $private:maxLength = $interval + ($inputData | %{ (Get-ColoredStringLength $_) } | measure-object -Maximum).Maximum
  if ($private:width -lt $private:maxLength) { throw "Too small width; data rows should be shorter" }

  [int]$private:colNum = [math]::truncate($private:width / $private:maxLength)
  [int]$private:rowNum = [math]::truncate((($inputData | measure-object).Count - 1) / $private:colNum) + 1

  for($private:row=0;$private:row -lt $private:rowNum; $private:row++) {
    [string]$private:output = "".PadLeft($leftMargin)
    if ($private:row -eq 0) { $private:output = "".PadLeft($firstRowLeftMargin) }
    for($private:col=0;$private:col -lt $private:colNum; $private:col++) {
        [string]$private:cell = $inputData[$private:row + ($private:col * $private:rowNum)]
        $private:output += $private:cell.PadRight($private:maxLength)
    }
    Write-Console $private:output
  }
}


<#
.SYNOPSIS
    NLedger Management - determining NLedger binaries location.
.DESCRIPTION
    Detects the deployment profile (production, development/debug, development/release) 
    and returns path to the most relevant NLedger binaries.
    If it detects multiple folders with binaries, it prefers the folder with the latest files.
#>
function NLedgerLocation {
  [CmdletBinding()]
  Param()

  $private:path = Get-NLedgerPath -preferCore:$false
  if (!$private:path) { throw "Cannot find NLedger executable file" }
  return [System.IO.Path]::GetDirectoryName($private:path)
}


<#
.SYNOPSIS
    Determines whether the current OS is Windows
.DESCRIPTION
    Returns True if the current OS is Windows or returns False otherwise.
#>
function Test-IsWindows {
  [CmdletBinding()]
  Param()

  [bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
}

<#
.SYNOPSIS
    Determines whether the current OS is OSX
.DESCRIPTION
    Returns True if the current OS is OSX or returns False otherwise.
#>
function Test-IsOSX {
  [CmdletBinding()]
  Param()

  [bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
}

<#
.SYNOPSIS
    Determines whether the current OS is Linux
.DESCRIPTION
    Returns True if the current OS is Linux or returns False otherwise.
#>
function Test-IsLinux {
  [CmdletBinding()]
  Param()

  [bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
}

<#
.SYNOPSIS
    Returns a full path to ProgramFiles(x86) folder
.DESCRIPTION
    Only for Windows OS
#>
function Get-SpecialFolderProgramFilesX86 {
  [CmdletBinding()]
  Param()

  if(Test-IsWindows){[System.Environment]::GetFolderPath(([System.Environment+SpecialFolder]::ProgramFilesX86))}else{""}
}

<#
.SYNOPSIS
    Returns a list of installed .Net Framework SDKs as a collection of versions
.DESCRIPTION
    On windows machines, installed .Net Framework SDKs can be identified by subfolders in %ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\
#>
function Get-NetFrameworkSDKs {
  [CmdletBinding()]
  Param()

  if (Get-SpecialFolderProgramFilesX86) {
    (Get-ChildItem -Path "$(Get-SpecialFolderProgramFilesX86)/Reference Assemblies/Microsoft/Framework/.NETFramework" -Directory) | 
      Where-Object { $_ -match "v\d+.\d+(.\d+)?" } |
      ForEach-Object { [version]($_ | Select-String -Pattern "\d+.\d+(.\d+)?").Matches.Value }
  }
}

<#
.SYNOPSIS
    Returns a list of available .Net Framework SDK targets (TFM codes)
.DESCRIPTION
    Transforms a collection of installed .Net Framework SDLs into TFM codes
#>
function Get-NetFrameworkSdkTargets {
  [CmdletBinding()]
  Param()

  (Get-NetFrameworkSDKs) | ForEach-Object { if($_.Build -eq -1) {"net$($_.Major)$($_.Minor)"} else {"net$($_.Major)$($_.Minor)$($_.Build)"} }
}

<#
.SYNOPSIS
    Determines whether dotnet is installed on the current machine.
.DESCRIPTION
    Returns True if "dotnet" command is available on the current machine.
#>
function Test-IsDotnetInstalled {
  [CmdletBinding()]
  Param()

  [bool]-not(-not($(get-command dotnet -ErrorAction SilentlyContinue)))
}

<#
.SYNOPSIS
    Determines currently installed dotnet SDKs
.DESCRIPTION
    Returns the list of versions of every installed SDK
#>
function Get-DotnetSdks {
  [CmdletBinding()]
  Param()

  $(dotnet --list-sdks) | ForEach-Object { [version]($_ | Select-String -Pattern "^\d+.\d+.\d+").Matches.Value }
}

<#
.SYNOPSIS
    Provides available dotnet build targets
.DESCRIPTION
    Returns the list of build targets (TFM codes) that correspond to the list of installed dotnet SDKs
#>
function Get-DotnetSdkTargets {
  [CmdletBinding()]
  Param()

  Get-DotnetSdks | ForEach-Object { if($_.Major -lt 5) {"netcoreapp$($_.Major).$($_.Minor)"} else {"net$($_.Major).$($_.Minor)"} }
}

<#
.SYNOPSIS
    Checks whether given string is a valid TFM code
.DESCRIPTION
    Supports .Net Core/5.0+ and .Net Framework codes
#>
function Test-IsTfmCode {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$code)

  [bool]($code -match "(net|netcoreapp)\d+(\.\d+)?")
}

<#
.SYNOPSIS
    Checks whether given TFM code is about .Net Framework target
.DESCRIPTION
    Supports all .Net Framework codes
#>
function Test-IsFrameworkTfmCode {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$code)

  [bool]($code -match "^net\d+$")
}

# For internal usage
function Get-NLedgerBins {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$folderName,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  if (Test-Path -LiteralPath $folderName -PathType Container) {
    Write-Verbose "Get-NLedgerBins - collecting info from $folderName folder."
    foreach($targetFolder in (Get-ChildItem -LiteralPath $folderName -Directory) | Where-Object { Test-IsTfmCode $_.Name }) {
      [string]$tfmCode = $targetFolder.Name
      [string]$tfmFolder = $targetFolder.FullName
      [bool]$isOsxRuntime = $false

      if (Test-Path -LiteralPath "$tfmFolder/osx-x64" -PathType Container) { 
        $tfmFolder = "$tfmFolder/osx-x64"
        $isOsxRuntime = $true
      }

      [string]$nledgerFile = "$tfmFolder/NLedger-cli.exe"
      if (!(Test-Path -LiteralPath $nledgerFile -PathType Leaf)) {
        $nledgerFile = "$tfmFolder/NLedger-cli"
        if (!(Test-Path -LiteralPath $nledgerFile -PathType Leaf)) { continue }
      }

      Write-Verbose "Found executable NLedger: $nledgerFile (TFM: $tfmCode; Debug: $isDebug; OSX: $isOsxRuntime)"
      [PSCustomObject]@{
        NLedgerExecutable = [System.IO.Path]::GetFullPath($nledgerFile)
        TfmCode = $tfmCode
        IsDebug = $isDebug
        IsOsxRuntme = $isOsxRuntime
      }

    }
  } else {
    Write-Verbose "Get-NLedgerBins - folder $folderName does not exist."
  }
}

<#
.SYNOPSIS
    Provides information about available NLedger binaries
.DESCRIPTION
    Returns a collection of objects containing information about local NLedger binaries.
    Every info object has a full path to NLedger executable file, corresponded TFM code and Debug/Release flag.
    The function looks for files in the current file structure 
    (either development structure that is a copy of the repository or public binaries distructured by means of zip archive or MSI file).
#>
function Get-NLedgerBinaryInfos {
  [CmdletBinding()]
  Param()

  [string]$devDebugBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Debug")
  [string]$devReleaseBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Release")
  [string]$publicBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../bin")

  return  (Get-NLedgerBins -folderName $devDebugBin -isDebug $true) +
          (Get-NLedgerBins -folderName $devReleaseBin -isDebug $false) +
          (Get-NLedgerBins -folderName $publicBin -isDebug $false)
}

<#
.SYNOPSIS
    Provides information about a specific NLedger binary file
.DESCRIPTION
    Looks for a specific NLedger instance in a collection of available local binaries
    using specified TFM code and Debug/Release flag
#>
function Get-NLedgerBinaryInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  return  Get-NLedgerBinaryInfos | Where-Object { $_.TfmCode -eq $tfmCode -and $_.IsDebug -eq $isDebug }
}




Export-ModuleMember -function * -alias *