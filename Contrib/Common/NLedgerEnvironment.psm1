<#
.SYNOPSIS
Helper functions for interacting with the NLedger binaries

.DESCRIPTION
Provides information about the current structure of NLedger deployment

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#> 

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/SysConsole.psm1
Import-Module $Script:ScriptPath/SysPlatform.psm1
Import-Module $Script:ScriptPath/SysDotnet.psm1
Import-Module $Script:ScriptPath/SysCommon.psm1

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
  Param(
    [Parameter(Mandatory)][string]$folderName, 
    [Switch][bool]$link
  )
  
  $folderName = [System.IO.Path]::GetFullPath($folderName)
  $fileName = if($link){"ledger"}else{"NLedger-cli"}
  if (Test-Path -LiteralPath $folderName -PathType Container) {
    [string]$nledgerFile = [System.IO.Path]::GetFullPath("$folderName/$fileName.exe")
    if (Test-Path -LiteralPath $nledgerFile -PathType Leaf) { $nledgerFile }
    else {
      $nledgerFile = [System.IO.Path]::GetFullPath("$tfmFolder/$fileName")
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
      [string]$nledgerLink = Get-NLedgerExecutableFile $tfmFolder -link

      Write-Verbose "Found executable NLedger: $nledgerFile (TFM: $tfmCode; Debug: $isDebug; OSX: $isOsxRuntime)"
      [PSCustomObject]@{
        NLedgerExecutable = $nledgerFile
        NLedgerLink = $nledgerLink
        Path = [System.IO.Path]::GetDirectoryName($nledgerFile)
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
  [string]$publicBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../bin")

  return  @() +
          (Find-NLedgerBinaries -folderName $devDebugBin -isDebug $true) +
          (Find-NLedgerBinaries -folderName $devReleaseBin -isDebug $false) +
          (Find-NLedgerBinaries -folderName $publicBin -isDebug $false)
}

function Find-CurrentNLedgerStandardAssembly {
  [CmdletBinding()]
  Param()

  $paths = @("$Script:ScriptPath/../../Source/NLedger/bin/Debug/netstandard2.0", "$Script:ScriptPath/../../Source/NLedger/bin/Release/netstandard2.0", "$Script:ScriptPath/../bin/netstandard2.0")
  $paths | ForEach-Object { [System.IO.Path]::GetFullPath("$_/NLedger.dll") } | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
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
      NLedgerLink = $bin.NLedgerLink
      Path = [System.IO.Path]::GetDirectoryName($bin.NLedgerExecutable)
      TfmCode = $bin.TfmCode
      IsDebug = $bin.IsDebug
      IsOsxRuntime = $bin.IsOsxRuntime
      IsInstalled = [bool]($bin.NLedgerExecutable -eq $effectiveInstalled)
      HasRuntime = ($runtimes | Where-Object { Test-IsRuntimeCompatible -binaryTfmCode $bin.TfmCode -runtimeTfmCode $_ } | Measure-Object).Count -gt 0
      ExpandedTfmCode = Expand-TfmCode $bin.TfmCode
    }
  } | Sort-Object { [string]::Format("{0}.{1}.{2:00}.{3:00}.{4:00}", [int]!$_.IsDebug, [int]!$_.ExpandedTfmCode.IsFramework, $_.ExpandedTfmCode.Version.Major, $_.ExpandedTfmCode.Version.Minor, $_.ExpandedTfmCode.Version.Build ) }

  $effectiveInstalledInfo = $infos | Where-Object { $_.IsInstalled }
  [bool]$isExternalInstall = $effectiveInstalled -and (!$effectiveInstalledInfo)

  $preferredNLedgerExecutableInfo = if($effectiveInstalledInfo) { $effectiveInstalledInfo }
  else { $infos | Where-Object { $_.HasRuntime } | Select-Object -Last 1 }

  [PSCustomObject]@{
    Binaries = $binaries
    Installed = $installed
    EffectiveInstalled = $effectiveInstalled
    EffectiveInstalledInfo = $effectiveInstalledInfo
    IsExternalInstall = $isExternalInstall
    Runtimes = $runtimes
    Infos = $infos
    PreferredNLedgerExecutableInfo = $preferredNLedgerExecutableInfo
    NLedgerStandardAssembly = (Find-CurrentNLedgerStandardAssembly)
  }

}

function Select-NLedgerDeploymentInfos {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)]$deploymentInfo,
    [Parameter(Mandatory)][string]$tfmCodes,
    [Parameter(Mandatory)][bool]$isDebug
  )

  $codes = $tfmCodes -split ";"
  $deploymentInfo.Infos | Where-Object { $codes -contains $_.TfmCode -and $_.IsDebug -eq $isDebug }
}

function Select-NLedgerDeploymentInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  $deploymentInfo.Infos | Where-Object { $_.TfmCode -eq $tfmCode -and $_.IsDebug -eq $isDebug } | Select-Object -First 1
}

