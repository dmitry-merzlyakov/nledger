# NLedger Setup Module that provides basic functions to manage NLedger settings

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$Script:nledgerPath = $null
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\NLWhere.psm1 -Force
Import-Module $Script:ScriptPath\NLCommon.psm1 -Force

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
function GetConfigData {
    [CmdletBinding()]
    Param()

    $private:appConfigFile = [System.IO.Path]::GetFullPath($Script:AppSettingsFile)
    setAppConfigFile $private:appConfigFile | Out-Null

    $private:extensionIndex = @{}
    $private:definitions = New-Object "System.Collections.Generic.List[NLedger.Utility.Settings.CascadeSettings.ISettingDefinition]"

    foreach($private:extension in $Global:ConfigDataExtensions.Values) {
        foreach($private:setting in $private:extension.Settings) {
            $private:def = New-Object $Script:StringSettingDefinitionType -ArgumentList @($private:setting.Name,$private:setting.Description,$private:setting.Default)
            $private:definitions.Add($private:def) | Out-Null
            $private:extensionIndex[$private:def] = $private:extension.Category
        }
    }

    $Private:NLedgerConfiguration = New-Object $Script:NLedgerConfigurationType @(,$private:definitions)
    if (!$Private:NLedgerConfiguration) { throw "Cannot create $Script:NLedgerConfigurationType" }

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
            HasDefaultValue=$private:valDefaultValue -ne $null
            
            AppSettingValue=[string]$private:valAppSettings
            HasAppSettingValue=$private:valAppSettings -ne $null
            IsAvailableForApp=(!$private:isIndexed)
            
            CommonSettingValue=[string]$private:valCommonSettings
            HasCommonSettingValue=$private:valCommonSettings -ne $null
            IsAvailableForCommon=$Private:definition.Scope -eq "user"
            
            UserSettingValue=[string]$private:valUserSettings
            HasUserSettingValue=$private:valUserSettings -ne $null
            IsAvailableForUser=$Private:definition.Scope -eq "user"
            
            EnvironmentSettingValue=[string]$private:valVarSettings
            HasEnvironmentSettingValue=$private:valVarSettings -ne $null
            IsAvailableForEnv=(!$private:isIndexed)
            
            EffectiveSettingValue=[string]$private:valEffectiveValue
            HasEffectiveSettingValue=$private:valEffectiveValue -ne $null
        }
    }

    return [PsCustomObject]@{
        CommonSettingsFile=[System.IO.Path]::GetFullPath($Private:NLedgerConfiguration.SettingsContainer.CommonSettings.FilePath)
        CommonSettingsFileExists=$($Private:NLedgerConfiguration.SettingsContainer.CommonSettings.FileExists)
        UserSettingsFile=[System.IO.Path]::GetFullPath($Private:NLedgerConfiguration.SettingsContainer.UserSettings.FilePath)
        UserSettingsFileExists=$($Private:NLedgerConfiguration.SettingsContainer.UserSettings.FileExists)
        AppSettingsFile=$private:appConfigFile
        AppSettingsFileExists=$Script:AppSettingsFileExists
        Settings=$Private:settings
    }
}

# Local helper function
function ReadConfig {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$configPath
    )

    $Private:settings = @{}
    if (Test-Path -LiteralPath $configPath -PathType Leaf) {
        [xml]$Private:config = Get-Content $configPath
        if (!($Private:config.DocumentElement.Name -eq "configuration")) { throw "Unexpected root element. Only 'configuration' is expected" }
        $Private:config.configuration.appSettings.GetElementsByTagName("add") | %{ $Private:settings[$_.key] = $_.value }
    }

    return $Private:settings
}

# Local helper function
function UpdateConfig {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$configPath,
        [Parameter(Mandatory=$True)]$addOrUpdate,
        [Parameter(Mandatory=$False)]$remove = @()
    )

    [xml]$Private:config = if (Test-Path -LiteralPath $configPath -PathType Leaf) { Get-Content $configPath } else { [xml]'<?xml version="1.0" encoding="utf-8" ?><configuration></configuration>' }
    UpdateConfigXml -xml $Private:config -addOrUpdate $addOrUpdate -remove $remove | Out-Null

    [bool]$Private:hasAppSettings = $Private:config.SelectNodes("/configuration/appSettings/add").Count
    [string]$private:folder = Split-Path -parent $configPath

    if ($Private:hasAppSettings) { 
        if (!(Test-Path -LiteralPath $private:folder -PathType Container)) { New-Item -ItemType Directory -Force -Path $private:folder }
        $Private:config.Save($configPath) | Out-Null 
    } else { 
        Remove-Item -LiteralPath $configPath -Force 
    }
}

