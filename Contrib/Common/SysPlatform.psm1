<#
.SYNOPSIS
Helper functions for interacting with OS features

.DESCRIPTION
Provides platform-independent functions that represent OS-related features.

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#> 

[CmdletBinding()]
Param()

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

    !(Test-IsWindows) -and [bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
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

    !(Test-IsWindows) -and [bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
}

<#
.SYNOPSIS
    Returns a full path to ProgramFiles(x86) folder
.DESCRIPTION
    Returns either a full path to ProgramFiles(x86) folder or empty string for non-Windows OS
#>
function Get-SpecialFolderProgramFilesX86 {
[CmdletBinding()]
Param()

    if(Test-IsWindows){[System.Environment]::GetFolderPath(([System.Environment+SpecialFolder]::ProgramFilesX86))}else{""}
}

#################
# PATH management
#################

[string]$Script:PathsSeparator = $(if(Test-IsWindows){";"}else{":"})

<#
.SYNOPSIS
    Forces reading recent changes in PATH variable
.DESCRIPTION
    Restars windows explorer so that a new process gets updated PATH variable. Does nothing on non-windows platforms.
#>
function Reset-Shell {
    [CmdletBinding()]
    Param()

    if (Test-IsWindows) {
        Write-Verbose "Restarting windows explorer"
        $null = (Stop-Process -ProcessName "explorer")
    } else {
        # Do nothing on non-windows platform. It is basically a good idea to execute "source ..." command in the parent shell, but it is too complicated.
    }
}

<#
.SYNOPSIS
    Returns a collection of all paths that PATH environment variable contains
.DESCRIPTION
    Return only valid entries (empty values are omitted) as a collection of strings
#>
function Get-Paths {
    [CmdletBinding()]
    Param()

    if (Test-IsWindows) {
        # For Windows, get paths from the registry value
        (Get-Item -LiteralPath 'registry::HKEY_CURRENT_USER\Environment').GetValue('Path', '', 'DoNotExpandEnvironmentNames') -split ';' -ne ''
    } else {
        # For other OS, get paths from the env variable
        $env:PATH -split $Script:PathsSeparator | Where-Object{ $_ }
    }
}

<#
.SYNOPSIS
    Compares two paths
.DESCRIPTION
    Checks whether two paths are equal and returns True or False. Properly manages relative and do-normalized paths.
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

    return $(if(Test-IsWindows){$Private:dirA -eq $Private:dirB}else{$Private:dirA -ceq $Private:dirB})
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

    $null = (Get-Paths | Where-Object { !(Test-PathsAreEqual $_ $path) }) + $path | Update-Paths
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

    $null = Get-Paths | Where-Object { !(Test-PathsAreEqual $_ $path) } | Update-Paths
}

function Update-Paths {
    Param([Parameter(ValueFromPipeline)]$path)
  
    Begin { $paths = @() }
    Process { $paths += $path }  
    End {
        $pathsValue = $paths -join $Script:PathsSeparator
        if (((Get-Paths) -join $Script:PathsSeparator) -ne $pathsValue) {

            if (Test-IsWindows) {
                $null = Set-ItemProperty -Type ExpandString -LiteralPath 'registry::HKEY_CURRENT_USER\Environment' Path $pathsValue
                [Environment]::SetEnvironmentVariable('PATH', $pathsValue, 'User')             
            } else {
                $Private:content = Get-Content $Script:ShellConfig
                $null = ($Private:content | Out-File "$($Script:ShellConfig).old")
                $Private:content = $Private:content | Where-Object { $_ -notmatch 'export PATH=.*' }
                $null = ($paths | ForEach-Object { $Private:content += $"export PATH=`$PATH:$path" })
                $null = ($Private:content | Out-File $Script:ShellConfig )
            }

            $env:PATH = $pathsValue
        }
    }
}


#################
# NGEN management
#################

<#
.SYNOPSIS
    Checks whether current user has administrative privileges
.DESCRIPTION
    Returns True or False depending on whether current script is run as administrator
#>
function Test-AdministrativePriviledges {
    [CmdletBinding()]
    Param()

    return (Test-IsWindows) -and ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
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

    return (Test-IsWindows) -and (Test-AdministrativePriviledges) -and (Get-InstalledNGens)
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

#####################
# HARDLINK management
#####################

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

    $null = (New-Item -ItemType HardLink -Path $linkPath -Value $filePath -ErrorAction Stop)
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


Export-ModuleMember -function * -alias *