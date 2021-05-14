[CmdletBinding()]
Param()

trap 
{ 
  write-error $_ 
  exit 1 
}

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\PyManagement.psm1 -Force
Import-Module $Script:ScriptPath\Settings.psm1 -Force

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

$appPrefix = "NLedger"
$pyEmbedPlatform = "amd64"
$pyEmbedVersion = "3.8.1"

function Discovery {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$False)][string]$path)

  if($path) {
    Write-Output "Testing Python by path: $path"

  } else {
    Write-Output "Searching local Python"

    Write-Output "Searching embedded Python"

    if($settings) {
      Write-Output "Python path in current settings:"
    }

  }

}

function Install-Embed {
  [CmdletBinding()]
  Param([Parameter(Mandatory=$False)][string]$version = $pyEmbedVersion)
  Write-Output "Embedded Python is available by path: $(Get-PyEmbed -appPrefix $appPrefix -pyVersion $version -pyPlatform $pyEmbedPlatform)"
}

function Uninstall-Embed {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$version,
    [Switch]$withPackage = $False
  )
  $Private:versions = if (!$version) {Search-PyEmbedInstalled -appPrefix $appPrefix -pyPlatform $pyEmbedPlatform}else{$version}
  $Private:versions | ForEach-Object { 
    $null = Uninstall-PyEmbed -appPrefix $appPrefix -pyPlatform $pyEmbedPlatform -pyVersion $_ -withPackage:$withPackage 
    Write-Output "Embedded Python $_ is uninstalled."
  }
}
