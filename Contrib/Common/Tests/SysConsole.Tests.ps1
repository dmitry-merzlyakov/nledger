# Tests for SysPlatform.psm1 module
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 5 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../SysConsole.psm1 -Force

describe 'Convert-ColorTokensToAnsi' {
    Context 'Run with ANSI_Colorization enabled' {
        BeforeEach { $Global:ANSI_Colorization = $true }
        AfterAll { $Global:ANSI_Colorization = $true }
        it 'Accepts empty strings' { Convert-ColorTokensToAnsi "" | Should -BeNullOrEmpty }
    }
    Context 'Run with ANSI_Colorization disabled' {
        BeforeEach { $Global:ANSI_Colorization = $false }
        AfterAll { $Global:ANSI_Colorization = $true }
        it 'Accepts empty strings' { Convert-ColorTokensToAnsi "" | Should -BeNullOrEmpty }
    }
}
