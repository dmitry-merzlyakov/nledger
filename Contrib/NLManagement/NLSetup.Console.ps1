# NLedger Setup Powershell Console
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $ScriptPath\NLCommon.psm1 -Force 
Import-Module $ScriptPath\NLSetup.psm1 -Force
Import-Module $ScriptPath\NLDoc.LiveDemo.psm1 -Force

# Specify an error handled to return exit code "1" in case of any exceptions
# See http://chrisoldwood.blogspot.com/2011/05/powershell-throwing-exceptions-exit.html for details
trap 
{ 
  write-error $_ 
  exit 1 
} 

function prompt {"NLedger Setup>"}

function help {
  [CmdletBinding()]
  Param()

  write-console ""
  write-console "{c:white}NLedger Setup Console"
  write-console "{c:white}*********************"
  Write-Console "{c:gray}This console allows to customize NLedger by managing application settings."
  Write-Console "{c:gray}Available commands:"
  Write-Console "{c:yellow}show{c:darkyellow} [setting_name -all -app -common -user -env]{c:gray} - shows all NLedger settings."
  Write-Console "{c:yellow}show-details{c:darkyellow} [setting_name -allValues -showPermitted]{c:gray} - shows detail information about all NLedger settings."
  Write-Console "{c:yellow}set-setting settingName settingValue {c:darkyellow}[-app -common -user]{c:gray} - sets a setting."
  Write-Console "{c:yellow}remove-setting settingName {c:darkyellow}[-app -common -user]{c:gray} - removes a setting."
  Write-Console "{c:yellow}set-platform {c:darkyellow}[-core]{c:gray} - selects an app binary platform (.Net Framework or .Net Core)."
  Write-Console "{c:yellow}help{c:gray} - shows this text."
  Write-Console "{c:yellow}exit{c:gray} - closes this console."
  Write-Console ""
  Write-Console "{c:gray}Note: you can get further information by executing {c:yellow}get-help [command] -Detailed{c:gray}."
  Write-Console "{c:gray}For example, {c:darkgray}get-help show{c:gray} explains every command option and provides examples."
  Write-Console ""
  Write-Console "{c:gray}Note: if you want to modify settings in app config file (e.g. NLedger-cli.exe.config),"
  Write-Console "{c:gray}you must have administrative priviledges. Run this console as an administrator."
  Write-Console ""
}

# Local helper function
function printFileInfo {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$fileDesc,
    [Parameter(Mandatory=$True)][bool]$fileExists,
    [Parameter(Mandatory=$True)][string]$fileName
  )
  if ($fileExists) {
    Write-Console "{c:gray}$fileDesc {c:darkgreen}[Exists   ]{c:gray} $fileName"
  } else {
    Write-Console "{c:gray}$fileDesc {c:darkyellow}[Not Exist]{c:gray} $fileName"
  }
}

# Local helper function
function getColoredSettingValue {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][AllowEmptyString()][string]$strValue,
    [Parameter(Mandatory=$True)][bool]$isAvailable,
    [Parameter(Mandatory=$True)][bool]$hasValue,
    [Parameter(Mandatory=$True)][string]$normalColor,
    [Parameter(Mandatory=$True)][string]$altColor
  )
  if (!$isAvailable) { return "{c:$altColor}[N/A]" }
  if (!$hasValue) { return "{c:$altColor}[not set]" }
  return "{c:$normalColor}$($strValue)"
}


<#
.SYNOPSIS
    It shows current values of NLedger settings.
.DESCRIPTION
    This function incorporates and shows all NLedger settings in a table format.
    Columns contain setting names and values for every source (default value,
    application, common, user settings and the value coming from environment variable).
    The last column contains an effective value that NLedger eventually takes.
    The list of column can be changes by means of switchers.
    Rows contain all NLedger settings. It is possible to limit this list by specifying a filter parameter.
.PARAMETER settingNameFilter
    Optional filter for the collection of NLedger parameters in Regex format.
.PARAMETER default
    Default: True. Indicates whether to show the column with default values.
.PARAMETER app
    Default: False. Indicates whether to show the column with application configuration settings ([binary folder]\NLedger-cli.exe.config).
