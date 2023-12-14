<#
.SYNOPSIS
NLedger configuration settings management

.DESCRIPTION
Provides helper functions that allow to read and update NLedger config settings

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#> 

[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/SysCommon.psm1
Import-Module $Script:ScriptPath/NLedgerEnvironment.psm1

$Global:ConfigDataExtensions = @{}


<#
.SYNOPSIS
    Returns configuration container.
.DESCRIPTION
    This function calls NLedger.CLI binaries to build a configuration container.
    The container contains complete descriptive information about all settings
    and current values.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function Get-NLedgerConfigData {
    [CmdletBinding()]
    Param([Parameter()][string]$nledgerPath)

    if (!(Test-NLedgerAssemblyLoaded)) { throw "NLedger binaries not loaded. GetConfigData is not available."}
    $appConfigName = Set-NLedgerAppConfigFile $(if($nledgerPath){$nledgerPath}else{(Get-NLedgerDeploymentInfo).PreferredNLedgerExecutableInfo.NLedgerExecutable})

    $private:extensionIndex = @{}
    $private:definitions = New-Object "System.Collections.Generic.List[NLedger.Utility.Settings.CascadeSettings.ISettingDefinition]"

    foreach($private:extension in $Global:ConfigDataExtensions.Values) {
        foreach($private:setting in $private:extension.Settings) {
            $private:def = New-Object $Script:StringSettingDefinitionType -ArgumentList @($private:setting.Name,$private:setting.Description,$private:setting.Default)
            $private:definitions.Add($private:def) | Out-Null
            $private:extensionIndex[$private:def] = $private:extension.Category
        }
    }

    $Private:NLedgerConfiguration = New-Object $Script:NLedgerConfigurationType @(,$private:definitions) | Assert-IsNotEmpty "Cannot create $Script:NLedgerConfigurationType"

    $Private:settings = @()
    foreach($Private:definition in $Private:NLedgerConfiguration.Definitions) {
        $private:valDefaultValue = $Private:definition.DefaultValue
        $private:valAppSettings = $Private:NLedgerConfiguration.SettingsContainer.AppSettings.GetValue($Private:definition.Name)
        $private:valCommonSettings = $Private:NLedgerConfiguration.SettingsContainer.CommonSettings.GetValue($Private:definition.Name)
        $private:valUserSettings = $Private:NLedgerConfiguration.SettingsContainer.UserSettings.GetValue($Private:definition.Name)
        $private:valVarSettings = $Private:NLedgerConfiguration.SettingsContainer.VarSettings.GetValue($Private:definition.Name)
        $private:valEffectiveValue = $Private:NLedgerConfiguration.SettingsContainer.GetEffectiveValue($Private:definition.Name)

        $private:isIndexed = $private:extensionIndex.ContainsKey($Private:definition)
        $private:category = if($private:isIndexed){$private:extensionIndex[$Private:definition]}else{".Net Ledger Application Settings"}

        $Private:settings += [PsCustomObject]@{
            Name=$Private:definition.Name
            Category=$private:category
            Description=$Private:definition.Description
            SettingType=$Private:definition.SettingType.Name
            PossibleValues=$Private:definition.GetValues()

            DefaultValue=[string]$private:valDefaultValue
            HasDefaultValue=[bool]$private:valDefaultValue
            
            AppSettingValue=[string]$private:valAppSettings
            HasAppSettingValue=[bool]$private:valAppSettings
            IsAvailableForApp=(!$private:isIndexed)
            
            CommonSettingValue=[string]$private:valCommonSettings
            HasCommonSettingValue=[bool]$private:valCommonSettings
            IsAvailableForCommon=$Private:definition.Scope -eq "user"
            
            UserSettingValue=[string]$private:valUserSettings
            HasUserSettingValue=[bool]$private:valUserSettings
            IsAvailableForUser=$Private:definition.Scope -eq "user"
            
            EnvironmentSettingValue=[string]$private:valVarSettings
            HasEnvironmentSettingValue=[bool]$private:valVarSettings
            IsAvailableForEnv=(!$private:isIndexed)
            
            EffectiveSettingValue=[string]$private:valEffectiveValue
            HasEffectiveSettingValue=[bool]$private:valEffectiveValue
        }
    }

    return [PsCustomObject]@{
        CommonSettingsFile=[System.IO.Path]::GetFullPath($Private:NLedgerConfiguration.SettingsContainer.CommonSettings.FilePath)
        CommonSettingsFileExists=$($Private:NLedgerConfiguration.SettingsContainer.CommonSettings.FileExists)
        UserSettingsFile=[System.IO.Path]::GetFullPath($Private:NLedgerConfiguration.SettingsContainer.UserSettings.FilePath)
        UserSettingsFileExists=$($Private:NLedgerConfiguration.SettingsContainer.UserSettings.FileExists)
        AppSettingsFile=$appConfigName
        AppSettingsFileExists=[bool]$appConfigName
        Settings=$Private:settings
    }
}

function Update-NLedgerConfigXml {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][xml]$xml,
        [Parameter(Mandatory=$False)][hashtable]$addOrUpdate = @{},
        [Parameter(Mandatory=$False)]$remove = @()
    )

    if (!($xml.DocumentElement.Name -eq "configuration")) { throw "Unexpected root element. Only 'configuration' is expected" }

    $Private:appSettings = ($xml.DocumentElement.GetElementsByTagName("appSettings"))[0]
    if (!$Private:appSettings) {
        $Private:appSettings  = $xml.CreateElement("appSettings")
        $xml.DocumentElement.AppendChild($Private:appSettings) | Out-Null
    }
    
    foreach($Private:name in $addOrUpdate.Keys) {
        $Private:appSetting = $Private:appSettings.GetElementsByTagName("add") | ?{$_.key -eq $Private:name}
        if (!$Private:appSetting) {
            $Private:appSetting = $xml.CreateElement("add")
            $Private:appSettings.AppendChild($Private:appSetting) | Out-Null
        }

        $Private:appSetting.SetAttribute("key", $Private:name) | Out-Null
        $Private:appSetting.SetAttribute("value", $addOrUpdate[$Private:name]) | Out-Null
    }

    foreach($Private:name in $remove) {
        $Private:appSetting = $Private:appSettings.GetElementsByTagName("add") | ?{$_.key -eq $Private:name}
        if ($Private:appSetting) { $Private:appSettings.RemoveChild($Private:appSetting) | Out-Null }
    }

    return $xml
}

