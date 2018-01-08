<#
.SYNOPSIS
    Converts Markdown (.md) files to HTML format
.DESCRIPTION
    This is a binding to CommonMark.Net library (https://github.com/Knagis/CommonMark.NET)
    See the referenced link to get further information about its features.
.PARAMETER inputFileName
    Relative or absolute path to a markdown file to convert.
    In case of a relative path, the tool uses the own location folder as a base.
.PARAMETER outputFileName
    Relative or absolute path to an output HTML file.
    If this parameter is omitted, the scripts creates an output file at the same folder
    and with the same name as the input file but adds .html extension.
    In case of a relative path, the tool uses the current directory as a base.
.PARAMETER addHtmlHeader
    The flag that indicates whether to add an HTML header to the output.
    Default value is True. 
.EXAMPLE
    C:\PS> .\CommonMark.ps1 -inputFileName readme.md
    Converts readme.md to readme.md.html
.NOTES
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    

[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True)][string]$inputFileName,
  [Parameter(Mandatory=$False)][string]$outputFileName = "",
  [Switch][bool]$addHtmlHeader = $True
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[System.Reflection.Assembly]::LoadFile("$Script:ScriptPath\CommonMark.dll") | Out-Null

[string]$Script:absInputFileName = if (![System.IO.Path]::IsPathRooted($inputFileName)) { Resolve-Path (Join-Path $Script:ScriptPath $inputFileName) -ErrorAction Stop } else { $inputFileName }
if (!(Test-Path $Script:absInputFileName -PathType Leaf)) { throw "Cannot find input file: $Script:absInputFileName" }
[string]$Script:inputText = (Get-Content $Script:absInputFileName -Encoding UTF8 | Out-String).Trim()

if ([String]::IsNullOrWhiteSpace($outputFileName)) { $outputFileName = "$Script:absInputFileName.html" }

[string]$output = [CommonMark.CommonMarkConverter]::Convert($Script:inputText)
if ($addHtmlHeader) {
    [string]$title = [System.IO.Path]::GetFileNameWithoutExtension($Script:absInputFileName)
    $output = "<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><title>$title</title></head><body>$output</body></html>"
}

$output | Set-Content -Encoding UTF8 -Path $outputFileName -ErrorAction Stop