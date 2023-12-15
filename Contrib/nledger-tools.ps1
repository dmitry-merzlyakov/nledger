<#
.SYNOPSIS
NLedger Control Console

.DESCRIPTION
The NLedger Control Console provides a common way to manage your NLedger environment.
It provides commands to install/uninstall NLedger, manage configuration settings,
run tests, manage Python integration.

It can be run as a script file from the command line, allowing you to execute individual commands.
or behave like a console shell within a Powershell session.

Use "help" to get a list of available commands, or "get-help [command]" for detailed information.

.PARAMETER command
Command to execute. Can be omitted if you are using this console in a Powershell session.

.PARAMETER commandArguments
Command arguments (if any).

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#>    
[CmdletBinding()]
Param(
  [Parameter(Position=0)][ValidateSet("status","install","uninstall","test","set-config","remove-config","show-config","python-discover","python-status","python-connect","python-disconnect","python-enable","python-disable","python-install","python-uninstall","python-install-wheel","python-uninstall-wheel","python-install-pythonnet","python-uninstall-pythonnet","python-install-ledger","python-uninstall-ledger","python-test-ledger","live-demo","help",IgnoreCase=$true)][string]$command,
  [Parameter(Position=1, ValueFromRemainingArguments)][string[]]$commandArguments
)

function Assert-FileExists {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory)][string]$fileName,
      [Parameter()][string]$message
    )

    $fileName = [System.IO.Path]::GetFullPath($fileName)
    if (!(Test-Path -LiteralPath $fileName -PathType Leaf)) { throw "File '$fileName' does not exist. $message" }
    return $fileName
}

function Import-ModuleIfAvailable {
    [CmdletBinding()]
    Param([Parameter(Mandatory)][string]$fileName)

    $fileName = [System.IO.Path]::GetFullPath($fileName)
    [bool] $(if (Test-Path -LiteralPath $fileName -PathType Leaf) { Import-Module $fileName })
}