function Update-NLedgerConfigFile {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$configPath,
        [Parameter(Mandatory=$True)][hashtable]$addOrUpdate,
        [Parameter(Mandatory=$False)]$remove = @()
    )

    [xml]$Private:config = if (Test-Path -LiteralPath $configPath -PathType Leaf) { Get-Content $configPath } else { [xml]'<?xml version="1.0" encoding="utf-8" ?><configuration></configuration>' }
    $null = Update-NLedgerConfigXml -xml $Private:config -addOrUpdate $addOrUpdate -remove $remove

    [bool]$Private:hasAppSettings = $Private:config.SelectNodes("/configuration/appSettings/add").Count
    [string]$private:folder = Split-Path -parent $configPath

    if ($Private:hasAppSettings) { 
        if (!(Test-Path -LiteralPath $private:folder -PathType Container)) { $null = New-Item -ItemType Directory -Force -Path $private:folder }
        $null = $Private:config.Save($configPath)
    } else { 
        $null = Remove-Item -LiteralPath $configPath -Force 
    }
}

<#
.SYNOPSIS
    Updates a setting value in a configuration file.
.DESCRIPTION
    This function physically updates a configuration file that conforms the specified scope.
.PARAMETER scope
    The configuration scope (app,common,user).
.PARAMETER settingName
    Setting name (case-insensitive).
