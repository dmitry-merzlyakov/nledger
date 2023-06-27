# Tests for SysDotnet.psm1 module
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 5 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../SysDotnet.psm1 -Force

describe 'Convert-ColorTokensToAnsi' {
    Context 'Run on non-Windows machine' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName Get-SpecialFolderProgramFilesX86 -MockWith { "" } }
        it "Returns empty string on non-Windows machine" { Get-NetFrameworkSDKs | Should -BeNullOrEmpty }
    }
}