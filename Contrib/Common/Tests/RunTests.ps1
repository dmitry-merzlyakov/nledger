# This file runs all tests for PowerShell Contrib scripts.
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 5 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
[CmdletBinding()]
Param()

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

Import-Module Pester

$config=[PesterConfiguration] @{
  Run = @{
    Path=$Script:ScriptPath
    Exit=$true
  }
  TestResult = @{
    OutputPath="$($Script:ScriptPath)/TestResults.xml"
  }
}

Invoke-Pester -Configuration $config
