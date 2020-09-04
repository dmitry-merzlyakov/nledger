<#
.SYNOPSIS
    Powershell module that manages shell PATH variable
.DESCRIPTION
    Adds, removes and checks paths in PATH variable on any platform (Wiindows, Linux, OSX)
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

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
[string]$Script:PathsSeparator = $(if($Script:isWindowsPlatform){";"}else{":"})

[string]$Script:ShellConfig = "$HOME/.bashrc"
if (!($Script:isWindowsPlatform) -and !(Test-Path -LiteralPath $Script:ShellConfig -PathType Leaf)) { throw "Cannot find file $Script:ShellConfig" }

<#
.SYNOPSIS
    Forces reading recent changes in PATH variable
.DESCRIPTION
    Restars windows explorer so that a new process gets updated PATH variable. Does nothing on non-windows platforms.
#>
function Reset-Shell {
    [CmdletBinding()]
    Param()

    if ($Script:isWindowsPlatform) {
        Write-Verbose "Restarting windows explorer"
        $null = (Stop-Process -ProcessName "explorer")
    } else {
        # Do nothing on non-windows platform. It is basically a good idea to execute "source ..." command in the parent shell, but it is too complicated.
    }
}

<#
.SYNOPSIS
    Compares two paths
.DESCRIPTION
    Checks whether two paths are equal and retrns True or False. Properly manages relative and do-normalized paths.
.PARAMETER pathA
    First path
.PARAMETER pathB
    Second path
#>
function Test-PathsAreEqual {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pathA,
        [Parameter(Mandatory=$True)][string]$pathB
    )

    if (!(Test-Path -LiteralPath $pathA -PathType Container) -or !(Test-Path -LiteralPath $pathB -PathType Container)) { return $false }
    [string]$Private:dirA = [System.IO.Path]::GetFullPath("$pathA/.")
    [string]$Private:dirB = [System.IO.Path]::GetFullPath("$pathB/.")

    return $(if($Script:isWindowsPlatform){$Private:dirA -eq $Private:dirB}else{$Private:dirA -ceq $Private:dirB})
}

<#
.SYNOPSIS
    Returns a collection of all paths
.DESCRIPTION
    Return only valid entries (empty values are ommitted) as a collection of strings
#>
function Get-Paths {
    [CmdletBinding()]
    Param()

    return $env:PATH -split $Script:PathsSeparator | Where-Object{$_}
}

<#
.SYNOPSIS
    Checks whether given path is in PATH variable
.DESCRIPTION
    Returns True or False depending on whether given path is in PATH variable. Properly manages relative and do-normalized paths.
#>
function Test-PathInPaths {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$path)

    return !(!(Get-Paths | Where-Object{ $(Test-PathsAreEqual $_ $path)}))
}

<#
.SYNOPSIS
    Adds path to PATH variable
.DESCRIPTION
    Adds a path to PATH variable if it is not there yet. Properly manages relative and do-normalized paths. Resets shell in case of changes in PATH.
#>
function Add-Path {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$path)

    if (!(Test-Path -LiteralPath $path -PathType Container)) { throw "Path $path does not exist" }
    if (!(Test-PathInPaths $path)) {

        if ($Script:isWindowsPlatform) {
            [System.Environment]::SetEnvironmentVariable("PATH","$($env:PATH)$($Script:PathsSeparator)$path",[System.EnvironmentVariableTarget]::User)
        } else {
            [string]$Private:exportString = "export PATH=`$PATH:$path"
            [string]$Private:escapedExportString = [System.Text.RegularExpressions.Regex]::Escape($Private:exportString)
            if(!(Select-String -LiteralPath $Script:ShellConfig -Pattern $Private:escapedExportString)) { # Check whether the file already contains the string
                $Private:content = Get-Content $Script:ShellConfig
                $null = ($Private:content | Out-File "$($Script:ShellConfig).old")
                $null = Add-Content $Script:ShellConfig $Private:exportString
            }
        }

        $env:PATH = "$($env:PATH)$($Script:PathsSeparator)$path"
        $null = Reset-Shell
    }
}

<#
.SYNOPSIS
    Removes path from PATH variable
.DESCRIPTION
    Removes a path from PATH variable if it is currently there. Properly manages relative and do-normalized paths. Resets shell in case of changes in PATH.
#>
function Remove-Path {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$path)

    if (!(Test-Path -LiteralPath $path -PathType Container)) { throw "Path $path does not exist" }
    if (Test-PathInPaths $path) {

        if ($Script:isWindowsPlatform) {
            [string]$Private:paths = ( Get-Paths | Where-Object { !(Test-PathsAreEqual $_ $path) } ) -join $($Script:PathsSeparator)
            [System.Environment]::SetEnvironmentVariable("PATH",$Private:paths,[System.EnvironmentVariableTarget]::User)
        } else {
            [string]$Private:exportString = "export PATH=`$PATH:$path"
            [string]$Private:escapedExportString = [System.Text.RegularExpressions.Regex]::Escape($Private:exportString)
            if(Select-String -LiteralPath $Script:ShellConfig -Pattern $Private:escapedExportString) { # Check whether the string was already removed
                $Private:content = Get-Content $Script:ShellConfig
                $null = ($Private:content | Out-File "$($Script:ShellConfig).old")
                $Private:content = $Private:content | Where-Object { $_ -ne $Private:exportString }
                $null = ($Private:content | Out-File $Script:ShellConfig )
            }
        }

        $env:PATH = ( Get-Paths | Where-Object { !(Test-PathsAreEqual $_ $path) } ) -join $($Script:PathsSeparator)
        $null = Reset-Shell
    }
}
