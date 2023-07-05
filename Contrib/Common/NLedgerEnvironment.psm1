 <#
.SYNOPSIS
Helper functions for interacting with the NLedger binaries

.DESCRIPTION
Provides information about the current structure of NLedger deployment
#> 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/SysConsole.psm1 -Force
Import-Module $Script:ScriptPath/SysPlatform.psm1 -Force
Import-Module $Script:ScriptPath/SysDotnet.psm1 -Force

<#
.SYNOPSIS
    Returns a fully qualified path and name to NLedger executable file
.DESCRIPTION
    Checks whether given folder contains NLedger executable file and returns a full path and name.
    It works on any OS (Windows, Linux, OSX) so it either returns NLedger-cli.exe or NLedger-cli.
    If folder or file does not exist, it returns an empty value.
#>
function Get-NLedgerExecutableFile {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$folderName)
  
  $folderName = [System.IO.Path]::GetFullPath($folderName)
  if (Test-Path -LiteralPath $folderName -PathType Container) {
    [string]$nledgerFile = [System.IO.Path]::GetFullPath("$folderName/NLedger-cli.exe")
    if (Test-Path -LiteralPath $nledgerFile -PathType Leaf) { $nledgerFile }
    else {
      $nledgerFile = [System.IO.Path]::GetFullPath("$tfmFolder/NLedger-cli")
      if (Test-Path -LiteralPath $nledgerFile -PathType Leaf) { $nledgerFile }
    }
  }
}

<#
.SYNOPSIS
    Returns a collection of NLedger binaries found in a package folder.
.DESCRIPTION
    Looks for NLedger binaries in all sub-folders of a given folder.
    Sub-folders should match TFM names or ignored otherwise.
    Returns a collection of objects containing a full path, TFM code and other flags.
    Note: isDebug is a declarative flag indicating wether folder contains Release or Debug binaries.
#>
function Find-NLedgerBinaries {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$folderName,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  if (Test-Path -LiteralPath $folderName -PathType Container) {
    Write-Verbose "Find-NLedgerBinaries - collecting info from $folderName folder."
    foreach($targetFolder in (Get-ChildItem -LiteralPath $folderName -Directory) | Where-Object { Test-IsTfmCode $_.Name }) {
      [string]$tfmCode = $targetFolder.Name
      [string]$tfmFolder = $targetFolder.FullName
      [bool]$isOsxRuntime = $false

      if (Test-Path -LiteralPath "$tfmFolder/osx-x64" -PathType Container) { 
        $tfmFolder = "$tfmFolder/osx-x64"
        $isOsxRuntime = $true
      }

      [string]$nledgerFile = Get-NLedgerExecutableFile $tfmFolder
      if (!$nledgerFile) { continue }

      Write-Verbose "Found executable NLedger: $nledgerFile (TFM: $tfmCode; Debug: $isDebug; OSX: $isOsxRuntime)"
      [PSCustomObject]@{
        NLedgerExecutable = $nledgerFile
        TfmCode = $tfmCode
        IsDebug = $isDebug
        IsOsxRuntime = $isOsxRuntime
      }
    }
  } else {
    Write-Verbose "Find-NLedgerBinaries - folder $folderName does not exist."
  }
}
  
<#
.SYNOPSIS
    Returns a collection of all available NLedger binaries located in the current deployment structure.
.DESCRIPTION
    Looks for NLedger binaries in expected locations in the current deployment structure (where the script is located).
    It properly recognizes development structures (and looks for binaries in Source/NLedger.CLI/bin folder)
    or deployment packages (with /bin/ folder).
#>
function Find-CurrentNLedgerBinaries {
  [CmdletBinding()]
  Param()

  [string]$devDebugBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Debug")
  [string]$devReleaseBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Release")
  [string]$publicBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../bin")

  return  @() +
          (Find-NLedgerBinaries -folderName $devDebugBin -isDebug $true) +
          (Find-NLedgerBinaries -folderName $devReleaseBin -isDebug $false) +
          (Find-NLedgerBinaries -folderName $publicBin -isDebug $false)
}

<#
.SYNOPSIS
    Returns a collection of all installed NLedger binaries on the current machine.
.DESCRIPTION
    NLedger is considered to be installed if it is available by PATH variable.
    This function returns all NLedger binary files found by any path listed in PATH variable.
    Of course, only the first path is effective.
#>
function Find-InstalledNLedgerBinaries {
  [CmdletBinding()]
  Param()

  Get-Paths | ForEach-Object { Get-NLedgerExecutableFile $_ } | Where-Object { $_ }
}

