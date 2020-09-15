<#
.SYNOPSIS
    Powershell module that manages NLedger deployment actions
.DESCRIPTION
    Installs, uninstalls and checks deployment status on any platform (Wiindows, Linux, OSX)
.NOTES
    Author: Dmitry Merzlyakov
    Date:   September 04, 2020
#>

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../NLManagement/NLWhere.psm1 -Force
Import-Module $Script:ScriptPath/SysPath.psm1 -Force
Import-Module $Script:ScriptPath/SysHardLink.psm1 -Force
Import-Module $Script:ScriptPath/SysNGen.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

<#
.SYNOPSIS
    Returns all installed NLedgers
.DESCRIPTION
    Checks PATH variable and returns a collection of objects containing NLedger deployment details for every path
#>
function Get-NLedgerInstalls {
  [CmdletBinding()]
  Param()

  [string]$Private:linkName = "ledger$(if($Script:isWindowsPlatform){".exe"})"
  [string]$Private:exeName = "NLedger-cli$(if($Script:isWindowsPlatform){".exe"})"

  foreach($path in Get-Paths) {
    [string]$Private:absLinkName = [System.IO.Path]::GetFullPath("$($path)/$Private:linkName")
    [string]$Private:absExeName = [System.IO.Path]::GetFullPath("$($path)/$Private:exeName")
    [bool]$Private:hasLink = Test-Path -LiteralPath $Private:absLinkName -PathType Leaf
    [bool]$Private:hasExe = Test-Path -LiteralPath $Private:absExeName -PathType Leaf
      
    if ($Private:hasLink -or $Private:hasExe) {
      [PsCustomObject]@{
        Path = [System.IO.Path]::GetFullPath($path)
        HasLink = $Private:hasLink
        HasExe = $Private:hasExe
        LinkPath = $Private:absLinkName
        ExePath = $Private:absExeName
      }
    }
  }
}

<#
.SYNOPSIS
    Installs NLedger
.DESCRIPTION
    Checks environment status and installs NLedger (adds to PATH, creates 'ledger' alias and runs NGen for .Net Framework binaries).
.PARAMETER path
    Path to NLedger executable binaries
#>
function Install-NLeger {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$path)

  Write-Verbose "Installing NLedger (path $path)"
  if (!(Test-Path -LiteralPath $path -PathType Container)) { throw "Folder not found: $path" }

  [string]$Private:linkName = [System.IO.Path]::GetFullPath("$($path)/ledger$(if($Script:isWindowsPlatform){".exe"})")
  [string]$Private:exeName = [System.IO.Path]::GetFullPath("$($path)/NLedger-cli$(if($Script:isWindowsPlatform){".exe"})")

  if (!(Test-Path -LiteralPath $Private:exeName -PathType Leaf)) { throw "Required file not found: $Private:exeName" }

  $Private:installedInfo = Get-NLedgerInstalls

  if($Private:installedInfo) {

    $Private:externalInstalls = $Private:installedInfo | Where-Object { !(Test-PathsAreEqual $_.Path $path) }
    if ($Private:externalInstalls) { 
      [string]$Private:externalPaths = ($Private:externalInstalls | Select-Object -ExpandProperty Path) -join ","
      throw "[Uninstall other copies of NLedger] Cannot install NLedger located by path $path because of other Ledger or NLedger contributions found in folder(s): $Private:externalPaths"
    }

    Write-Verbose "NLedger is already installed by path $path"

    if (Test-CanCallNGen) { 
      Write-Verbose "Updating NGen native image (if possible) $($Private:exeName)"
      $null = Remove-NGenImage $Private:exeName 
      $null = Add-NGenImage $Private:exeName 
    }

    if (!($Private:installedInfo.HasLink)) {
      Write-Verbose "Adding hard link $Private:linkName to $Private:exeName"
      $null = Add-HardLink -linkPath $Private:linkName -filePath $Private:exeName
    }
  } else {

    if (Test-CanCallNGen) { 
      Write-Verbose "Adding NGen native image (if possible) $($Private:exeName)"
      $null = Add-NGenImage $Private:exeName 
    }

    Write-Verbose "Adding hard link $($Private:linkName)"
    if (Test-HardLink $Private:linkName) { Remove-HardLink $Private:linkName}
    $null = Add-HardLink -linkPath $Private:linkName -filePath $Private:exeName

    Write-Verbose "Adding to PATH: $path"
    Add-Path $path
  }

  Write-Verbose "Installation finished"
}

