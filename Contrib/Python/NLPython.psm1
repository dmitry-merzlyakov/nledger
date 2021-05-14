<#
.SYNOPSIS
Helper module that manages NLedger.Extensibility.Python.settings.xml file

.DESCRIPTION
Provides functions to read, update and remove NLedger Python integration settings
#>

[string]$Script:localAppData = [Environment]::GetFolderPath("LocalApplicationData")
[string]$Script:settingsFileName = [System.IO.Path]::GetFullPath($(Join-Path $Script:localAppData "NLedger/NLedger.Extensibility.Python.settings.xml"))

function Get-PythonIntegrationSettingsFileName {
    [CmdletBinding()]
    Param()
    return $Script:settingsFileName
}

function Get-PythonIntegrationSettings {
    [CmdletBinding()]
    Param()

    if (!(Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf)) {
        Write-Verbose "File $Script:settingsFileName does not exist; no settings."
        return $null
    }

    [xml]$Private:xmlContent = Get-Content $Script:settingsFileName
    $Private:settings = [PSCustomObject]@{
        PyExecutable = $Private:xmlContent.'nledger-python-settings'.'py-executable'
        PyHome = $Private:xmlContent.'nledger-python-settings'.'py-home'
        PyPath = $Private:xmlContent.'nledger-python-settings'.'py-path'
        PyDll = $Private:xmlContent.'nledger-python-settings'.'py-dll'
        PyNetRuntimeDll = $Private:xmlContent.'nledger-python-settings'.'py-net-runtime-dll'
    }
    Write-Verbose "Read settings: $Private:settings"
    return $Private:settings
}

function Set-PythonIntegrationSettings {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$true)]$settings)

    [xml]$Private:doc = New-Object System.Xml.XmlDocument
    $null = $Private:doc.AppendChild($Private:doc.CreateXmlDeclaration("1.0","UTF-8",$null))
    $null = $Private:doc.AppendChild($Private:doc.CreateComment("NLedger Python Integration Settings"))
    $Private:root = $Private:doc.CreateElement("nledger-python-settings")

    $Private:elm = $Private:doc.CreateElement("py-executable")
    $Private:elm.InnerText = $settings.PyExecutable
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-home")
    $Private:elm.InnerText = $settings.PyHome
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-path")
    $Private:elm.InnerText = $settings.PyPath
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-dll")
    $Private:elm.InnerText = $settings.PyDll
    $null = $Private:root.AppendChild($Private:elm)

    $Private:elm = $Private:doc.CreateElement("py-net-runtime-dll")
    $Private:elm.InnerText = $settings.PyNetRuntimeDll
    $null = $Private:root.AppendChild($Private:elm)

    $null = $Private:doc.AppendChild($Private:root)

    $Private:folderName = [System.IO.Path]::GetDirectoryName($Script:settingsFileName)
    if (!(Test-Path -LiteralPath $Private:folderName -PathType Container)) {
        Write-Verbose "Creating a folder for settings file: $Private:folderName"
        $null = New-Item -ItemType Directory -Force -Path $Private:folderName
        if (!(Test-Path -LiteralPath $Private:folderName -PathType Container)) { throw "Cannot create folder '$Private:folderName'" }
    }

    $Private:doc.Save($Script:settingsFileName)
    Write-Verbose "Settings $settings are saved to file $Script:settingsFileName"

}

function Remove-PythonIntegrationSettings {
    [CmdletBinding()]
    Param()

    if (Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf) {
        Write-Verbose "Removing NLedger Python integration settings: $Script:settingsFileName"
        $null = Remove-Item -LiteralPath $Private:settingsFileName
        if (Test-Path -LiteralPath $Script:settingsFileName -PathType Leaf) { throw "Cannot delete file $Script:settingsFileName" }
    }
}

function Update-PythonEnvironmentSettings {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pyExecutable,
        [Parameter(Mandatory=$True)][string]$pyHome,
        [Parameter(Mandatory=$True)][string]$pyPath,
        [Parameter(Mandatory=$True)][string]$pyDll,
        [Parameter(Mandatory=$True)][string]$PyNetRuntimeDll
    )

    $Private:settings = Get-PythonIntegrationSettings
    if(!$Private:settings) {
        $Private:settings = [PSCustomObject]@{
            PyExecutable = $pyExecutable
            PyHome = $pyHome
            PyPath = $pyPath
            PyDll = $pyDll
            PyNetRuntimeDll = $PyNetRuntimeDll
        }    
    } else {
        $Private:settings.PyExecutable = $pyExecutable
        $Private:settings.PyHome = $pyHome
        $Private:settings.PyPath = $pyPath
        $Private:settings.PyDll = $pyDll
        $Private:settings.PyNetRuntimeDll = $PyNetRuntimeDll
    }

    $null = Set-PythonIntegrationSettings $Private:settings
    return $Private:settings
}
