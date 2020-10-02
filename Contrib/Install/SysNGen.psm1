<#
.SYNOPSIS
    Powershell module that helps managing .Net Framework native images
.DESCRIPTION
    Creates, removes and checks status for .Net Framework native images by calling NGen
.NOTES
    Author: Dmitry Merzlyakov
    Date:   September 04, 2020
#>

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

<#
.SYNOPSIS
    Checks whether current user has administrative privileges
.DESCRIPTION
    Returns True or False depending on whether current script is run as administrator
#>
function Test-AdministrativePriviledges {
    [CmdletBinding()]
    Param()

    return $Script:isWindowsPlatform -and ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

<#
.SYNOPSIS
    Returns paths to all installed NGen on current environment
.DESCRIPTION
    Checks the latest installed .Net Framework and returns absolute paths to NGen executable files (x86 and x64 versions).
#>
function Get-InstalledNGens {
    [CmdletBinding()]
    Param()

    [string]$Private:ngen64 = "$((Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").InstallPath)ngen.exe"
    [string]$Private:ngen32 = "$((Get-ItemProperty "HKLM:SOFTWARE\WOW6432Node\Microsoft\NET Framework Setup\NDP\v4\Full").InstallPath)ngen.exe"

    if (Test-Path -LiteralPath $Private:ngen64 -PathType Leaf) { $Private:ngen64 }
    if (Test-Path -LiteralPath $Private:ngen32 -PathType Leaf) { $Private:ngen32 }
}

<#
.SYNOPSIS
    Checks whether NGen functions can be called
.DESCRIPTION
    The conditions are: windows platform, administrative privileges, NGen is available
#>
function Test-CanCallNGen {
    [CmdletBinding()]
    Param()

    return $Script:isWindowsPlatform -and (Test-AdministrativePriviledges) -and (Get-InstalledNGens)
}

<#
.SYNOPSIS
    Checks whether given .Net binary file has a native image
.DESCRIPTION
    Executes NGen display command to verify the status for given .Net binary file    
.PARAMETER ngenPath
    Absoulte path to NGen executable file.
.PARAMETER path
    Absolute path to a .Net Framework binary file.
#>
function Test-NGenImage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$ngenPath,
        [Parameter(Mandatory=$True)][string]$path
    )
    
    if(!(Test-Path -LiteralPath $ngenPath -PathType Leaf)) { throw "Cannot find $ngenPath" }
    [string]$Private:displayResult = & $Private:ngenPath display $path
    Write-Verbose "NGen display responds $Private:displayResult"
    return ($Private:displayResult -notmatch "The specified assembly is not installed" )
}

<#
.SYNOPSIS
    Creates a native image for a .Net binary file
.DESCRIPTION
    Executes NGen install command to create a native image for a .Net binary file if it has not been installed yet
.PARAMETER path
    Absolute path to a .Net Framework binary file.
#>
function Add-NGenImage {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$path)

    if(!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Cannot find the assembly $path" }

    foreach($Private:ngenPath in (Get-InstalledNGens | Where-Object {!(Test-NGenImage $_ $path)}) ) {
        [string]$Private:installResult = & $Private:ngenPath install $path
        Write-Verbose "NGen install responds $Private:installResult"
        if ($installResult -match "error") { Write-Verbose "Error adding assembly $path" }
    }
}

<#
.SYNOPSIS
    Removes a native image of a .Net binary file from NGen repository
.DESCRIPTION
    Executes NGen uninstall command to remove a native image if it has not been removed yet
.PARAMETER path
    Absolute path to a .Net Framework binary file.
#>
function Remove-NGenImage {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$path)

    if(!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Cannot find the assembly $path" }

    foreach($Private:ngenPath in (Get-InstalledNGens | Where-Object {Test-NGenImage $_ $path}) ) {
        [string]$Private:uninstallResult = & $ngenPath uninstall $path
        Write-Verbose "NGen uninstall responds $Private:uninstallResult"
        if ($uninstallResult -match "error") { Write-Verbose "Error removing assembly $path" }
    }
}