<#
.SYNOPSIS
    Uninstalls NLedger
.DESCRIPTION
    Checks environment status and uninstalls NLedger (removes from PATH, deletes 'ledger' alias and removed NGen image for .Net Framework binaries).
.PARAMETER path
    Path to NLedger executable binaries
#>
function Uninstall-NLedger {
  [CmdletBinding()]
  Param()
  Write-Verbose "Uninstalling NLedger"

  foreach($info in Get-NLedgerInstalls) {
    Write-Verbose "Uniinstalling NLedger path $($info.Path)"

    if (!($info.HasExe)) {
      Write-Warning "Folder $($info.Path) contains 'ledger' but not 'NLedger-cli'. It might indicate that this folder is not part of NLedger contribution and should not be removed from PATH"
    } else {
      if ($info.HasLink) { 
        Write-Verbose "Removing hard link $($info.LinkPath)"
        $null = Remove-HardLink $info.LinkPath 
      }
      if (Test-CanCallNGen) { 
        Write-Verbose "Removing NGen native image (if presented) $($info.ExePath)"
        $null = Remove-NGenImage $info.ExePath 
      }
      Write-Verbose "Removing from PATH"
      Remove-Path $info.Path
    }
  }
  Write-Verbose "Uninstalling finished"
}

<#
.SYNOPSIS
    Returns a summary status for NLedger deployment
.DESCRIPTION
    Observes the current machine and collects information about NLedger deployment (available and installed binaries)
#>
function Get-NLedgerDeploymentStatus{
  [CmdletBinding()]
  Param()
  
  # Detect current biinaries
  [string]$Private:frameworkNLedger = Get-NLedgerPath -preferCore:$false
  [string]$Private:coreNLedger = Get-NLedgerPath -preferCore:$true
  if ($Private:frameworkNLedger -eq $Private:coreNLedger) { $Private:frameworkNLedger = $null }
  [bool]$Private:binaryExist = ($Private:frameworkNLedger) -or ($Private:coreNLedger)

  # Detect existing installations
  $Private:existingInstalls = Get-NLedgerInstalls
  
  $Private:currentInstall = $null
  if ($Private:frameworkNLedger) { $Private:currentInstall = $Private:existingInstalls | Where-Object { Test-PathsAreEqual $_.Path ([System.IO.Path]::GetDirectoryName($Private:frameworkNLedger)) } }
  if (($Private:coreNLedger) -and !($Private:currentInstall)) { $Private:currentInstall = $Private:existingInstalls | Where-Object { Test-PathsAreEqual $_.Path ([System.IO.Path]::GetDirectoryName($Private:coreNLedger)) } }

  $Private:externalInstalls = $Private:existingInstalls
  if ($Private:currentInstall) { $Private:externalInstalls = $Private:externalInstalls | Where-Object { $_.Path -ne $Private:currentInstall.Path } }

  [bool]$Private:isCore = $false
  if ($Private:currentInstall) { $Private:isCore = Test-PathsAreEqual $Private:currentInstall.Path ([System.IO.Path]::GetDirectoryName($Private:coreNLedger)) }

  [PsCustomObject]@{
    FrameworkNLedger = $Private:frameworkNLedger
    CoreNLedger = $Private:coreNLedger
    BinaryExist = $Private:binaryExist
    ExistingInstalls = $Private:existingInstalls
    HasExistingInstalls = !(!($Private:existingInstalls))
    CurrentInstall = $Private:currentInstall
    HasCurrentInstall = !(!($Private:currentInstall))
    IsCurrentInstallCore = $Private:isCore
    ExternalInstalls = $Private:externalInstalls
    HasExternalInstalls = !(!($Private:externalInstalls))
  }

}
