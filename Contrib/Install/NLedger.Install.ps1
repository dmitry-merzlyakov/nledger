<#
.SYNOPSIS
    Installs or uninstall NLedger binaries on an individual machine.
.DESCRIPTION
    Installing NLedger is an optional operation that may improve it's usability.
    It performs three basic actions:
      1) Adds the path to NLedger binaries to PATH variable so that you can call in any folder;
      2) Creates an alias (hard link) 'ledger' to 'NLedger-cli.exe' to let you use a typical name;
      3) Calls NGen to create a very efficient native image for NLedger binaries that
         dramatically improves performance. Note: this action requires administrative priviledges.
    With the switch 'uninstall' this script removes all modifications it made.
    By default it performs all the actions but you can select only some of them.
.PARAMETER uninstall
    Switch that indicates whether to install or uninstall NLedger. 
    If this switch is not set, the script installs NLedger and uninstalls otherwise.
.PARAMETER addPath
    Switch that indicates whether to add a path to NLedger binaries to PATH variable.
    Default value is True, so the script adds the path if this switch is not set.
    If you want not to update PATH, you need to add -addPath:False.
    In case of uninstall process, this switch indicates wether to remove NLedger path.
    NOTE: this option requires administrative priviledges.
.PARAMETER addAlias
    Switch that indicates whether to create 'ledger' alias for NLedger-cli.exe.
    Short alias might simplify calling NLedger in the command line.
    Default value is True, so the script creates the alias if this switch is not set.
    If you want not to create it, you need to add -addAlias:False.
    In case of uninstall process, this switch indicates wether to remove the alias.
.PARAMETER callNGen
    Switch that indicates whether to call NGen to create a native image to speed up NLedger.
    Adding a native image significantly improves performance of .Net applications.
    Default value is True, so the script creates a native image if this switch is not set.
    If you want not to create it, you need to add -callNGen:False.
    In case of uninstall process, this switch indicates wether to remove the image.
    NOTE: this option requires administrative priviledges.
.PARAMETER nledgerPath
    Relative or absolute path to NLedger binaries (the folder that contains NLedger-cli.exe).
    In case of a relative path, it uses the script location as a base.    
.EXAMPLE
    C:\PS> .\NLedger.Install.ps1
    Installs NLedger on the system. Requires administrative priviledges.
.EXAMPLE
    C:\PS> .\NLedger.Install.ps1 -uninstall
    Uninstalls NLedger from the system. Requires administrative priviledges.
.EXAMPLE
    C:\PS> .\NLedger.Install.ps1 -callNGen:False
    Installs NLedger on the system but does not create a native image.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   December 21, 2017    
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    

[CmdletBinding()]
Param(
    [Switch][bool]$uninstall = $False,
    [Switch]$addPath = $True,
    [Switch]$addAlias = $True,
    [Switch]$callNGen = $True,
    [Parameter(Mandatory=$False,HelpMessage="Path to NLedger-cli.exe file.")][string]$nledgerPath = ".."
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
if (![System.IO.Path]::IsPathRooted($nledgerPath)) { $nledgerPath = Resolve-Path (Join-Path $Script:ScriptPath $nledgerPath) -ErrorAction Stop }
Write-Verbose "Path to NLedger binaries is $nledgerPath"
if (!(Test-Path $nledgerPath -PathType Container)) { throw "Cannot find NLedger folder by path $nledgerPath" }
[string]$nledgerExePath = "$nledgerPath\NLedger-cli.exe"
if (!(Test-Path $nledgerExePath -PathType Leaf)) { throw "Cannot find NLedger by path $nledgerExePath" }
Write-Verbose "Path to NLedger executable is $nledgerExePath"

# Tech functions

function Get-FrameworkDirectory {
    [CmdletBinding()]
    Param()
    $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory())
}

