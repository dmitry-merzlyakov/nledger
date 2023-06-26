# Tests for SysPlatform.psm1 module
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 4 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../SysPlatform.psm1 -Force

describe 'Test-IsWindows' {
    Context 'Run on any machine' {
       it 'Returns the same result as IsOSPlatform function' { Test-IsWindows | Should -Be ([bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows))}
    }
}

describe 'Test-IsOSX' {
    Context 'Run on any machine' {
        it 'Returns the same result as IsOSPlatform function' { Test-IsOSX | Should -Be ([bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX))}
    }
}

describe 'Test-IsLinux' {
    Context 'Run on any machine' {
        it 'Returns the same result as IsOSPlatform function' { Test-IsOSX | Should -Be ([bool][System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux))}
    }
}

describe 'Get-SpecialFolderProgramFilesX86' {
    Context 'Run on Windows machine' {
        BeforeEach { Mock Test-IsWindows { $true } }
        it 'Returns be equal to ProgramFilesX86' { Get-SpecialFolderProgramFilesX86 | Should -Be ([System.Environment]::GetFolderPath(([System.Environment+SpecialFolder]::ProgramFilesX86))) }
    }
    Context 'Run on non-Windows machine' {
        BeforeEach { Mock Test-IsWindows { $false } }
        it 'Returns empty result' { Get-SpecialFolderProgramFilesX86 | Should -BeNullOrEmpty }
    }
}