function Get-NLedgerDeploymentInfo {
  [CmdletBinding()]
  Param()

  $runtimes = Get-AllRuntimeTargets

  $binaries = Find-CurrentNLedgerBinaries

  $installed = Find-InstalledNLedgerBinaries
  [string]$effectiveInstalled = $installed | Select-Object -First 1

  $infos = $binaries | ForEach-Object {
    $bin = $_
    [PSCustomObject]@{
      NLedgerExecutable = $bin.NLedgerExecutable
      TfmCode = $bin.TfmCode
      IsDebug = $bin.IsDebug
      IsOsxRuntime = $bin.IsOsxRuntime
      IsInstalled = [bool]($bin.NLedgerExecutable -eq $effectiveInstalled)
      HasRuntime = ($runtimes | Where-Object { Test-IsRuntimeCompatible -binaryTfmCode $bin.TfmCode -runtimeTfmCode $_ } | Measure-Object).Count -gt 0
      ExpandedTfmCode = Expand-TfmCode $bin.TfmCode
    }
  } | Sort-Object { [string]::Format("{0}.{1}.{2:00}.{3:00}.{4:00}", [int]!$_.IsDebug, [int]!$_.ExpandedTfmCode.IsFramework, $_.ExpandedTfmCode.Version.Major, $_.ExpandedTfmCode.Version.Minor, $_.ExpandedTfmCode.Version.Build ) }

  [bool]$isExternalInstall = $effectiveInstalled -and ($infos | Where-Object ( $_.IsInstalled) | Measure-Object).Count -eq 0

  $preferredNLedgerExecutable = if($effectiveInstalled -and !$isExternalInstall) { $effectiveInstalled }
  else { $infos | Where-Object { $_.HasRuntime } | ForEach-Object { $_.NLedgerExecutable } | Select-Object -Last 1 }

  [PSCustomObject]@{
    Binaries = $binaries
    Installed = $installed
    EffectiveInstalled = $effectiveInstalled
    IsExternalInstall = $isExternalInstall
    Runtimes = $runtimes
    Infos = $infos
    PreferredNLedgerExecutable = $preferredNLedgerExecutable
  }

}

function Out-NLedgerDeploymentInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable
  )

  $deploymentInfo.Infos | ForEach-Object {
    $info = [ordered]@{
      "TFM" = $(if($_.NLedgerExecutable -eq $selectedNLedgerExecutable){"{c:Yellow}$($_.TfmCode){f:Normal}" | Out-AnsiString}else{$_.TfmCode})
      "Profile" = "$(if($_.IsDebug){"Debug"}else{"Release"})$(if($_.IsOsxRuntime){"[OSX]"})$(if($_.IsInstalled){" [Installed]"})"
      "Runtime" = $(if($_.HasRuntime){"Available"})
      "Path" = $_.NLedgerExecutable  
    }
    [PSCustomObject]$info
  } | Format-Table
}

function Out-NLedgerDeploymentInfoCompact {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable
  )

  if (($deploymentInfo.Infos | Measure-Object).Count -eq 0) { return "None"}

  ($deploymentInfo.Infos | 
    Group-Object { $_.IsDebug } | 
    ForEach-Object { 
      [string]$releaseMarker = $(if($_.Name -eq "False"){"Release"}else{"Debug"})
      [string]$tfmList = $_.Group | ForEach-Object { 
        $code = "[$($_.TfmCode)]"
        if (!$_.HasRuntime) { "{c:DarkGray}$code{f:Normal}" }
        elseif ($_.NLedgerExecutable -eq $selectedNLedgerExecutable) { "{c:Yellow}$code{f:Normal}" }
        else { $code }
      }
      "{c:DarkGray}$releaseMarker{f:Normal} $tfmList " 
    }) -join "  " | Out-AnsiString
}

function Expand-NLedgerExecutableInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable
  )

  [bool]$isSpecified = $selectedNLedgerExecutable
  [bool]$exists = $isSpecified -and (Test-Path -LiteralPath $selectedNLedgerExecutable -PathType Leaf)
  $binInfo = ($deploymentInfo.Infos | Where-Object { $_.NLedgerExecutable -eq $selectedNLedgerExecutable} | Select-Object -First 1)

  [PSCustomObject]@{
    DeploymentInfo = $deploymentInfo
    NLedgerExecutable = $selectedNLedgerExecutable
    IsSpecified = $isSpecified
    Exists = $exists
    BinInfo = $binInfo
    IsExternal = $exists -and !($binInfo)
    HasRuntime = $binInfo.HasRuntime
    PlatformFlags = "[$($binInfo.TfmCode)][$(if($binInfo.IsDebug){"Debug"}else{"Release"})]$(if($binInfo.IsOsxRuntime){"[OSX]"})$runtimeInfo"
  }
}

