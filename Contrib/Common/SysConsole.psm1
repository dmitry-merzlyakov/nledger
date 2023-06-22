 <#
.SYNOPSIS
Helper functions for interacting with the console

.DESCRIPTION
Provides the ability to output colorized text to the terminal
#> 

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

<#
.SYNOPSIS
    Replaces color tokens in the input text with ANSI escape code
.DESCRIPTION
    Console output can be colorized by adding ANSI escape codes into the output text.
    Adding control codes directly to text can be tricky; easier to interact with mnemonic color names.
    This function converts specially formatted tokens to escape code so you can colorize your text in an easy way.

    The token format is {action:color} where action is a letter 'b', 'c' or 'f':
    - 'b' means Background color (possible color values are Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, 
          DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White)
    - 'c' means Foreground color (possible color values as above)
    - 'f' means Format and can be specified as Normal, Bold, Underline, Blink

    The output string contains corresponded ANSI escape code and ready for output to the terminal.

    The colorization can be globally disabled by setting a variable $Global:ANSI_Colorization to $False.
    This variable is $True by default except Windows OS older than version 10 (where support of ANSI code is limited).
.PARAMETER inputString
    The input string containing color tokens.
.EXAMPLE
    Convert-ColorTokensToAnsi "{c:Green}Colored{f:Normal} text"
    Prints Colored word highlighted Green
.EXAMPLE
    Convert-ColorTokensToAnsi "plan text"
    Prints a plain text without colorization
.NOTES
    It is recommended to reset the colorization at the end of the text using {f:Normal} token.
    This will restore the original background and foreground terminal colors, so further output will match the original color schema.
#>
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

<#
.SYNOPSIS
    Replaces color tokens in the pipeline with ANSI escape code
.DESCRIPTION
    Functionally equal to Convert-ColorTokensToAnsi but can be easily used in pipelines.
    You can colorize the output just by adding it after your text and a pipe.
.EXAMPLE
    "{c:Green}Colored{f:Normal} text" | Out-AnsiString
    Prints Colored word highlighted Green
#>
function Out-AnsiString {
  Process { Convert-ColorTokensToAnsi $_ }
}

Export-ModuleMember -function * -alias *