[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.Build.Utilities.v4.0')

function LatestFrameworkDirectories {
    [CmdletBinding()]
    Param()
    [string]$x86 = [Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToDotNetFramework("VersionLatest", "Bitness32")
    [string]$x64 = [Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToDotNetFramework("VersionLatest", "Bitness64")
    return $($x86,$x64)
}

function ConsoleMessage {
    [CmdletBinding()]
    Param(    
        [Parameter(Mandatory=$True)][string]$text,
        [Switch]$newLine = $False
    )
    if ($newLine) { Write-Host $text } else { Write-Host -NoNewline $text }
}

function RestartExplorer {
    [CmdletBinding()]
    Param()
    Stop-Process -ProcessName "explorer"
}

function CheckAdministrativePriviledges {
    [CmdletBinding()]
    Param()
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "This script requires administrative priviledges to perform install actions. Type 'get-help nledger-install' in PS console for more information."
    }
}

# Working with PATH variable

[string]$Script:PathKey = "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"
$Script:hklm = [Microsoft.Win32.Registry]::LocalMachine

function GetPaths {
    [CmdletBinding()]
    Param()
    $Private:regKey = $Script:hklm.OpenSubKey($Script:PathKey, $FALSE)
    [string]$Private:paths = $Private:regKey.GetValue("Path", "", [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
    Write-Verbose "Getting paths from registry: $Private:paths"
    return $Private:paths -split ";" | ?{![string]::IsNullOrWhiteSpace($_)}
}

function SetPaths {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][array]$paths
    )
    if (!$paths -or $paths.Length -eq 0) { throw "Cannot set paths because input array is empty" }
    [string]$Private:path = $paths -join ";"
    Write-Verbose "Composed paths to write to registry: $Private:path"
    # Add to registry
    $Private:regKey = $Script:hklm.OpenSubKey($Script:PathKey, $True)
    $Private:regKey.SetValue("Path", $Private:path, [Microsoft.Win32.RegistryValueKind]::ExpandString) | Out-Null
}

function AddNLedgerPath {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$nledgerPath
    )
    ConsoleMessage -newLine "Adding path to NLedger binary folder to PATH environment variable." | Out-Null
    Write-Verbose "NLedger path to add: $nledgerPath"

    $Private:currentPaths = GetPaths
    if ($Private:currentPaths -notcontains $nledgerPath) {
        $Private:currentPaths = ($Private:currentPaths | ?{$_ -notmatch "nledger" }) + $nledgerPath
        SetPaths $Private:currentPaths | Out-Null
        # Add to current session to be consistent
        #$env:path = "$Private:path;$nledgerPath"
        ConsoleMessage -newLine "Path is added.`r`n" | Out-Null
        return $True
    } else {
        ConsoleMessage -newLine "Path already exists, no changes.`r`n" | Out-Null
        return $False
    }
}

function RemoveNLedgerPath {
    [CmdletBinding()]
    Param()
    ConsoleMessage -newLine "Removing path to NLedger binary folder from PATH environment variable." | Out-Null

    $Private:currentPaths = GetPaths
    [int]$origcount = $Private:currentPaths.Length
    $Private:currentPaths = ($Private:currentPaths | ?{$_ -notmatch "nledger" })

    if ($origcount -ne $Private:currentPaths.Length) {
        SetPaths $Private:currentPaths | Out-Null
        ConsoleMessage -newLine "Path is removed.`r`n" | Out-Null
        return $True
    } else {
        ConsoleMessage -newLine "Path does not exist, no changes.`r`n" | Out-Null
        return $False
    }
}

# Working with aliases

function AddAlias {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$nledgerLinkPath,
        [Parameter(Mandatory=$True)][string]$nledgerExePath
    )
    Write-Verbose "Creating hard link '$nledgerLinkPath' to '$nledgerExePath'"

    ConsoleMessage -newLine "Creating an alias 'ledger' to NLedger-CLI.exe." | Out-Null
    if (!(Test-Path $nledgerLinkPath -PathType Leaf)) 
    { 
        Write-Verbose "Current context paths are: $env:Path"
        [string]$result = . cmd /c mklink /h $nledgerLinkPath $nledgerExePath 
        ConsoleMessage -newLine "Alias is created"  | Out-Null
    } else {
        ConsoleMessage -newLine "Alias already exists, no changes." | Out-Null
    }
    ConsoleMessage -newLine " " | Out-Null
}

