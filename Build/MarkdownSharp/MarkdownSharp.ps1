<#
.SYNOPSIS
    Converts Markdown (.md) files to HTML format
.DESCRIPTION
    This is a binding to MarkdownSharp library (https://code.google.com/archive/p/markdownsharp/)
    See the referenced link to get further information about its features.
.PARAMETER inputFileName
    Relative or absolute path to a markdown file to convert.
    In case of a relative path, the tool uses the own location folder as a base.
.PARAMETER outputFileName
    Relative or absolute path to an output HTML file.
    If this parameter is omitted, the scripts creates an output file at the same folder
    and with teh same name as the input file but adds .html extension.
    In case of a relative path, the tool uses the current directory as a base.
.PARAMETER autoHyperlink
    when true, (most) bare plain URLs are auto-hyperlinked  
    WARNING: this is a significant deviation from the markdown spec
.PARAMETER autoNewlines
    when true, RETURN becomes a literal newline  
    WARNING: this is a significant deviation from the markdown spec
.PARAMETER emptyElementSuffix
    use ">" for HTML output, or " />" for XHTML output
.PARAMETER encodeProblemUrlCharacters
    when true, problematic URL characters like [, ], (, and so forth will be encoded 
    WARNING: this is a significant deviation from the markdown spec
.PARAMETER linkEmails
    when false, email addresses will never be auto-linked  
    WARNING: this is a significant deviation from the markdown spec
.PARAMETER strictBoldItalic
    when true, bold and italic require non-word characters on either side  
    WARNING: this is a significant deviation from the markdown spec
.EXAMPLE
    C:\PS> .\MarkdownSharp.ps1 -inputFileName readme.md
    Converts readme.md to readme.md.html
.NOTES
    Author: Dmitry Merzlyakov
    Date:   December 26, 2017    
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    

[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True)][string]$inputFileName,
  [Parameter(Mandatory=$False)][string]$outputFileName = "",
  [Switch][bool]$autoHyperlink = $False,
  [Switch][bool]$autoNewlines = $False,
  [Parameter(Mandatory=$False)][string]$emptyElementSuffix = "",
  [Switch][bool]$encodeProblemUrlCharacters = $False,
  [Switch][bool]$linkEmails = $False,
  [Switch][bool]$strictBoldItalic = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[System.Reflection.Assembly]::LoadFile("$Script:ScriptPath\MarkdownSharp.dll") | Out-Null

[string]$Script:absInputFileName = if (![System.IO.Path]::IsPathRooted($inputFileName)) { Resolve-Path (Join-Path $Script:ScriptPath $inputFileName) -ErrorAction Stop } else { $inputFileName }
if (!(Test-Path $Script:absInputFileName -PathType Leaf)) { throw "Cannot find input file: $Script:absInputFileName" }
[string]$Script:inputText = (Get-Content $Script:absInputFileName -Encoding UTF8 | Out-String).Trim()

if ($outputFileName -eq "") { $outputFileName = "$Script:absInputFileName.html" }

[MarkdownSharp.MarkdownOptions]$Script:options = New-Object -TypeName MarkdownSharp.MarkdownOptions
$Script:options.AutoHyperlink = $autoHyperlink
$Script:options.AutoNewlines = $autoNewlines
$Script:options.EmptyElementSuffix = $emptyElementSuffix
$Script:options.EncodeProblemUrlCharacters = $encodeProblemUrlCharacters
$Script:options.LinkEmails = $linkEmails
$Script:options.StrictBoldItalic = $strictBoldItalic

[MarkdownSharp.Markdown]$Script:markdown = New-Object -TypeName MarkdownSharp.Markdown -ArgumentList $Script:options
[string]$outputText = $Script:markdown.Transform($Script:inputText)
 
$outputText | Set-Content -Encoding UTF8 -Path $outputFileName -ErrorAction Stop