[string]$Script:CommandPath = $MyInvocation.MyCommand.Path
[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module (Assert-FileExists "$Script:ScriptPath/Common/SysCommon.psm1" -message "Check that code base is in valid state")
Import-Module (Assert-FileExists "$Script:ScriptPath/Common/NLedgerEnvironment.psm1" -message "Check that code base is in valid state")
Import-Module (Assert-FileExists "$Script:ScriptPath/Common/NLedgerConfig.psm1" -message "Check that code base is in valid state")
$Script:isPythonAvailable = Import-ModuleIfAvailable "$Script:ScriptPath/Python/NLedgerPyEnvironment.psm1"
$Script:isLiveDemoAvailable = Import-ModuleIfAvailable "$Script:ScriptPath/LiveDemo/NLDoc.LiveDemo.psm1"
$Script:isTestFrameworkAvailable = Import-ModuleIfAvailable "$Script:ScriptPath/test/NLTest.psm1"

$Global:AssertionCommandCompletedFailure = { Write-Output "{c:Red}[ERROR]{f:Normal} $($_.exception.message)`n" | Out-AnsiString }

# Basic Console commands

<#
.SYNOPSIS
Shows information about the current NLedger deployment

.DESCRIPTION
Provides information on the following aspects:
- Path to the currently used NLedger executable (if it exists)
- Indicates whether it is installed (in other words, added to the PATH variable) or 
  simply preferred and will be used in the current deployment without being explicitly installed. 
  If the NLedger added to PATH does not belong to the current deployment (for example, 
  it is in a different folder), it is listed as External.
- Information about all available NLedger binaries in the current deployment (for example, 
  for net6 or net8 platforms).
- Optional, information about the .Net runtimes available on the current machine.

.PARAMETER details
By default, the status is displayed in a compact form without detailed information.
This switch can expand information about the available NLedger binaries and also show the currently installed .Net runtimes.

.EXAMPLE
PS> status

Shows the NLedger deployment status in a compact form.

.EXAMPLE
PS> status -details

Shows the status of NLedger deployment, including detailed information about available NLedger binaries and .Net runtimes.

#>
function Show-NLedgerStatus {
  [CmdletBinding()]
  [Alias("status")]
  Param([switch]$details)

  Assert-CommandCompleted {
    $deploymentInfo = Get-NLedgerDeploymentInfo

    $info = [ordered]@{}
    if ($deploymentInfo.Installed -and $deploymentInfo.IsExternalInstall) { $info["NLedger (Installed, External)"] = $deploymentInfo.Installed }
    if ($deploymentInfo.Installed -and !$deploymentInfo.IsExternalInstall) { $info["NLedger (Installed)"] = $deploymentInfo.Installed }
    if (!$deploymentInfo.Installed -and $deploymentInfo.PreferredNLedgerExecutableInfo) { $info["NLedger (Preferred, Not Installed)"] = $deploymentInfo.PreferredNLedgerExecutableInfo.NLedgerExecutable }
    if (!$deploymentInfo.Installed -and !$deploymentInfo.PreferredNLedgerExecutableInfo) { $info["NLedger"] = "Not found" }

    if (!$details) {
      $info["Available"] = ($deploymentInfo.Infos | ForEach-Object {
        $item = ""
        if ($_.IsInstalled) { $item += "{c:Yellow}" | Out-AnsiString }
        if (!$_.HasRuntime) { $item += "{c:DarkGray}" | Out-AnsiString }
        $item += "$(Out-TfmProfile $_.TfmCode $_.IsDebug) $(if($_.IsOsxRuntime){"[OSX]"})".Trim()
        $item += "{f:Normal}" | Out-AnsiString
        $item
      }) -join ", "
      if (!$deploymentInfo.Infos) { $info["Available"] = "Not found" }
    }

    $output = @( [PSCustomObject]$info )

    if ($details) {
      $output += $deploymentInfo.Infos | ForEach-Object {
          $info = [ordered]@{}
          $info["Executable"] = $_.NLedgerExecutable
          $info["Link"] = $_.NLedgerLink | Out-MessageWhenEmpty
          $info["TFM"] = "$(Out-TfmProfile $_.TfmCode $_.IsDebug) $(if($_.IsOsxRuntime){"[OSX]"})"
          if ($_.IsInstalled) { $info["Installed"] = "Yes" }
          $info["Runtime"] = if($_.HasRuntime){"Available"} else {"Not available"}
          [PSCustomObject]$info
      }

      $output += [PSCustomObject][ordered]@{
        ".Net Runtime(s)" = $deploymentInfo.Runtimes -join ", "
      }
    }

    $output | Format-List
  }
}

<#
.SYNOPSIS
Installs the current version of NLedger on the machine

.DESCRIPTION
Installing NLedger essentially involves adding the path to the NLedger binaries to your PATH variable so that you can later use it as a regular command line utility.
Specifically, the installation includes three steps:
- add the path to the selected NLedger to the PATH variable
- optionally create a hard link named 'ledger' in the same folder (use the 'link' switch for it)
- For .Net Frameworks, it also calls NGen to optimize the selected binary.

By default, NLedger is installed for the latest runtime built into the Release profile.
You can select a different executable by specifying the TFM code (for example, 'net6' or 'net8') and, if necessary, the profile (for example, 'Debug').
A list of available binaries and corresponding TFM codes can be viewed with the command status -detail.

If NLedger is already installed, the command will try to remove the previous installation before installing it again.
If the command detects an external installation, the installation will be stopped. You should fix your PATH variable manually.
If you select the same targetm platform (TFM code) as the one currently installed, the command will also stopped. You may perform the 'uninstall' manually.

.PARAMETER tfmCode
Optionally specifies the target runtime (TFM code) for NLedger binaries. By default, it looks for the latest available runtime.

.PARAMETER profile
Specifies the target profile (Release or Debug) for NLedger binaries. By default it uses Release. If this parameter is specified, the 'tfmCode' parameter must also be specified.

.PARAMETER link
Adds a 'ledger' hard link during installation. A hard link can be useful if you want to mimic the original Ledger and type something like 'ledger -f etc.' on the command line.

.EXAMPLE
PS> install

Installs the preferred NLedger. The path to the preferred NLedger is displayed by the status command.

.EXAMPLE
PS> install net6

Installs NLedger binaries built for the 'net6' runtime with the 'Release' profile. The list of available binaries is displayed by the status command.

.EXAMPLE
PS> install net5 debug

Installs NLedger binaries built for the 'net5' runtime with the 'Debug' profile (assuming they were built locally)

.EXAMPLE
PS> install -link

Install the preferred NLedger and create a hardlink 'ledger' to it.

.NOTES
Since the PATH variable changes during installation, it is recommended to restart the console after installation (if you are using it in interactive mode).

In some cases, this command may require administrator rights (for example, to create a hard link or run NGen).
#>
function Install-NLedger {
  [CmdletBinding()]
  [Alias("install")]
  Param(
    [Parameter()][string]$tfmCode,
    [Parameter()][ValidateSet("debug","release",IgnoreCase=$true)][string]$profile,
    [switch]$link
  )

  Assert-CommandCompleted {
    $deploymentInfo = Get-NLedgerDeploymentInfo
    $info = $deploymentInfo | Select-NLedgerPreferrableInfo $tfmCode $profile

    if ($info.IsInstalled) { return "NLedger binaries for {c:Yellow}$(Out-TfmProfile $info.TfmCode $info.IsDebug){f:Normal} are already installed." | Out-AnsiString }

    if ($deploymentInfo.EffectiveInstalled) { $null = Uninstall-NLedgerExecutable }
    Install-NLedgerExecutable $info.TfmCode $info.IsDebug -link:$link | Format-List
  }
}

<#
.SYNOPSIS
Uninstalls the current version of NLedger on the machine

.DESCRIPTION
The command undoes the changes made when installing NLedger on your computer:
- removes the path to NLedger from the PATH variable
- deletes a hard link named "ledger" if it exists
- For .Net Frameworks, it removes the NLedger image from the NGen cache.

The command cannot remove an external NLedger installation (if NLedger does not belong to the current folder); in this case you can change PATH manually.

.EXAMPLE
PS> uninstall

Removes the currently installed NLedger from the system.

.NOTES
Since the PATH variable changes during installation, it is recommended to restart the console after installation (if you are using it in interactive mode).

In some cases, this command may require administrator rights (for example, to remove a hard link or run NGen).
#>
function Uninstall-NLedger {
  [CmdletBinding()]
  [Alias("uninstall")]
  Param()

  Assert-CommandCompleted {
    $deploymentInfo = Get-NLedgerDeploymentInfo
    if (!$deploymentInfo.Installed) { return "NLedger is not installed."}

    $null = Uninstall-NLedgerExecutable
    "NLedger is uninstalled"
  }
}

<#
.SYNOPSIS
Updates NLedger confiuration settings

.DESCRIPTION
The command is intended to add or update NLedger configuration settings.
It takes the parameter name and value and updates the corresponding configuration file.

By default, it updates the user configuration file, which assumes 
the scope is "user" (see "show-config" for more information).
You can specify a different scope, such as the "application" scope 
(application configuration file) or the "public" scope (for all users).
If the configuration file does not exist, it will be created.

Some settings require values within the acceptable range. For example, 
Boolean settings accept only true or false values.
Review the output of 'show-config -details' to verify the exact list of allowed values.

.PARAMETER settingName
The name of the NLedger configuration parameter. Allowed names are provided by the 'show-config' command.

.PARAMETER settingValue
The configuration parameter value you want to apply. See the "show-config -details" output 
for a list of allowed values (if the option accepts only allowed values).

.PARAMETER scope
Specifies the scope of the configuration parameter value. 
Valid values are "user" (current user scope - default), 
"general" (all users), or "application" (application configuration).

.EXAMPLE
PS> set-config AnsiTerminalEmulation false

Disables Ansi terminal emulation in NLedger for the current user.

.EXAMPLE
PS> set-config TimeZoneId "GMT Standard Time" common

Sets the time zone to "GMT Standard Time" for all NLedger users on the current computer.

.NOTES
In some cases, this command may require administrator rights (for example, to change an application's configuration file).
#>
function Set-NLedgerConfig {
  [CmdletBinding()]
  [Alias("set-config")]
  Param(
    [Parameter(Mandatory)][string]$settingName,
    [Parameter(Mandatory)][AllowEmptyString()][string]$settingValue,
    [Parameter()][ValidateSet("app","common","user",IgnoreCase=$true)][string]$scope = "user"
  )

  Assert-CommandCompleted { Set-NLedgerConfigValue -scope $scope -settingName $settingName -settingValue $settingValue | Format-List }
}

<#
.SYNOPSIS
Remove NLedger confiuration settings

.DESCRIPTION
If you want to undo configuration changes, you can remove the entered configuration parameter.
By default, the command tries to remove the setting for the "user" scope, but you can specify alternatives.
If the command removes the last setting in a configuration file, the file itself will also be removed.

.PARAMETER settingName
The name of the NLedger configuration parameter. Allowed names are provided by the 'show-config' command.

.PARAMETER scope
Specifies the scope of the configuration parameter value. 
Valid values are "user" (current user scope - default), 
"general" (all users), or "application" (application configuration).

.EXAMPLE
PS> remove-config AnsiTerminalEmulation

Undoes disabling Ansi terminal emulation in NLedger for the current user.

.EXAMPLE
PS> remove-config TimeZoneId common

Resets the time zone to the default for all NLedger users on the current machine.

.NOTES
In some cases, this command may require administrator rights (for example, to change an application's configuration file).
#>
function Remove-NLedgerConfig {
  [CmdletBinding()]
  [Alias("remove-config")]
  Param(
    [Parameter(Mandatory)][string]$settingName,
    [Parameter()][ValidateSet("app","common","user",IgnoreCase=$true)][string]$scope = "user"
  )

  Assert-CommandCompleted { Remove-NLedgerConfigValue -scope $scope -settingName $settingName | Format-List }
}

<#
.SYNOPSIS
Shows effective NLedger confiuration settings

.DESCRIPTION
NLedger settings can be stored in configuration files or environment variables. 
If the parameter is not specified in any of the mentioned sources, NLedger uses 
the default (hard-coded) value.

This command provides comprehensive information about the current configuration
settings and their effective values. In particular, it shows the names of 
the settings, their types and values defined in each scope (listed in order of priority):
- Default: values specified in the code.
- Application: The value specified in the application configuration file.
- Common: value specified in the common (accessible to all users) configuration file.
- User: value specified in the current user's configuration file.
- Env: The value specified in the environment variable (prefixed with "nledger" followed by the parameter name).
- Effective: The effective setting value used by NLedger.

With the 'details' flag, this command also shows the physical location of 
configuration files and lists of allowed values (for settings that validate input)

.PARAMETER settingFilter
This option allows you to display a limited set of configuration options by filtering by their names.

.PARAMETER details
With the 'details' flag, this command shows detail information about configuration settings.

.EXAMPLE
PS> show-config

Shows NLedger configuration settings in a table.

.EXAMPLE
PS> show-config demo

Shows NLedger configuration settings that have the text "demo" in their name.

.EXAMPLE
PS> show-config -details

Shows detail information about NLedger configuration settings.

.NOTES
For information about the NLedger Python connection file, see the 'python-status' command.
#>
function Show-NLedgerConfig {
  [CmdletBinding()]
  [Alias("show-config")]
  Param(
    [Parameter()][string]$settingFilter,
    [Switch]$details
  )

  Assert-CommandCompleted {
    $data = Get-NLedgerConfigData
    $settings = $(if($settingFilter) { $data.Settings | Where-Object { $_.Name -match $settingFilter} } else { $data.Settings })

    if (!$details) {
      $settings | ForEach-Object {
        [PSCustomObject]@{
          Name = "{c:Yellow}$($_.Name){f:Normal}" | Out-AnsiString
          Type = "{c:Gray}$($_.SettingType){f:Normal}" | Out-AnsiString
          Default = $_.DefaultValue
          App = $_.AppSettingValue
          Common = $_.CommonSettingValue
          User = $_.UserSettingValue
          Env = $_.EnvironmentSettingValue
          Effective = "{c:Yellow}$($_.EffectiveSettingValue){f:Normal}" | Out-AnsiString
        }
      } | Format-Table
    } else {
      @(
        [PSCustomObject]@{ Scope='Common settings'; File=$data.CommonSettingsFile; Exists=($data.CommonSettingsFileExists | Out-YesNo) }
        [PSCustomObject]@{ Scope='User settings'; File=$data.UserSettingsFile; Exists=($data.UserSettingsFileExists | Out-YesNo) }
        [PSCustomObject]@{ Scope='App settings'; File=$data.AppSettingsFile; Exists=($data.AppSettingsFileExists | Out-YesNo) }
      ) | Format-Table

      function FormatValue {
        param (
          [Parameter()][bool]$hasValue,
          [Parameter()][bool]$isAvailable,
          [Parameter()][string]$value
        )
        if (!$isAvailable) { "N/A" }
        else { if (!$hasValue) { "-" } else { "'$value'" } }
      }

      $settings | ForEach-Object {
        [PSCustomObject]@{
          Name = "{c:Yellow}$($_.Name){f:Normal}" | Out-AnsiString
          Category = $_.Category
          Description = $_.Description
          Type = "{c:Gray}$($_.SettingType){f:Normal}" | Out-AnsiString
          Permitted = "{c:Gray}$($_.PossibleValues -join ","){f:Normal}" | Out-AnsiString
          Default = FormatValue $_.HasDefaultValue $true $_.DefaultValue
          App = FormatValue $_.HasAppSettingValue $_.IsAvailableForApp $_.AppSettingValue
          Common = FormatValue $_.HasCommonSettingValue $_.IsAvailableForCommon $_.CommonSettingValue
          User = FormatValue $_.HasUserSettingValue $_.IsAvailableForUser $_.UserSettingValue
          Env = FormatValue  $_.HasEnvironmentSettingValue $_.IsAvailableForEnv $_.EnvironmentSettingValue
          Effective = FormatValue $_.HasEffectiveSettingValue $true "{c:Yellow}$($_.EffectiveSettingValue){f:Normal}" | Out-AnsiString
        }
      } | Format-List
    }
  }
}

<#
.SYNOPSIS
Shows available commands

.DESCRIPTION
Collects information about available commands from associated reference 
descriptions and displays the corresponding aliases and synopsis in a table.

.EXAMPLE
PS> help

Shows a quick reference list of available commands.

.NOTES
Use get-help [command] to get more information about a specific command.
#>
function Get-QuickHelp {
  [CmdletBinding()]
  [Alias("help")]
  Param()

  Write-Output ("`n{c:DarkCyan}Available console commands (use 'get-help [command]' for more information){f:Normal}`n" | Out-AnsiString )

  $highlighted = @( "status","install","test","python-connect","help" )

  Assert-CommandCompleted {
    ((Get-Command $Script:CommandPath).Parameters.Command.Attributes | Where-Object { $_.TypeId.Name -eq "ValidateSetAttribute" }).ValidValues | 
    ForEach-Object { 
      $h = Get-Help $_; 
      [PsCustomObject] @{ 
        Alias = "$(if($_ -in $highlighted){"{c:Yellow}"}else{"{c:DarkYellow}"})$_{f:Normal}" | Out-AnsiString
        Description = $h.Synopsis 
      } 
    } | Format-Table -HideTableHeaders -AutoSize
  }

}

if($command) {Invoke-Expression (Get-CommandExpression $command $commandArguments) } else {
  Write-Output "{c:White}NLedger Control Console{f:Normal}`nUse '{c:Yellow}help{f:Normal}' to get quick help on the available commands.`n" | Out-AnsiString
}