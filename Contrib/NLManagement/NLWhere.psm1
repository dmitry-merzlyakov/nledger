# NLedger-Where module locates NLedger executable binaries in the current deployment profile

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

function getFullPath { # Private wrapper for mocking
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$path)
  return [System.IO.Path]::GetFullPath($path)
}

function getLastWriteTime { # Private wrapper for mocking
  [CmdletBinding()]
  Param([Parameter(Mandatory=$True)][string]$path)
  return [System.IO.File]::GetLastWriteTimeUtc($path)
}

function checkPath {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$path,
    [Switch][bool]$isDebug = $false,
    [Switch][bool]$isRelease = $false,
    [Switch][bool]$isPackage = $false,
    [Switch][bool]$isCore = $false,
    [Switch][bool]$isAlias = $false,
    [Switch][bool]$isNotWindows = $false
  )

  [string]$private:buildPath = "$Script:ScriptPath$([System.IO.Path]::DirectorySeparatorChar)$path"
  Write-Verbose "Building path: $private:buildPath"
  [string]$private:exePath = getFullPath($private:buildPath)
  Write-Verbose "Checking path: $private:exePath"

  if (Test-Path -LiteralPath $Private:exePath -PathType Leaf) {
     Write-Verbose "Found by path: $Private:exePath"
     [PsCustomObject]@{
        Path = $Private:exePath
        Date = getLastWriteTime($Private:exePath)
        IsDebug = $isDebug
        IsRelease = $isRelease
        IsPackage = $isPackage
        IsCore = $isCore
        IsAlias = $isAlias
        IsWindows = $(!($isNotWindows))
     }
  }
}

<#
.SYNOPSIS
    Collects information about NLedger executable binaries in the current location.
.DESCRIPTION
    Returns a collection of all available NLedger binaries in current deployment structure.
    The output objects contain the absolute path to a found binary file, its date and
    several flags that help to classify the file:
    -IsAlias indicates that find binary is a hard link (ledger.exe);
    -IsCore indicates that found binary is a .Net Core application (.Net Framework otherwise);
    -IsPackage means that the file is a part of a deployment package (MSI or ZIP archive);
    -IsRelease means that the file is on development environment compiled with Release target;
    -IsDebug means Debug target respectively.
.EXAMPLE
    C:\PS> Get-NLedgerInstances
#>
function Get-NLedgerInstances {
  [CmdletBinding()]
  Param()

  checkPath -path "..\..\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" -isDebug
  checkPath -path "..\..\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" -isDebug -isCore
  checkPath -path "../../Source/NLedger.CLI/bin/Debug/netcoreapp3.1/NLedger-cli" -isDebug -isCore -isNotWindows

  checkPath -path "..\..\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" -isRelease
  checkPath -path "..\..\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" -isRelease -isCore
  checkPath -path "../../Source/NLedger.CLI/bin/Release/netcoreapp3.1/NLedger-cli" -isRelease -isCore -isNotWindows

  checkPath -path "..\NLedger-cli.exe" -isPackage
  checkPath -path "..\bin\netcoreapp3.1\NLedger-cli.exe" -isPackage -isCore

  checkPath -path "..\ledger.exe" -isAlias -isPackage
}

<#
.SYNOPSIS
    Selects a preferable NLedger executable binary from an input collection.
.DESCRIPTION
    Selects the latest NLedger executable file from an input collection provided by Get-NLedgerInstances.
    Alias is ignored, so it returns paths to physical files only.
    The switch "preferCore" allows to prefer .Net Core binaries to .Net Framework ones (default preference).
.EXAMPLE
    C:\PS> Get-NLedgerInstances | Select-NLedgerInstance -preferCore
#>
function Select-NLedgerInstance {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True,ValueFromPipeline=$True,ValueFromPipelineByPropertyName=$True)][PsCustomObject[]]$instances,
    [Switch][bool]$preferCore = $false
  )
  Begin { $Private:coll = @() }
  Process { $Private:coll += $instances }
  End { 
    $private:collected = $Private:coll | Sort-Object -Property Date -Descending | Where-Object {!$_.IsAlias}
    if ($preferCore -and $($private:collected | Where-Object {$_.IsCore})) { $private:collected = $private:collected | Where-Object {$_.IsCore}}
    if (!($preferCore) -and $($private:collected | Where-Object {!($_.IsCore)})) { $private:collected = $private:collected | Where-Object {!($_.IsCore)}}
    $private:collected | Select-Object -First 1 }
}

<#
.SYNOPSIS
    Returns an absolute path to the latest preferable NLedger executable file (string value only).
.DESCRIPTION
    The switch "preferCore" allows to select .Net Core binaries 
    (.Net Framework is prefferable by default).
.EXAMPLE
    C:\PS> Get-NLedgerPath -preferCore
#>
function Get-NLedgerPath {
  [CmdletBinding()]
  Param(
    [Switch][bool]$preferCore = $false
  )
  (Get-NLedgerInstances | Select-NLedgerInstance -preferCore:$preferCore).Path
}

Export-ModuleMember -function Get-NLedgerInstances, Select-NLedgerInstance, Get-NLedgerPath