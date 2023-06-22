 <#
.SYNOPSIS
Helper functions for interacting with the NLedger binaries

.DESCRIPTION
Provides information about the current structure of NLedger deployment
#> 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/Contrib/NLManagement/NLCommon.psm1 -Force


# For internal usage
function Out-NLedgerBinaryInfo {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)]$info)
  
    $text = "[$($info.TfmCode)|$(if($($info.IsDebug)){"Debug"}else{"Release"}$(if($($info.IsOsxRuntme)){"(OSX)"}))]"
    if ($currentNLedgerExecutable -eq $info.NLedgerExecutable) {$text="{c:Yellow}$text{f:Normal}"}
    $text
  }
  
  # For internal usage
  function Get-NLedgerBins {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory=$True)][string]$folderName,
      [Parameter(Mandatory=$True)][bool]$isDebug
    )
  
    if (Test-Path -LiteralPath $folderName -PathType Container) {
      Write-Verbose "Get-NLedgerBins - collecting info from $folderName folder."
      foreach($targetFolder in (Get-ChildItem -LiteralPath $folderName -Directory) | Where-Object { Test-IsTfmCode $_.Name }) {
        [string]$tfmCode = $targetFolder.Name
        [string]$tfmFolder = $targetFolder.FullName
        [bool]$isOsxRuntime = $false
  
        if (Test-Path -LiteralPath "$tfmFolder/osx-x64" -PathType Container) { 
          $tfmFolder = "$tfmFolder/osx-x64"
          $isOsxRuntime = $true
        }
  
        [string]$nledgerFile = "$tfmFolder/NLedger-cli.exe"
        if (!(Test-Path -LiteralPath $nledgerFile -PathType Leaf)) {
          $nledgerFile = "$tfmFolder/NLedger-cli"
          if (!(Test-Path -LiteralPath $nledgerFile -PathType Leaf)) { continue }
        }
  
        Write-Verbose "Found executable NLedger: $nledgerFile (TFM: $tfmCode; Debug: $isDebug; OSX: $isOsxRuntime)"
        [PSCustomObject]@{
          NLedgerExecutable = [System.IO.Path]::GetFullPath($nledgerFile)
          TfmCode = $tfmCode
          IsDebug = $isDebug
          IsOsxRuntme = $isOsxRuntime
          VersionOrder = (Get-TfmCodeVersion -code $tfmCode -isDebug $isDebug)
        }
  
      }
    } else {
      Write-Verbose "Get-NLedgerBins - folder $folderName does not exist."
    }
  }
  
  <#
  .SYNOPSIS
      Provides information about available NLedger binaries
  .DESCRIPTION
      Returns a collection of objects containing information about local NLedger binaries.
      Every info object has a full path to NLedger executable file, corresponded TFM code and Debug/Release flag.
      The function looks for files in the current file structure 
      (either development structure that is a copy of the repository or public binaries distributed by means of zip archive or MSI file).
  #>
  function Get-NLedgerBinaryInfos {
    [CmdletBinding()]
    Param()
  
    [string]$devDebugBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Debug")
    [string]$devReleaseBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../Source/NLedger.CLI/bin/Release")
    [string]$publicBin = [System.IO.Path]::GetFullPath("$Script:ScriptPath/../../bin")
  
    return  (Get-NLedgerBins -folderName $devDebugBin -isDebug $true) +
            (Get-NLedgerBins -folderName $devReleaseBin -isDebug $false) +
            (Get-NLedgerBins -folderName $publicBin -isDebug $false)
  }
  
  <#
  .SYNOPSIS
      Provides information about a specific NLedger binary file
  .DESCRIPTION
      Looks for a specific NLedger instance in a collection of available local binaries
      using specified TFM code and Debug/Release flag
  #>
  function Get-NLedgerBinaryInfo {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory=$True)][string]$tfmCode,
      [Parameter(Mandatory=$True)][bool]$isDebug
    )
  
    return  Get-NLedgerBinaryInfos | Where-Object { $_.TfmCode -eq $tfmCode -and $_.IsDebug -eq $isDebug }
  }
  
  
  
  <#
  .SYNOPSIS
      Provides information about preferred ready-to-use NLedger binary file
  .DESCRIPTION
      Returns a NLedger instance that belongs to the current deployment structure.
      If the current structure contains more than one binary file, it returns the installed one.
      If NLedger is not installed (or installed NLedger belongs to another deployment structure), 
      it returns the instance with the highest TFM code.
      If the current deployment structure does not have any NLedger binary file, the function returns null.
  #>
  function Get-PreferredNLedgerBinaryInfo {
    [CmdletBinding()]
    Param()
  
    # TODO
    return  Get-NLedgerBinaryInfos | Sort-Object {$_.VersionOrder} | Select-Object -Last 1
  }
  
  <#
  .SYNOPSIS
      Provides information about available NLedger binaries in a well-formatted user-friendly way
  .DESCRIPTION
      Returns a comma separated string containing TFM code and debug/release flag for every available binary.
      If the parameter currentNLedgerExecutable is provided, the corresponded code is highlighted
  #>
  
  function Out-NLedgerBinaryInfos {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory=$False)][string]$currentNLedgerExecutable
    )
  
    return  (Get-NLedgerBinaryInfos | Sort-Object {$_.VersionOrder} | ForEach-Object { (Out-NLedgerBinaryInfo $_) }) -join ","
  }
  
  Export-ModuleMember -function * -alias *