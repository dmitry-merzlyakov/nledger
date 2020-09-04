<#
.SYNOPSIS
    Powershell module that helps managing hard links for files
.DESCRIPTION
    Creates, removes and checks file hard links on any platform (Wiindows, Linux, OSX)
.NOTES
    Author: Dmitry Merzlyakov
    Date:   September 04, 2020
#>

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

trap 
{ 
  write-error $_ 
  exit 1 
} 

<#
.SYNOPSIS
    Checks whether given path is a hard link to a file
.DESCRIPTION
    ReturnsTrue If the parameter is a hard link or False otherwise.
.PARAMETER linkPath
    Path to a file or link.
#>
function Test-HardLink{
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$linkPath)

    if (Test-Path -LiteralPath $linkPath -PathType Leaf) {
        return !(!(Get-ChildItem -LiteralPath $linkPath |  Where-Object {$_.LinkType -eq 'HardLink'}))
    }
}

<#
.SYNOPSIS
    Creates a hard link to a file
.DESCRIPTION
    If the target file exist and a link does not exist - creates a hard link.
.PARAMETER filePath
    Path to an existing file.
.PARAMETER linkPath
    Path to a link that needs to be created.
#>
function Add-HardLink {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$filePath,
        [Parameter(Mandatory=$True)][string]$linkPath
    )

    if (!(Test-Path -LiteralPath $filePath -PathType Leaf)) { throw "Link target file does not exist: $filePath" }
    if (Test-Path -LiteralPath $linkPath -PathType Leaf) { throw "Link name already exist: $linkPath" }

    $null = (New-Item -ItemType HardLink -Name $linkPath -Value $filePath)
}

<#
.SYNOPSIS
    Removes a hard link to a file
.DESCRIPTION
    If the parameter is a path to a hard link - removes a hard link.
.PARAMETER linkPath
    Path to a link that needs to be removed.
#>
function Remove-HardLink {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$linkPath)

    if(Test-HardLink $linkPath) {
        $null = (Remove-Item $linkPath)
    }
}
