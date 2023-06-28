# Tests for SysDotnet.psm1 module
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 5 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath/../SysDotnet.psm1 -Force

describe 'Get-NetFrameworkSDKs' {
    Context 'Run on non-Windows machine' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName Get-SpecialFolderProgramFilesX86 -MockWith { "" } }
        it "Returns empty string on non-Windows machine" { Get-NetFrameworkSDKs | Should -BeNullOrEmpty }
    }
    Context 'Run on Windows machine' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Get-SpecialFolderProgramFilesX86 -MockWith { "C:\Program86" } 
            Mock -ModuleName SysDotnet -CommandName Get-ChildItem -ParameterFilter { 
                ($Path -eq "C:\Program86/Reference Assemblies/Microsoft/Framework/.NETFramework") -and $Directory
            } -MockWith { 
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.X"
            } 
        }
        it "Returns SDK versions ignoring folders that do not match version pattern" { Get-NetFrameworkSDKs | Should -Be @(
            [version]"4.0"
            [version]"4.5"
            [version]"4.5.1"
            [version]"4.5.2"
            [version]"4.6"
            [version]"4.6.1"
            [version]"4.7.2"
            [version]"4.8"
        ) }
    }
}

describe 'Get-NetFrameworkSdkTargets' {
    Context 'Run on non-Windows machine' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName Get-SpecialFolderProgramFilesX86 -MockWith { "" } }
        it "Returns empty string on non-Windows machine" { Get-NetFrameworkSdkTargets | Should -BeNullOrEmpty }
    }
    Context 'Run on Windows machine' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Get-SpecialFolderProgramFilesX86 -MockWith { "C:\Program86" } 
            Mock -ModuleName SysDotnet -CommandName Get-ChildItem -ParameterFilter { 
                ($Path -eq "C:\Program86/Reference Assemblies/Microsoft/Framework/.NETFramework") -and $Directory
            } -MockWith { 
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
                "C:\Program86\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.X"
            } 
        }
        it "Returns SDK targets in TFM code format" { Get-NetFrameworkSdkTargets | Should -Be @(
            "net40"
            "net45"
            "net451"
            "net452"
            "net46"
            "net461"
            "net472"
            "net48"
        ) }
    }
}

describe 'Get-NetFrameworkRuntimeTarget' {
    Context 'Run on non-Windows machine' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $false } }
        it "Returns empty string on non-Windows machine" { Get-NetFrameworkRuntimeTarget | Should -BeNullOrEmpty }
    }
    Context 'Run on Windows machine with .Net Framework 4.8.1' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 533320 } 
        }
        it "Returns net481 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net481" }
    }
    Context 'Run on Windows machine with .Net Framework 4.8' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 528040 } 
        }
        it "Returns net48 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net48" }
    }
    Context 'Run on Windows machine with .Net Framework 4.7.2' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 461808 } 
        }
        it "Returns net472 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net472" }
    }
    Context 'Run on Windows machine with .Net Framework 4.7.1' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 461308 } 
        }
        it "Returns net471 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net471" }
    }
    Context 'Run on Windows machine with .Net Framework 4.7' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 460798 } 
        }
        it "Returns net47 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net47" }
    }
    Context 'Run on Windows machine with .Net Framework 4.6.2' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 394802 } 
        }
        it "Returns net462 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net462" }
    }
    Context 'Run on Windows machine with .Net Framework 4.6.1' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 394254 } 
        }
        it "Returns net461 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net461" }
    }
    Context 'Run on Windows machine with .Net Framework 4.6' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 393295 } 
        }
        it "Returns net46 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net46" }
    }
    Context 'Run on Windows machine with .Net Framework 4.5.2' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 379893 } 
        }
        it "Returns net452 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net452" }
    }
    Context 'Run on Windows machine with .Net Framework 4.5.1' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 378675 } 
        }
        it "Returns net451 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net451" }
    }
    Context 'Run on Windows machine with .Net Framework 4.5' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 378389 } 
        }
        it "Returns net45 TFM code" { Get-NetFrameworkRuntimeTarget | Should -Be "net45" }
    }
    Context 'Run on Windows machine with .Net Framework older than 4.5' {
        BeforeEach { 
            Mock -ModuleName SysDotnet -CommandName Test-IsWindows -MockWith { $true }
            Mock -ModuleName SysDotnet -CommandName Get-ItemPropertyValue -ParameterFilter { 
                ($LiteralPath -eq "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full") -and $Name -eq "Release"
            } -MockWith { 378389 - 1 } 
        }
        it "Returns empty result" { Get-NetFrameworkRuntimeTarget | Should -BeNullOrEmpty }
    }

}