.PARAMETER common
    Default: False. Indicates whether to show the column with common (all user) configuration settings ([ProgramData]\NLedger\common.config).
.PARAMETER user
    Default: True. Indicates whether to show the column with current user configuration settings ([AppData\Local]\NLedger\user.config).
.PARAMETER env
    Default: False. Indicates whether to show the column with configuration settings specified in environment variables.
.PARAMETER all
    Default: False. Indicates whether to show all the columns with configuration settings.
.EXAMPLE
    Show current user settings
    C:\PS> show
.EXAMPLE
    Show all possible sources for current user settings
    C:\PS> show -all
.EXAMPLE
    Show IsAtty setting only
    C:\PS> show isatty
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function show {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$settingNameFilter,
    [Switch]$default = $True,
    [Switch]$app = $False,
    [Switch]$common = $False,
    [Switch]$user = $True,
    [Switch]$env = $False,
    [Switch]$all = $False
  )

  $Private:configData = GetConfigData

  $private:columns = @()
  $private:columns += Add-TextTableColumn -property "Name"  -header "Name"
  if ($default -or $all) { $private:columns += Add-TextTableColumn -property "DefaultValue" -header "Default" }
  if ($app -or $all) { $private:columns += Add-TextTableColumn -property "AppSettingValue" -header "App" }
  if ($common -or $all) { $private:columns += Add-TextTableColumn -property "CommonSettingValue" -header "Common" }
  if ($user -or $all) { $private:columns += Add-TextTableColumn -property "UserSettingValue" -header "User" }
  if ($env -or $all) { $private:columns += Add-TextTableColumn -property "EnvironmentSettingValue"-header "Env" }
  $private:columns += Add-TextTableColumn -property "EffectiveSettingValue" -header "Effective"

  $private:settings = $Private:configData.Settings | ?{ if ($settingNameFilter) {$_.Name -match $settingNameFilter} else {$true} } | %{ [PsCustomObject]@{
        Name="{c:white}$($_.Name)"
        DefaultValue=[string](getColoredSettingValue $_.DefaultValue $true $_.HasDefaultValue "yellow" "darkyellow")
        AppSettingValue=[string](getColoredSettingValue $_.AppSettingValue $_.IsAvailableForApp $_.HasAppSettingValue "yellow" "darkyellow")
        CommonSettingValue=[string](getColoredSettingValue $_.CommonSettingValue $_.IsAvailableForCommon $_.HasCommonSettingValue "yellow" "darkyellow")
        UserSettingValue=[string](getColoredSettingValue $_.UserSettingValue $_.IsAvailableForUser $_.HasUserSettingValue "yellow" "darkyellow")
        EnvironmentSettingValue=[string](getColoredSettingValue $_.EnvironmentSettingValue $_.IsAvailableForEnv $_.HasEnvironmentSettingValue "yellow" "darkyellow")
        EffectiveSettingValue=[string](getColoredSettingValue $_.EffectiveSettingValue $true $_.HasEffectiveSettingValue "green" "darkyellow")
  } }

  Write-Console ""
  Write-Console "{c:gray}NLedger application setting table. Columns are:"
  if ($default -or $all) { Write-Console " {c:white}Default{c:gray} - Default setting value specified in code" }
  if ($app -or $all) { Write-Console " {c:white}App{c:gray} - Application setting value (specified in app.config)" }
  if ($common -or $all) { Write-Console " {c:white}Common{c:gray} - Common setting value (shared with all users)" }
  if ($user -or $all) { Write-Console " {c:white}User{c:gray} - Current user setting value" }
  if ($env -or $all) { Write-Console " {c:white}Env{c:gray} - Environment variable setting value (variables that are started with 'nledger' in names" }
  Write-Console " {c:white}Effective{c:gray} - effective setting value that NLedger receives"
  Write-Console ""

  if ($private:settings) {
    Write-TextTable -inputData $private:settings -columns $private:columns -headerBackground "blue" -separatorColor "white"
  } else {
    Write-Console "{c:yellow}No settings are selected to show. Check the validness of the setting name filter: '{c:white}$settingNameFilter{c:yellow}'"
  } 
  Write-Console ""
}