function Out-NLedgerExecutableInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable
  )

  $expanded = Expand-NLedgerExecutableInfo -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable

  $info = [ordered]@{}
  if (!$expanded.IsSpecified) { $info["NLedger Executable"] = "{c:Red}Not selected{f:Normal}" | Out-AnsiString }
  else {
    $info["NLedger Executable"] = $selectedNLedgerExecutable
    if (!$expanded.Exists) { $info["Detail Info"] = "{c:Red}File does not exist{f:Normal}" | Out-AnsiString }
    else {
      if ($expanded.IsExternal) { $info["Detail Info"] = "{c:Yellow}External file (it does not belong to the current deployment structure).{f:Normal}" | Out-AnsiString }
      else {
        $runtimeInfo = "[Runtime: $(if($expanded.HasRuntime){"Available"}else{"{c:Yellow}Not available{f:Normal}" | Out-AnsiString})]"
        $info["Detail Info"] = "$($expanded.PlatformFlags)$runtimeInfo"
      }
    }
  }

  [PSCustomObject]$info | Format-List
}

function Out-NLedgerExecutableInfoCompact {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable
  )

  $expanded = Expand-NLedgerExecutableInfo -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable

  $(if (!$expanded.IsSpecified) { "{c:Red}[Not selected]{f:Normal}" }
  else {
    if (!$expanded.Exists) { "{c:Red}[Not exist]{f:Normal} $selectedNLedgerExecutable"}
    else {
      if ($expanded.IsExternal) { "{c:Yellow}[External]{f:Normal} $selectedNLedgerExecutable"}
      else {
        $runtimeInfo = $(if($expanded.HasRuntime){""}else{"{c:Yellow}[No runtime]{f:Normal}"})
        "$($expanded.PlatformFlags)$runtimeInfo $selectedNLedgerExecutable"
      }
    }
  }) | Out-AnsiString
}

function Out-NLedgerDeploymentStatus {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$selectedNLedgerExecutable,
    [Switch][bool]$compact
  )

  if ($compact) {
    [PSCustomObject]([ordered]@{
      "NLedger Executable" = Out-NLedgerExecutableInfoCompact -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable
      "Available binaries" = Out-NLedgerDeploymentInfoCompact -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable
    }) | Format-List
  } else {
    Write-Output "`n{c:White}Selected NLedger binary file{f:Normal}`n" | Out-AnsiString
    (Out-NLedgerExecutableInfo -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable | Out-String).Trim()
    Write-Output "`n{c:White}Available NLedger binary files{f:Normal}" | Out-AnsiString
    (Out-NLedgerDeploymentInfo -deploymentInfo $deploymentInfo -selectedNLedgerExecutable $selectedNLedgerExecutable)
  }

}

function Select-NLedgerExecutable {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  return  $deploymentInfo.Binaries | Where-Object { $_.TfmCode -eq $tfmCode -and $_.IsDebug -eq $isDebug } | ForEach-Object { $_.NLedgerExecutable } | Select-Object -First 1
}

function Assert-NLedgerExecutableIsValid {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$True)][AllowEmptyString()][string]$selectedNLedgerExecutable,
    [Parameter(Mandatory=$true)][scriptblock]$ScriptBlock    
  )

  if (!$selectedNLedgerExecutable) { Write-Output "{c:Red}[ERROR] NLedger binary file is not specified{f:Normal}" | Out-AnsiString }
  elseif (!(Test-Path -LiteralPath $selectedNLedgerExecutable)) { Write-Output "{c:Red}[ERROR] NLedger binary file '$selectedNLedgerExecutable' does not exist{f:Normal}" | Out-AnsiString }
  else {
    $bin = $deploymentInfo.Infos | Where-Object { $_.NLedgerExecutable -eq $selectedNLedgerExecutable} | Select-Object -First 1
    if (!$bin) { Write-Output "{c:Yellow}[WARNING] Specified NLedger binary file '$selectedNLedgerExecutable' does not belong to the current deployment structure).{f:Normal}" | Out-AnsiString }
    if (!$bin.HasRuntime) { Write-Output "{c:Yellow}[WARNING] NLedger binary file '$selectedNLedgerExecutable' refers to '$($bin.TfmCode)' runtime that is not detected on the current machine.{f:Normal}" | Out-AnsiString }

    . $ScriptBlock 
  }
}
  
  
Export-ModuleMember -function * -alias *