describe 'Test-IsDotnetInstalled' {
    Context 'Run on a machine without dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName get-command -MockWith { $false } }
        it "Returns False if dotnet is not installed" { Test-IsDotnetInstalled | Should -BeFalse }
    }
    Context 'Run on a machine with dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName get-command -MockWith { "Represents a valid command object..." } }
        it "Returns True if dotnet is installed" { Test-IsDotnetInstalled | Should -BeTrue }
    }
}

describe 'Get-DotnetSdks' {
    Context 'Run on a machine with dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName dotnet -MockWith { 
            "3.1.426 [C:\Program Files\dotnet\sdk]"
            "5.0.102 [C:\Program Files\dotnet\sdk]"
            "6.0.201 [C:\Program Files\dotnet\sdk]"
            "7.0.103 [C:\Program Files\dotnet\sdk]"
        } }
        it "Returns list of installed SDK versions" { Get-DotnetSdks | Should -Be @(
            [version]"3.1.426"
            [version]"5.0.102"
            [version]"6.0.201"
            [version]"7.0.103"
        ) }
    }
}

describe 'Get-DotnetRuntimes' {
    Context 'Run on a machine with dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName dotnet -MockWith { 
            "Microsoft.AspNetCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.NETCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.WindowsDesktop.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
        } }
        it "Returns list of installed runtime names and versions" { Get-DotnetRuntimes | ForEach-Object { "$_.Name-$.Version" } | Should -Be (@(
            [PSCustomObject]@{Name = "Microsoft.AspNetCore.App"; Version = [version]"3.1.32"}
            [PSCustomObject]@{Name = "Microsoft.AspNetCore.App"; Version = [version]"5.0.2"}
            [PSCustomObject]@{Name = "Microsoft.AspNetCore.App"; Version = [version]"6.0.3"}
            [PSCustomObject]@{Name = "Microsoft.AspNetCore.App"; Version = [version]"6.0.14"}
            [PSCustomObject]@{Name = "Microsoft.AspNetCore.App"; Version = [version]"7.0.3"}
            [PSCustomObject]@{Name = "Microsoft.NETCore.App"; Version = [version]"3.1.32"}
            [PSCustomObject]@{Name = "Microsoft.NETCore.App"; Version = [version]"5.0.2"}
            [PSCustomObject]@{Name = "Microsoft.NETCore.App"; Version = [version]"6.0.3"}
            [PSCustomObject]@{Name = "Microsoft.NETCore.App"; Version = [version]"6.0.14"}
            [PSCustomObject]@{Name = "Microsoft.NETCore.App"; Version = [version]"7.0.3"}
            [PSCustomObject]@{Name = "Microsoft.WindowsDesktop.App"; Version = [version]"3.1.32"}
            [PSCustomObject]@{Name = "Microsoft.WindowsDesktop.App"; Version = [version]"5.0.2"}
            [PSCustomObject]@{Name = "Microsoft.WindowsDesktop.App"; Version = [version]"6.0.3"}
            [PSCustomObject]@{Name = "Microsoft.WindowsDesktop.App"; Version = [version]"6.0.14"}
            [PSCustomObject]@{Name = "Microsoft.WindowsDesktop.App"; Version = [version]"7.0.3"}
        ) | ForEach-Object { "$_.Name-$.Version" } ) }
    }
}