<#
.SYNOPSIS
    It shows detail information about current values of NLedger settings.
.DESCRIPTION
    This function gives detail information about every setting including its name,
    description, value type, permitted values (in case they are limited) and etc.
    In addition, it gives technical information about location of configuration files.
.PARAMETER settingNameFilter
    Optional filter for the collection of NLedger parameters in Regex format.
.PARAMETER allValues
    Default: False. Indicates whether to show the values from all sources or only whose that have a value.
.PARAMETER showPermitted
    Default: True. Indicates whether to show the permitted values for a setting (in case they are limited).
.EXAMPLE
    Show detail information about current user settings
    C:\PS> show-details
.EXAMPLE
    Show detail information excluding permitted values to shorten the output
    C:\PS> show-details -showPermitted:$false
.EXAMPLE
    Show IsAtty setting only
    C:\PS> show-details isatty
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function show-details {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$settingNameFilter,
    [Switch]$allValues = $False,
    [Switch]$showPermitted = $True
  )

  $Private:configData = GetConfigData

  Write-Console ""
  Write-Console "{c:yellow}Configuration files:"
  Write-Console ""
  printFileInfo -fileDesc "App settings   " -fileExists $Private:configData.AppSettingsFileExists -fileName $Private:configData.AppSettingsFile
  printFileInfo -fileDesc "Common settings" -fileExists $Private:configData.CommonSettingsFileExists -fileName $Private:configData.CommonSettingsFile
  printFileInfo -fileDesc "User settings  " -fileExists $Private:configData.UserSettingsFileExists -fileName $Private:configData.UserSettingsFile
  Write-Console ""

  Write-Console "{c:yellow}Configuration settings:"
  Write-Console ""
  $private:settings = $Private:configData.Settings | ?{ if ($settingNameFilter) {$_.Name -match $settingNameFilter} else {$true} } | %{ 
    Write-Console "{c:gray}Name:            {c:white}$($_.Name)"
    if ($_.Description) { Write-Console "{c:gray}Description:     {c:white}$($_.Description)" }
    if ($_.Category)    { Write-Console "{c:gray}Category:        {c:white}$($_.Category)" }
    Write-Console "{c:gray}Value Type:      {c:white}$($_.SettingType)"
    if ($showPermitted -and $_.PossibleValues) {
        Write-Console -NoNewLine "{c:gray}Permitted:       "
        Write-Columns ($_.PossibleValues | Sort-Object) -leftMargin 17 -rightBoundary 88
    }
    if ($allValues -or $_.HasDefaultValue) { Write-Console "{c:gray}Default Value:   $(getColoredSettingValue $_.DefaultValue $true $_.HasDefaultValue "yellow" "darkyellow")" }
    if ($allValues -or $_.HasAppSettingValue) { Write-Console "{c:gray}App setting:     $(getColoredSettingValue $_.AppSettingValue $_.IsAvailableForApp $_.HasAppSettingValue "yellow" "darkyellow")" }
    if ($allValues -or $_.HasCommonSettingValue) { Write-Console "{c:gray}Common setting:  $(getColoredSettingValue $_.CommonSettingValue $_.IsAvailableForCommon $_.HasCommonSettingValue "yellow" "darkyellow")" }
    if ($allValues -or $_.HasUserSettingValue) { Write-Console "{c:gray}User setting:    $(getColoredSettingValue $_.UserSettingValue $_.IsAvailableForUser $_.HasUserSettingValue "yellow" "darkyellow")" }
    if ($allValues -or $_.HasEnvironmentSettingValue) { Write-Console "{c:gray}Environment Var: $(getColoredSettingValue $_.EnvironmentSettingValue $_.IsAvailableForEnv $_.HasEnvironmentSettingValue "yellow" "darkyellow")" }
    Write-Console "{c:gray}Effective Value: $(getColoredSettingValue $_.EffectiveSettingValue $true $_.HasEffectiveSettingValue "green" "darkyellow")"
    Write-Console ""
  }

}

<#
.SYNOPSIS
    Set a setting value.