function Select-NLedgerExecutableInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)]$deploymentInfo,
    [Parameter(Mandatory=$True)][string]$tfmCode,
    [Parameter(Mandatory=$True)][bool]$isDebug
  )

  $deploymentInfo.Binaries | Where-Object { $_.TfmCode -eq $tfmCode -and $_.IsDebug -eq $isDebug } | Select-Object -First 1
}

function Select-NLedgerPreferrableInfo {
  Param(
    [Parameter()][string]$tfmCode,
    [Parameter()][ValidateSet("debug","release","",IgnoreCase=$true)][string]$profile,
    [Parameter(ValueFromPipeline)]$deploymentInfo
  )

  Process {
    if (!$tfmCode -and $profile) { throw "Invalid arguments: profile can be specified only when tfmCode is specified also."}

    if ($tfmCode) { 
      $isDebug = $profile -eq "debug"
      Select-NLedgerDeploymentInfo $deploymentInfo $tfmCode $isDebug | Assert-IsNotEmpty "NLedger binaries for $(Out-TfmProfile $tfmCode $isDebug) are not found." 
    } else { 
      $deploymentInfo.PreferredNLedgerExecutableInfo | Assert-IsNotEmpty "No available binaries for installation." 
    }
  }
}

function Out-TfmProfile{
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$tfmCode,
    [Parameter(Mandatory)][bool]$isDebug
  )
  "$tfmCode ($(if($isDebug){"Debug"}else{"Release"}))"
}

function Get-LinkNameFromFile{
  [CmdletBinding()]
  Param([Parameter(Mandatory)][string]$nledgerExecutable)
  
  $path = [System.IO.Path]::GetDirectoryName($nledgerExecutable)
  $name = [System.IO.Path]::GetFileName($nledgerExecutable)

  [System.IO.Path]::GetFullPath("$path/$($name.Replace("NLedger-cli", "ledger"))")
}

function Install-NLedgerExecutable {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory)][string]$tfmCode,
    [Parameter(Mandatory)][bool]$isDebug,
    [switch]$link
  )

  $deploymentInfo = Get-NLedgerDeploymentInfo
  if($deploymentInfo.EffectiveInstalled) {throw "NLedger is already installed: ($($deploymentInfo.EffectiveInstalled))"}

  $info = Select-NLedgerExecutableInfo $deploymentInfo $tfmCode $isDebug
  if (!$info) {throw "NLedger binaries for $(Out-TfmProfile $tfmCode $isDebug) are not found."}

  Write-Verbose "Installing NLedger; adding to PATH: $($info.Path)"
  $null = Add-Path $info.Path

  if($link -and !($info.NLedgerLink)) {
    $linkName = Get-LinkNameFromFile $info.NLedgerExecutable
    Write-Verbose "Adding hard link $linkName"
    $null = Add-HardLink -linkPath $linkName -filePath $info.NLedgerExecutable
  }

  if ((Test-IsFrameworkTfmCode $info.TfmCode) -and (Test-CanCallNGen)) { 
    Write-Verbose "Adding NGen native image (if possible) $($info.NLedgerExecutable)"
    $null = Add-NGenImage $info.NLedgerExecutable
  }

  [PSCustomObject]@{
    Path = $info.Path
    NLedgerExecutable = $info.NLedgerExecutable
    NLedgerLink = $linkName
  }
}

function Uninstall-NLedgerExecutable {
  [CmdletBinding()]
  Param()

  Write-Verbose "Uninstalling NLedger"
  $deploymentInfo = Get-NLedgerDeploymentInfo

  foreach($nledgerPath in Find-InstalledNLedgerBinaries ) {
    $info = $deploymentInfo.Infos | Where-Object { $_.NLedgerExecutable -eq $nledgerPath } | Select-Object -First 1
    if ($info) {
      Write-Verbose "Uninstalling NLedger found by path $($info.NLedgerExecutable)"

      if ($info.NLedgerLink) {         
        Write-Verbose "Removing hard link $($info.NLedgerLink)"
        $null = Remove-HardLink $info.NLedgerLink 
      }

      if ((Test-IsFrameworkTfmCode $info.TfmCode) -and (Test-CanCallNGen)) { 
        Write-Verbose "Removing NGen native image (if presented) $($info.NLedgerExecutable)"
        $null = Remove-NGenImage $info.NLedgerExecutable 
      }

      Write-Verbose "Removing from PATH"
      $null = Remove-Path $info.Path

      Write-Verbose "NLedger located by this path is uninstalled"
    } else {
      Write-Warning "NLedger installed by path $($info.NLedgerExecutable) does not belong to the current deployment and cannot be uninstalled. Check PATH variable if it is not expected."
    }
  }

}

Export-ModuleMember -function * -alias *