describe 'Get-DotnetSdkTargets' {
    Context 'Run on a machine with dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName dotnet -MockWith { 
            "3.1.426 [C:\Program Files\dotnet\sdk]"
            "5.0.102 [C:\Program Files\dotnet\sdk]"
            "6.0.201 [C:\Program Files\dotnet\sdk]"
            "7.0.103 [C:\Program Files\dotnet\sdk]"
        } }
        it "Returns list of installed SDK in TFM code format" { Get-DotnetSdkTargets | Should -Be @(
            "netcoreapp3.1"
            "net5.0"
            "net6.0"
            "net7.0"
        ) }
    }
}

describe 'Get-DotnetRuntimeNetCoreTargets' {
    Context 'Run on a machine with dotnet' {
        BeforeEach { Mock -ModuleName SysDotnet -CommandName dotnet -MockWith { 
            "Microsoft.AspNetCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.AspNetCore.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]"
            "Microsoft.NETCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.NETCore.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]"
            "Microsoft.WindowsDesktop.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 5.0.2 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 6.0.3 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 6.0.14 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
            "Microsoft.WindowsDesktop.App 7.0.3 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
        } }
        it "Returns list of installed NetCore.App runtimes in TFM code format" { Get-DotnetRuntimeNetCoreTargets | Should -Be @(
            "netcoreapp3.1"
            "net5.0"
            "net6.0"
            "net7.0"
        ) }
    }
}

describe 'Test-IsTfmCode' {
    it "Recognizes valid TFM code netcoreapp1.0" { Test-IsTfmCode "netcoreapp1.0" | Should -BeTrue }
    it "Recognizes valid TFM code netcoreapp2.0" { Test-IsTfmCode "netcoreapp2.0" | Should -BeTrue }
    it "Recognizes valid TFM code netcoreapp2.1" { Test-IsTfmCode "netcoreapp2.1" | Should -BeTrue }
    it "Recognizes valid TFM code netcoreapp2.2" { Test-IsTfmCode "netcoreapp2.2" | Should -BeTrue }
    it "Recognizes valid TFM code netcoreapp3.0" { Test-IsTfmCode "netcoreapp3.0" | Should -BeTrue }
    it "Recognizes valid TFM code netcoreapp3.1" { Test-IsTfmCode "netcoreapp3.1" | Should -BeTrue }
    it "Recognizes valid TFM code net5.0" { Test-IsTfmCode "net5.0" | Should -BeTrue }
    it "Recognizes valid TFM code net6.0" { Test-IsTfmCode "net6.0" | Should -BeTrue }
    it "Recognizes valid TFM code net7.0" { Test-IsTfmCode "net7.0" | Should -BeTrue }
    it "Recognizes valid TFM code net11" { Test-IsTfmCode "net11" | Should -BeTrue }
    it "Recognizes valid TFM code net20" { Test-IsTfmCode "net20" | Should -BeTrue }
    it "Recognizes valid TFM code net35" { Test-IsTfmCode "net35" | Should -BeTrue }
    it "Recognizes valid TFM code net40" { Test-IsTfmCode "net40" | Should -BeTrue }
    it "Recognizes valid TFM code net403" { Test-IsTfmCode "net403" | Should -BeTrue }
    it "Recognizes valid TFM code net45" { Test-IsTfmCode "net45" | Should -BeTrue }
    it "Recognizes valid TFM code net451" { Test-IsTfmCode "net451" | Should -BeTrue }
    it "Recognizes valid TFM code net452" { Test-IsTfmCode "net452" | Should -BeTrue }
    it "Recognizes valid TFM code net46" { Test-IsTfmCode "net46" | Should -BeTrue }
    it "Recognizes valid TFM code net461" { Test-IsTfmCode "net461" | Should -BeTrue }
    it "Recognizes valid TFM code net462" { Test-IsTfmCode "net462" | Should -BeTrue }
    it "Recognizes valid TFM code net47" { Test-IsTfmCode "net47" | Should -BeTrue }
    it "Recognizes valid TFM code net471" { Test-IsTfmCode "net471" | Should -BeTrue }
    it "Recognizes valid TFM code net472" { Test-IsTfmCode "net472" | Should -BeTrue }
    it "Recognizes valid TFM code net48" { Test-IsTfmCode "net48" | Should -BeTrue }
    it "Recognizes invalid TFM code" { Test-IsTfmCode "code12" | Should -BeFalse }
}