.DESCRIPTION
    This function sets a value to NLedger configuration in a specified scope.
    The default scope is 'user' (current user settings) but it is also possible 
    to set 'common' settings (for all users) and app settings (that is basically not
    recommended in order to keep binary foolder unmodified).
    The values are verified whether they are permitted and acceptable for given scope.
.PARAMETER settingName
    The name of setting (case-insensitive).
.PARAMETER settingValue
    The value (case-insensitive if the list permitted values is specified).
.PARAMETER app
    Default: False. Indicates that the current scope is Application. You must have adminidstrative priviledges.
.PARAMETER common
    Default: False. Indicates that the current scope is Common (all users).
.PARAMETER user
    Default: True. Indicates that the current scope is User (current users).
.EXAMPLE
    Set IsAtty to False for the current user
    C:\PS> set-setting isatty false
.EXAMPLE
    Set IsAtty to False for all users
    C:\PS> set-setting isatty false -common
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function set-setting {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$settingName,
    [Parameter(Mandatory=$True)][AllowEmptyString()][string]$settingValue,
    [Switch]$app = $False,
    [Switch]$common = $False,
    [Switch]$user = $True
  )

  if ($app) { $private:response = SetConfigValue -scope "app" -settingName $settingName -settingValue $settingValue }
  else {
    if ($common) { $private:response = SetConfigValue -scope "common" -settingName $settingName -settingValue $settingValue }
    else {
        if ($user) { $private:response = SetConfigValue -scope "user" -settingName $settingName -settingValue $settingValue }
        else { throw "Configuration scope is not specified. You may specify either 'app' or 'common' or 'user' flag." }
    }
  }

  PrintStatusResponse $private:response | Out-Null
}

<#
.SYNOPSIS
    Removes a setting value.
.DESCRIPTION
    This function cleans up a value from NLedger configuration in a specified scope.
    The default scope is 'user' (current user settings) but it is also possible 
    to remove a value from 'common' settings (all users) or from app settings.
.PARAMETER settingName
    The name of setting (case-insensitive).
.PARAMETER app
    Default: False. Indicates that the current scope is Application. You must have adminidstrative priviledges.
.PARAMETER common
    Default: False. Indicates that the current scope is Common (all users).
.PARAMETER user
    Default: True. Indicates that the current scope is User (current users).
.EXAMPLE
    Remove IsAtty from current user settings
    C:\PS>  remove-setting isatty
.EXAMPLE
    Remove IsAtty from all user settings
    C:\PS> remove-setting isatty -common
.NOTES
    Author: Dmitry Merzlyakov
    Date:   June 25, 2018    
#>
function remove-setting {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$settingName,
    [Switch]$app = $False,
    [Switch]$common = $False,
    [Switch]$user = $True
  )

  if ($app) { $private:response = RemoveConfigValue -scope "app" -settingName $settingName }
  else {
    if ($common) { $private:response = RemoveConfigValue -scope "common" -settingName $settingName }
    else {
        if ($user) { $private:response = RemoveConfigValue -scope "user" -settingName $settingName }
        else { throw "Configuration scope is not specified. You may specify either 'app' or 'common' or 'user' flag." }
    }
  }

  PrintStatusResponse $private:response | Out-Null
}

<#
.SYNOPSIS
    Selects an application binary platform.
.DESCRIPTION
    If the current deployment contains binary files for both platforms (.Net Framework and .Net Core),
    this function allows to select which application configuration file will be modified (for -app scope).
    Prefers .Net Framework by default. This function has no effect if there is core-only deployment.
.PARAMETER core
    .Net Core application configuration file will be modified if this switch is selected.
.EXAMPLE
    Modify .Net Core app configuration file
    C:\PS>  set-platform -core
.EXAMPLE
    Modify .Net Framework app configuration file (if available)
    C:\PS> set-platform
.NOTES
    Author: Dmitry Merzlyakov
    Date:   September 9, 2020    
#>
function set-platform {
    [CmdletBinding()]
    Param([Switch][bool]$core = $false)

    $private:response = SetPlatform -preferCore:$core
    PrintStatusResponse $private:response | Out-Null
}
  
help 