function RemoveAlias {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$nledgerLinkPath
    )

    Write-Verbose "Deleting hard link '$nledgerLinkPath'"

    ConsoleMessage -newLine "Removing the alias 'ledger'." | Out-Null
    if (Test-Path $nledgerLinkPath -PathType Leaf) 
    {
        Remove-Item $nledgerLinkPath
        ConsoleMessage -newLine "Alias is removed" | Out-Null  
    } else {
        ConsoleMessage -newLine "Alias does not exist, no changes." | Out-Null
    }
    ConsoleMessage -newLine " " | Out-Null
}

# Working with NGen

function CreateImage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$nledgerExePath
    )

    ConsoleMessage -newLine "Creating native images for NLedger."
    Write-Verbose "Adding images for $nledgerExePath"

    [int]$added = 0
    [int]$notchanged = 0
    [int]$errors = 0

    $private:frameworkPaths = LatestFrameworkDirectories
    foreach($private:path in $private:frameworkPaths) {
        Write-Verbose "Working with framework $private:path"
        [string]$ngenPath = "$private:path\ngen.exe"
        
        # check whether it is already installed
        [string]$checkInstall = & $ngenPath display $nledgerExePath
        Write-Verbose "NGen display responds $checkInstall"
        [bool]$isInstalled = $checkInstall -notmatch "The specified assembly is not installed"

        if (!$isInstalled) {
            [string]$installResult = & $ngenPath install $nledgerExePath
            Write-Verbose "NGen install responds $installResult"
            if ($installResult -match "error") { $error++ } else { $added++ }
        } else { $notchanged++ }
    }

    ConsoleMessage -newLine "Done, installed $added image(s); $notchanged not changed; $errors error(s)" | Out-Null
    ConsoleMessage -newLine " " | Out-Null
}

function RemoveImage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$nledgerExePath
    )

    ConsoleMessage -newLine "Removing NLedger native images." | Out-Null
    Write-Verbose "Adding images for $nledgerExePath"

    [int]$removed = 0
    [int]$notchanged = 0
    [int]$errors = 0

    $private:frameworkPaths = LatestFrameworkDirectories
    foreach($private:path in $private:frameworkPaths) {
        Write-Verbose "Working with framework $private:path"
        [string]$ngenPath = "$private:path\ngen.exe"
        
        # check whether it is already installed
        [string]$checkInstall = & $ngenPath display $nledgerExePath
        Write-Verbose "NGen display responds $checkInstall"
        [bool]$isInstalled = $checkInstall -notmatch "The specified assembly is not installed"

        if ($isInstalled) {
            [string]$uninstallResult = & $ngenPath uninstall $nledgerExePath
            Write-Verbose "NGen uninstall responds $uninstallResult"
            if ($uninstallResult -match "error") { $error++ } else { $removed++ }
        } else { $notchanged++ }
    }

    ConsoleMessage -newLine "Done, removed $removed image(s); $notchanged already removed; $errors error(s)" | Out-Null
    ConsoleMessage -newLine " " | Out-Null
}

# Installation steps

CheckAdministrativePriviledges | Out-Null

[bool]$isPathChanged = $False

if (!$uninstall) {
    ConsoleMessage -newLine "Installing NLedger components on this computer."
    ConsoleMessage -newLine "Path to NLedger binaries: $nledgerPath"
    ConsoleMessage -newLine " "

    if ($addPath) { $isPathChanged = AddNLedgerPath $nledgerPath }
    if ($addAlias) { AddAlias "$nledgerPath\ledger.exe" $nledgerExePath }
    if ($callNGen) { CreateImage $nledgerExePath }
} else {
    ConsoleMessage -newLine "Removing references to NLedger components from this computer"
    ConsoleMessage -newLine "Path to NLedger binaries: $nledgerPath"
    ConsoleMessage -newLine " "

    if ($addPath) { $isPathChanged = RemoveNLedgerPath }
    if ($addAlias) { RemoveAlias "$nledgerPath\ledger.exe" }
    if ($callNGen) { RemoveImage $nledgerExePath }
}

if ($isPathChanged) {
    ConsoleMessage -newLine "Restaring Windows Explorer to apply changes in PATH variable."
    RestartExplorer | Out-Null
    ConsoleMessage -newLine "Done`r`n"
}

ConsoleMessage -newLine "Finished. Type 'exit' or close PS console if you see it.`r`n"