.PARAMETER settingValue
    Setting value. Will be validated before an update.
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function Set-NLedgerConfigValue {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][ValidateSet("app","common","user",IgnoreCase=$true)][string]$scope,
        [Parameter(Mandatory)][string]$settingName,
        [Parameter(Mandatory)][AllowEmptyString()][string]$settingValue
    )

    $private:configData = Get-NLedgerConfigData
    $private:setting = $private:configData.Settings | Where-Object{$_.Name -eq $settingName} | Assert-IsNotEmpty "Cannot find setting $settingName"
    $settingName = $private:setting.Name

    # Check value
    $private:permitted = $private:setting.PossibleValues | ForEach-Object{ $_ }
    if ($private:permitted) {
        [string]$private:selectedPermitted = $private:permitted | Where-Object{ $_ -eq $settingValue } | Assert-IsNotEmpty "Cannot set '$settingValue' to '$settingName' setting because it is not in the range of permitted values."
        $settingValue = $private:selectedPermitted
    }

    # Check availability
    if (($scope -eq "app") -and !($private:setting.IsAvailableForApp)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "common") -and !($private:setting.IsAvailableForCommon)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "user") -and !($private:setting.IsAvailableForUser)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }

    [string]$private:fileName = if($scope -eq "app") { $Private:configData.AppSettingsFile } else { if($scope -eq "common") { $Private:configData.CommonSettingsFile } else { $Private:configData.UserSettingsFile } }
    [string]$private:folder = Split-Path -parent $private:fileName
    if (!(Test-Path -LiteralPath $private:folder -PathType Container)) { $null = New-Item -ItemType Directory -Force -Path $private:folder }

    # Set value    
    $null = Update-NLedgerConfigFile -configPath $private:fileName -addOrUpdate @{ "$settingName"="$settingValue" }

    [PSCustomObject]@{
        Status = "Updated"
        Setting = $settingName
        Value = $settingValue
        Scope = $scope
    }
}

function Remove-NLedgerConfigValue {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][ValidateSet("app","common","user",IgnoreCase=$true)][string]$scope,
        [Parameter(Mandatory)][string]$settingName
    )

    $private:configData = Get-NLedgerConfigData
    $private:setting = $private:configData.Settings | Where-Object{$_.Name -eq $settingName} | Assert-IsNotEmpty "Cannot find setting $settingName"
    $settingName = $private:setting.Name

    # Check availability
    if (($scope -eq "app") -and !($private:setting.IsAvailableForApp)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "common") -and !($private:setting.IsAvailableForCommon)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "user") -and !($private:setting.IsAvailableForUser)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }

    [string]$private:fileName = if($scope -eq "app") { $Private:configData.AppSettingsFile } else { if($scope -eq "common") { $Private:configData.CommonSettingsFile } else { $Private:configData.UserSettingsFile } }
    if (!(Test-Path -LiteralPath $private:fileName -PathType Leaf)) { throw "Cannot remove setting $settingName because it is already removed" }

    # Set value    
    $null = Update-NLedgerConfigFile -configPath $private:fileName -addOrUpdate @{} -remove @("$settingName")

    [PSCustomObject]@{
        Status = "Removed"
        Setting = $settingName
        Scope = $scope
    }
}


function Set-NLedgerAppConfigFile {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$nledgerPath)

    if (!(Test-Path -LiteralPath $nledgerPath -PathType Leaf)) { return }

    $path = [System.IO.Path]::GetDirectoryName($nledgerPath)
    $appConfig = [System.IO.Path]::GetFullPath("$path/NLedger-cli.dll.config")
    if (!(Test-Path -LiteralPath $appConfig -PathType Leaf)) { $appConfig = [System.IO.Path]::GetFullPath("$path/NLedger-cli.exe.config") }
    if (!(Test-Path -LiteralPath $appConfig -PathType Leaf)) { return }

    Write-Verbose "Use app.config file: $appConfig"
    [NLedger.Utility.Settings.CascadeSettings.Sources.SystemConfigurationSettingsSource]::AppConfigFileName = $appConfig
    $appConfig
}

function Get-NLedgerAssemblyType {
    [CmdletBinding()]
    Param([Parameter()][string]$typeName)
  
    ($Script:nledgerAssembly | Assert-IsNotEmpty "NLedger assembly is not loaded").GetType($typeName) | Assert-IsNotEmpty "Class $typeName not found."
}

function Test-NLedgerAssemblyLoaded {
    [CmdletBinding()]
    Param()

    [bool]$Script:nledgerAssembly
}

# Initialization

$Script:nledgerAssemblyName = (Get-NLedgerDeploymentInfo).NLedgerStandardAssembly
if ($Script:nledgerAssemblyName) { $Script:nledgerAssembly = [System.Reflection.Assembly]::LoadFrom($Script:nledgerAssemblyName) }

if (!(Test-NLedgerAssemblyLoaded)) {
    Write-Verbose "Cannot load NLedger assembly; config management functionality is limited"
    return
}

Write-Verbose "Looking for NLedger configuration class"
$Script:NLedgerConfigurationType = Get-NLedgerAssemblyType "NLedger.Utility.Settings.NLedgerConfiguration"
$Script:StringSettingDefinitionType = Get-NLedgerAssemblyType "NLedger.Utility.Settings.CascadeSettings.Definitions.StringSettingDefinition"