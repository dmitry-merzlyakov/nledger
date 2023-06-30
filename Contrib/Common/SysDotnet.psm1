 <#
.SYNOPSIS
Helper functions for interacting with Dotnet features

.DESCRIPTION
Provides access to .Net Framework and Net/NetCore environmental information
#> 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/SysPlatform.psm1 -Force

<#
.SYNOPSIS
    Returns a list of installed .Net Framework SDKs as a collection of version numbers
.DESCRIPTION
    This function finds information about installed .Net Framework SDKs by checking the content of the folder
      %ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\
    It returns an empty output for non-Windows machines.
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
    Returns available .Net Framework SDK targets on the current machine as a list of TFM codes
.DESCRIPTION
    Transforms a collection of installed .Net Framework SDKs into a list of TFM codes
#>
function Get-NetFrameworkSdkTargets {
  [CmdletBinding()]
  Param()

  (Get-NetFrameworkSDKs) | ForEach-Object { if($_.Build -eq -1) {"net$($_.Major)$($_.Minor)"} else {"net$($_.Major)$($_.Minor)$($_.Build)"} }
}
  
<#
.SYNOPSIS
    Returns a version of currently installed .Net Framework
.DESCRIPTION
    Returns a TFM code that corresponds to the installed .Net Framework on the current machine. 
    If .Net Framework is not installed (or if it is not available on non-Windows OS), the function returns an empty result.
#>
function Get-NetFrameworkRuntimeTarget {
  [CmdletBinding()]
  Param()

  if (Test-IsWindows) {
    [int]$release = Get-ItemPropertyValue -LiteralPath 'HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -Name Release
    if ($release -ge 533320) { return "net481" }
    if ($release -ge 528040) { return "net48" }
    if ($release -ge 461808) { return "net472" }
    if ($release -ge 461308) { return "net471" }
    if ($release -ge 460798) { return "net47" }
    if ($release -ge 394802) { return "net462" }
    if ($release -ge 394254) { return "net461" }
    if ($release -ge 393295) { return "net46" }
    if ($release -ge 379893) { return "net452" }
    if ($release -ge 378675) { return "net451" }
    if ($release -ge 378389) { return "net45" }
  }
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
    Returns a list of versions that includes all installed SDK returned by the command "dotnet --list-sdks"
#>
function Get-DotnetSdks {
  [CmdletBinding()]
  Param()

  $(dotnet --list-sdks) | ForEach-Object { [version]($_ | Select-String -Pattern "^\d+\.\d+\.\d+").Matches.Value }
}
  
<#
.SYNOPSIS
    Determines currently installed dotnet runtime versions
.DESCRIPTION
    Returns the list of versions of every installed dotnet runtime
#>
function Get-DotnetRuntimes {
  [CmdletBinding()]
  Param()

  $(dotnet --list-runtimes) | ForEach-Object { $g = ($_ | Select-String -Pattern "^(\w+\.\w+\.\w+)\s(\d+\.\d+\.\d+)").Matches.Groups; [PSCustomObject]@{Name = $g[1].Value;Version = [version]$g[2].Value} }
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

  Get-DotnetSdks | ForEach-Object { "$(if($_.Major -lt 5){"netcoreapp"}else{"net"})$($_.Major).$($_.Minor)" } | Select-Object -Unique
}
  
<#
.SYNOPSIS
    Provides targets for available dotnet runtimes
.DESCRIPTION
    Returns the list of targets (TFM codes) that correspond to the list of installed dotnet runtimes for 'Microsoft.NETCore.App'
#>
function Get-DotnetRuntimeNetCoreTargets {
  [CmdletBinding()]
  Param()

  Get-DotnetRuntimes | Where-Object {$_.Name -eq "Microsoft.NETCore.App"} | ForEach-Object {$_.Version} |
    ForEach-Object { "$(if($_.Major -lt 5){"netcoreapp"}else{"net"})$($_.Major).$($_.Minor)" } | Select-Object -Unique
}
  
<#
.SYNOPSIS
    Provides targets for all available runtimes (.Net Framework and Net Core)
.DESCRIPTION
    Returns the list of targets (TFM codes) for all available runtimes
#>
function Get-AllRuntimeTargets {
  @() + (Get-NetFrameworkRuntimeTarget) + (Get-DotnetRuntimeNetCoreTargets)
}

<#
.SYNOPSIS
    Tests whether given string is a valid TFM code
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
    Tests whether given TFM code is about .Net Framework target
.DESCRIPTION
    Supports all .Net Framework codes
#>
function Test-IsFrameworkTfmCode {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$code)

  [bool]($code -match "^net\d+$")
}

<#
.SYNOPSIS
    Extracts version and a generation flag from given Tfm code
.DESCRIPTION
    Supports all .Net Framework codes
#>
function Expand-TfmCode {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$code)

  if ($code -match "^net(\d)(\d)$") { return [PSCustomObject]@{ TfmCode=$code; IsFramework=$True; Version=[version]"$($Matches[1]).$($Matches[2])" } }
  if ($code -match "^net(\d)(\d)(\d)$") { return [PSCustomObject]@{ TfmCode=$code; IsFramework=$True; Version=[version]"$($Matches[1]).$($Matches[2]).$($Matches[3])" } }
  if ($code -match "^net(\d)\.(\d)$") { return [PSCustomObject]@{ TfmCode=$code; IsFramework=$False; Version=[version]"$($Matches[1]).$($Matches[2])" } }
  if ($code -match "^netcoreapp(\d)\.(\d)$") { return [PSCustomObject]@{ TfmCode=$code; IsFramework=$False; Version=[version]"$($Matches[1]).$($Matches[2])" } }
}

<#
.SYNOPSIS
    Tests whether given dotnet binary can be run on given dotnet runtime
.DESCRIPTION
    Takes binary and runtime TFM codes and returns True if they are compatible or False otherwise
#>
function Test-IsRuntimeCompatible {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$binaryTfmCode,
    [Parameter(Mandatory=$True)][string]$runtimeTfmCode
  )

  $bin = Expand-TfmCode $binaryTfmCode
  $runtime = Expand-TfmCode $runtimeTfmCode

  if ($bin -and $runtime -and ($bin.IsFramework -eq $runtime.IsFramework)) {
    if ($bin.IsFramework) { return $bin.Version -le $runtime.Version}
    else { return $bin.Version.Major -eq $runtime.Version.Major}
  }

  return $false
}

Export-ModuleMember -function * -alias *