describe 'Test-IsFrameworkTfmCode' {
    it "Recognizes non-framework TFM code netcoreapp1.0" { Test-IsFrameworkTfmCode "netcoreapp1.0" | Should -BeFalse }
    it "Recognizes non-framework TFM code netcoreapp2.0" { Test-IsFrameworkTfmCode "netcoreapp2.0" | Should -BeFalse }
    it "Recognizes non-framework TFM code netcoreapp2.1" { Test-IsFrameworkTfmCode "netcoreapp2.1" | Should -BeFalse }
    it "Recognizes non-framework TFM code netcoreapp2.2" { Test-IsFrameworkTfmCode "netcoreapp2.2" | Should -BeFalse }
    it "Recognizes non-framework TFM code netcoreapp3.0" { Test-IsFrameworkTfmCode "netcoreapp3.0" | Should -BeFalse }
    it "Recognizes non-framework TFM code netcoreapp3.1" { Test-IsFrameworkTfmCode "netcoreapp3.1" | Should -BeFalse }
    it "Recognizes non-framework TFM code net5.0" { Test-IsFrameworkTfmCode "net5.0" | Should -BeFalse }
    it "Recognizes non-framework TFM code net6.0" { Test-IsFrameworkTfmCode "net6.0" | Should -BeFalse }
    it "Recognizes non-framework TFM code net7.0" { Test-IsFrameworkTfmCode "net7.0" | Should -BeFalse }
    it "Recognizes framework TFM code net11" { Test-IsFrameworkTfmCode "net11" | Should -BeTrue }
    it "Recognizes framework TFM code net20" { Test-IsFrameworkTfmCode "net20" | Should -BeTrue }
    it "Recognizes framework TFM code net35" { Test-IsFrameworkTfmCode "net35" | Should -BeTrue }
    it "Recognizes framework TFM code net40" { Test-IsFrameworkTfmCode "net40" | Should -BeTrue }
    it "Recognizes framework TFM code net403" { Test-IsFrameworkTfmCode "net403" | Should -BeTrue }
    it "Recognizes framework TFM code net45" { Test-IsFrameworkTfmCode "net45" | Should -BeTrue }
    it "Recognizes framework TFM code net451" { Test-IsFrameworkTfmCode "net451" | Should -BeTrue }
    it "Recognizes framework TFM code net452" { Test-IsFrameworkTfmCode "net452" | Should -BeTrue }
    it "Recognizes framework TFM code net46" { Test-IsFrameworkTfmCode "net46" | Should -BeTrue }
    it "Recognizes framework TFM code net461" { Test-IsFrameworkTfmCode "net461" | Should -BeTrue }
    it "Recognizes framework TFM code net462" { Test-IsFrameworkTfmCode "net462" | Should -BeTrue }
    it "Recognizes framework TFM code net47" { Test-IsFrameworkTfmCode "net47" | Should -BeTrue }
    it "Recognizes framework TFM code net471" { Test-IsFrameworkTfmCode "net471" | Should -BeTrue }
    it "Recognizes framework TFM code net472" { Test-IsFrameworkTfmCode "net472" | Should -BeTrue }
    it "Recognizes framework TFM code net48" { Test-IsFrameworkTfmCode "net48" | Should -BeTrue }
    it "Recognizes invalid TFM code" { Test-IsFrameworkTfmCode "code12" | Should -BeFalse }
}