# Local helper function
function UpdateConfigXml {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][xml]$xml,
        [Parameter(Mandatory=$False)]$addOrUpdate = @{},
        [Parameter(Mandatory=$False)]$remove = @()
    )

    if (!($xml.DocumentElement.Name -eq "configuration")) { throw "Unexpected root element. Only 'configuration' is expected" }

    $Private:appSettings = ($xml.DocumentElement.GetElementsByTagName("appSettings"))[0]
    if ($Private:appSettings -eq $null) {
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
function SetConfigValue {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$scope,
        [Parameter(Mandatory=$True)][string]$settingName,
        [Parameter(Mandatory=$True)][AllowEmptyString()][string]$settingValue
    )

    # Check scope value
    if (!(@("app","common","user") -contains $scope)) { throw "Invalid scope value: $scope" }

    $private:configData = GetConfigData
    $private:setting = $private:configData.Settings | ?{$_.Name -eq $settingName}

    # Check name
    if (!($private:setting)) { return get-fault "Cannot find setting $settingName" }
    $settingName = $private:setting.Name

    # Check value
    $private:permitted = $private:setting.PossibleValues | %{ $_ }
    if ($private:permitted) {
        [string]$private:selectedPermitted = $private:permitted | ?{ $_ -eq $settingValue }
        if (!($private:selectedPermitted)) { return get-fault "Cannot set '$settingValue' to '$settingName' setting because it is not in the range of permitted values." }
        $settingValue = $private:selectedPermitted
    }

    # Check availability
    if (($scope -eq "app") -and !($private:setting.IsAvailableForApp)) { return get-fault "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "common") -and !($private:setting.IsAvailableForCommon)) { return get-fault "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "user") -and !($private:setting.IsAvailableForUser)) { return get-fault "Cannot set '$settingName' setting because it is out of permitted scope." }

    [string]$private:fileName = if($scope -eq "app") { $Private:configData.AppSettingsFile } else { if($scope -eq "common") { $Private:configData.CommonSettingsFile } else { $Private:configData.UserSettingsFile } }
    [string]$private:folder = Split-Path -parent $private:fileName
    if (!(Test-Path -LiteralPath $private:folder -PathType Container)) { New-Item -ItemType Directory -Force -Path $private:folder }

    # Set value    
    UpdateConfig -configPath $private:fileName -addOrUpdate @{ "$settingName"="$settingValue" } | Out-Null

    return get-success "Setting '$settingName' is set to '$settingValue' (Scope '$scope')"
}

function RemoveConfigValue {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$scope,
        [Parameter(Mandatory=$True)][string]$settingName
    )

    # Check scope value
    if (!(@("app","common","user") -contains $scope)) { throw "Invalid scope value: $scope" }

    $private:configData = GetConfigData
    $private:setting = $private:configData.Settings | ?{$_.Name -eq $settingName}

    # Check name
    if (!($private:setting)) { return get-fault "Cannot find setting $settingName" }
    $settingName = $private:setting.Name

    # Check availability
    if (($scope -eq "app") -and !($private:setting.IsAvailableForApp)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "common") -and !($private:setting.IsAvailableForCommon)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }
    if (($scope -eq "user") -and !($private:setting.IsAvailableForUser)) { throw "Cannot set '$settingName' setting because it is out of permitted scope." }

    [string]$private:fileName = if($scope -eq "app") { $Private:configData.AppSettingsFile } else { if($scope -eq "common") { $Private:configData.CommonSettingsFile } else { $Private:configData.UserSettingsFile } }
    if (!(Test-Path -LiteralPath $private:fileName -PathType Leaf)) { return get-fault "Cannot remove setting $settingName because it is already removed" }

    # Set value    
    UpdateConfig -configPath $private:fileName -addOrUpdate @{} -remove @("$settingName") | Out-Null

    return get-success "Setting '$settingName' is removed (Scope '$scope')"
}

function setAppConfigFile {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appConfigFile
    )

    [NLedger.Utility.Settings.CascadeSettings.Sources.SystemConfigurationSettingsSource]::AppConfigFileName = $appConfigFile
}

function SetPlatform {
    [CmdletBinding()]
    Param(
        [Switch][bool]$preferCore = $false
    )

    [string]$private:frameworkLedger = Get-NLedgerPath
    [string]$private:coreLedger = Get-NLedgerPath -preferCore
    if (!($private:frameworkLedger) -and !($private:coreLedger)) {throw "No binaries found. Check that they are built"}
    if ($private:frameworkLedger -eq $private:coreLedger) {$private:frameworkLedger = $null}

    [string]$Private:preferableNLedger = Get-NLedgerPath -preferCore:$preferCore
    [string]$Private:preferablePath = [System.IO.Path]::GetDirectoryName($Private:preferableNLedger)

    $Script:IsCorePlatform = $private:coreLedger -eq $Private:preferableNLedger
    $Script:AppSettingsFile = if($Script:IsCorePlatform){"$Private:preferablePath\NLedger-cli.dll.config"}else{"$Private:preferablePath\NLedger-cli.exe.config"}
    $Script:AppSettingsFileExists = (Test-Path -LiteralPath $Script:AppSettingsFile -PathType Leaf)

    return get-success "Current platform is $(if($Script:IsCorePlatform){".Net Core"}else{".Net Framework"}) [App config file is $($Script:AppSettingsFile)]"
}

# Module Initialization

if (!$Script:nledgerPath) {
    Write-Verbose "NLedger path is not specified; calling NLedgerLocation..."
    $Script:nledgerPath = NLedgerLocation
}

Write-Verbose "NLedger path is $Script:nledgerPath"

Write-Verbose "Loading NLedger binaries..."
$Script:NLedgerAssembly = [System.Reflection.Assembly]::LoadFrom("$Script:nledgerPath\NLedger.dll")

Write-Verbose "Looking for NLedger configuration class"
$Script:NLedgerConfigurationType = $Script:NLedgerAssembly.GetType("NLedger.Utility.Settings.NLedgerConfiguration")
if (!$Script:NLedgerConfigurationType) { throw "Class NLedger.Utility.Settings.NLedgerConfiguration not found." }
$Script:StringSettingDefinitionType = $Script:NLedgerAssembly.GetType("NLedger.Utility.Settings.CascadeSettings.Definitions.StringSettingDefinition")
if (!$Script:StringSettingDefinitionType) { throw "Class NLedger.Utility.Settings.CascadeSettings.Definitions.StringSettingDefinition not found." }

$Script:IsCorePlatform = $false
$Script:AppSettingsFile = $null
$Script:AppSettingsFileExists = $null
$null = SetPlatform

