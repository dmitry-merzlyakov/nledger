# Tests for NLedger Where module (NLWhere.psm1)
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 4 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\..\NLWhere.psm1 -Force

describe 'Get-NLedgerInstances' {

    Context "Collecting NLedger Instances" {

        [string]$ParentPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..")
        [string]$RootPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..\..")
        [string]$env:RootPath = $RootPath # It is needed for return value mock block; it does not have access to the current context

        mock -ModuleName NLWhere getFullPath { throw "Unexpected test path" } 
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\bin\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\ledger.exe" } -ParameterFilter { $path -eq "$ParentPath\..\ledger.exe" }

        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\ledger.exe" -and $PathType -eq "Leaf" }

        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:01", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:02", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:03", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:04", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:05", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:06", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:07", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\ledger.exe" }

        $result = Get-NLedgerInstances

        it 'found all available NLedger instances' { $result.Count | Should -Be 7 }

        it 'Validate NLedger instance 1 - Path' { ($result | Select-Object -Index 0).Path | Should -Be "$RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" }
        it 'Validate NLedger instance 1 - Date' { ($result | Select-Object -Index 0).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:01", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 1 - IsDebug' { ($result | Select-Object -Index 0).IsDebug | Should -Be $true }
        it 'Validate NLedger instance 1 - IsRelease' { ($result | Select-Object -Index 0).IsRelease | Should -Be $false }
        it 'Validate NLedger instance 1 - IsPackage' { ($result | Select-Object -Index 0).IsPackage | Should -Be $false }
        it 'Validate NLedger instance 1 - IsCore' { ($result | Select-Object -Index 0).IsCore | Should -Be $false }
        it 'Validate NLedger instance 1 - IsAlias' { ($result | Select-Object -Index 0).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 2 - Path' { ($result | Select-Object -Index 1).Path | Should -Be "$RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" }
        it 'Validate NLedger instance 2 - Date' { ($result | Select-Object -Index 1).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:02", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 2 - IsDebug' { ($result | Select-Object -Index 1).IsDebug | Should -Be $true }
        it 'Validate NLedger instance 2 - IsRelease' { ($result | Select-Object -Index 1).IsRelease | Should -Be $false }
        it 'Validate NLedger instance 2 - IsPackage' { ($result | Select-Object -Index 1).IsPackage | Should -Be $false }
        it 'Validate NLedger instance 2 - IsCore' { ($result | Select-Object -Index 1).IsCore | Should -Be $true }
        it 'Validate NLedger instance 2 - IsAlias' { ($result | Select-Object -Index 1).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 3 - Path' { ($result | Select-Object -Index 2).Path | Should -Be "$RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" }
        it 'Validate NLedger instance 3 - Date' { ($result | Select-Object -Index 2).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:03", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 3 - IsDebug' { ($result | Select-Object -Index 2).IsDebug | Should -Be $false }
        it 'Validate NLedger instance 3 - IsRelease' { ($result | Select-Object -Index 2).IsRelease | Should -Be $true }
        it 'Validate NLedger instance 3 - IsPackage' { ($result | Select-Object -Index 2).IsPackage | Should -Be $false }
        it 'Validate NLedger instance 3 - IsCore' { ($result | Select-Object -Index 2).IsCore | Should -Be $false }
        it 'Validate NLedger instance 3 - IsAlias' { ($result | Select-Object -Index 2).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 4 - Path' { ($result | Select-Object -Index 3).Path | Should -Be "$RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" }
        it 'Validate NLedger instance 4 - Date' { ($result | Select-Object -Index 3).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:04", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 4 - IsDebug' { ($result | Select-Object -Index 3).IsDebug | Should -Be $false }
        it 'Validate NLedger instance 4 - IsRelease' { ($result | Select-Object -Index 3).IsRelease | Should -Be $true }
        it 'Validate NLedger instance 4 - IsPackage' { ($result | Select-Object -Index 3).IsPackage | Should -Be $false }
        it 'Validate NLedger instance 4 - IsCore' { ($result | Select-Object -Index 3).IsCore | Should -Be $true }
        it 'Validate NLedger instance 4 - IsAlias' { ($result | Select-Object -Index 3).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 5 - Path' { ($result | Select-Object -Index 4).Path | Should -Be "$RootPath\Contrib\NLedger-cli.exe" }
        it 'Validate NLedger instance 5 - Date' { ($result | Select-Object -Index 4).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:05", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 5 - IsDebug' { ($result | Select-Object -Index 4).IsDebug | Should -Be $false }
        it 'Validate NLedger instance 5 - IsRelease' { ($result | Select-Object -Index 4).IsRelease | Should -Be $false }
        it 'Validate NLedger instance 5 - IsPackage' { ($result | Select-Object -Index 4).IsPackage | Should -Be $true }
        it 'Validate NLedger instance 5 - IsCore' { ($result | Select-Object -Index 4).IsCore | Should -Be $false }
        it 'Validate NLedger instance 5 - IsAlias' { ($result | Select-Object -Index 4).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 6 - Path' { ($result | Select-Object -Index 5).Path | Should -Be "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" }
        it 'Validate NLedger instance 6 - Date' { ($result | Select-Object -Index 5).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:06", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 6 - IsDebug' { ($result | Select-Object -Index 5).IsDebug | Should -Be $false }
        it 'Validate NLedger instance 6 - IsRelease' { ($result | Select-Object -Index 5).IsRelease | Should -Be $false }
        it 'Validate NLedger instance 6 - IsPackage' { ($result | Select-Object -Index 5).IsPackage | Should -Be $true }
        it 'Validate NLedger instance 6 - IsCore' { ($result | Select-Object -Index 5).IsCore | Should -Be $true }
        it 'Validate NLedger instance 6 - IsAlias' { ($result | Select-Object -Index 5).IsAlias | Should -Be $false }

        it 'Validate NLedger instance 7 - Path' { ($result | Select-Object -Index 6).Path | Should -Be "$RootPath\Contrib\ledger.exe" }
        it 'Validate NLedger instance 7 - Date' { ($result | Select-Object -Index 6).Date | Should -Be $([DateTime]::ParseExact("2020/10/10 09:00:07", "yyyy/MM/dd hh:mm:ss", $null)) }
        it 'Validate NLedger instance 7 - IsDebug' { ($result | Select-Object -Index 6).IsDebug | Should -Be $false }
        it 'Validate NLedger instance 7 - IsRelease' { ($result | Select-Object -Index 6).IsRelease | Should -Be $false }
        it 'Validate NLedger instance 7 - IsPackage' { ($result | Select-Object -Index 6).IsPackage | Should -Be $true }
        it 'Validate NLedger instance 7 - IsCore' { ($result | Select-Object -Index 6).IsCore | Should -Be $false }
        it 'Validate NLedger instance 7 - IsAlias' { ($result | Select-Object -Index 6).IsAlias | Should -Be $true }
   }
}

describe 'Select-NLedgerInstance' {

    Context "Collecting NLedger Instances" {

        it 'Returns null if no input instances' { @() | Select-NLedgerInstance | Should -BeNullOrEmpty }
        it 'Returns null if no instances selected' {
            $instances = @(
                [PsCustomObject]@{
                    Path = "some-path"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $false
                    IsAlias = $true  # Note: Select-NLedgerInstance ignores an alias
                })
            $instances | Select-NLedgerInstance | Should -BeNullOrEmpty }
        it 'Returns the latest instance' {
            $instances = @(
                [PsCustomObject]@{
                    Path = "some-path1"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $false
                    IsAlias = $false
                },
                [PsCustomObject]@{
                    Path = "some-path2"
                    Date = [DateTime]::Now.AddDays(1)
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $false
                    IsAlias = $false
                })
            ($instances | Select-NLedgerInstance).Path | Should -Be "some-path2" }
        it 'Prefers .Net Framework by default' {
            $instances = @(
                [PsCustomObject]@{
                    Path = "some-path1"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $true
                    IsAlias = $false
                },
                [PsCustomObject]@{
                    Path = "some-path2"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $false
                    IsAlias = $false
                })
            ($instances | Select-NLedgerInstance).Path | Should -Be "some-path2" }
        it 'Prefers .Net Core if switch is added' {
            $instances = @(
                [PsCustomObject]@{
                    Path = "some-path1"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $true
                    IsAlias = $false
                },
                [PsCustomObject]@{
                    Path = "some-path2"
                    Date = [DateTime]::Now
                    IsDebug = $false
                    IsRelease = $false
                    IsPackage = $false
                    IsCore = $false
                    IsAlias = $false
                })
            ($instances | Select-NLedgerInstance -preferCore).Path | Should -Be "some-path1" }
    }
}

describe 'Get-NLedgerPath' {

    Context "Getting NLedger path" {

        [string]$ParentPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..")
        [string]$RootPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..\..\..")
        [string]$env:RootPath = $RootPath # It is needed for return value mock block; it does not have access to the current context

        mock -ModuleName NLWhere getFullPath { throw "Unexpected test path" } 
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\..\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" } -ParameterFilter { $path -eq "$ParentPath\..\bin\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getFullPath { return "$env:RootPath\Contrib\ledger.exe" } -ParameterFilter { $path -eq "$ParentPath\..\ledger.exe" }

        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" -and $PathType -eq "Leaf" }
        mock -ModuleName NLWhere -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq "$RootPath\Contrib\ledger.exe" -and $PathType -eq "Leaf" }

        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:01", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Debug\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:02", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Debug\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:03", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Release\net45\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:04", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Source\NLedger.CLI\bin\Release\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:05", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:06", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe" }
        mock -ModuleName NLWhere getLastWriteTime { return [DateTime]::ParseExact("2020/10/10 09:00:07", "yyyy/MM/dd hh:mm:ss", $null) } -ParameterFilter { $path -eq "$RootPath\Contrib\ledger.exe" }

        it 'Returns path to the latest .Net Framework NLedger ignoring alias - by default' {
            Get-NLedgerPath | Should -Be "$RootPath\Contrib\NLedger-cli.exe"
        }

        it 'Returns path to the latest .Net Core NLedger ignoring alias - if switch is enabled' {
            Get-NLedgerPath -preferCore | Should -Be "$RootPath\Contrib\bin\netcoreapp3.1\NLedger-cli.exe"
        }
    }
}