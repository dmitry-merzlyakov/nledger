 <#
.SYNOPSIS
Helper functions for interacting with OS features

.DESCRIPTION
Provides platform-independent functions that represent OS-related features.
#> 

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
    Returns either a full path to ProgramFiles(x86) folder or empty string for non-Windows OS
#>
function Get-SpecialFolderProgramFilesX86 {
[CmdletBinding()]
Param()

    if(Test-IsWindows){[System.Environment]::GetFolderPath(([System.Environment+SpecialFolder]::ProgramFilesX86))}else{""}
}

Export-ModuleMember -function * -alias *