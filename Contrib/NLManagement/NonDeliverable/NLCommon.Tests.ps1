# Tests for NLedger Common module (NLCommon.psm1)
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 4 (or higher) needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\..\NLCommon.psm1 -Force

$Script:NLCommonScriptPath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..")

describe 'Write-ConsoleColors' {

    it 'it accepts empty string' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { !$Object -and !$NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-ConsoleColors "" | Out-Null
        Assert-VerifiableMock
    }

    it 'it sends string to write-host' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { $Object -eq "test string" -and !$NoNewline  -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-ConsoleColors "test string" | Out-Null
        Assert-VerifiableMock
    }

    it 'it passes NoNewLine switch' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { $Object -eq "test string" -and $NoNewline  -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-ConsoleColors "test string" -NoNewLine | Out-Null
        Assert-VerifiableMock
    }

    it 'it adds foreground color switcher if specified' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { $Object -eq "test string" -and !$NoNewline  -and "red" -eq $ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-ConsoleColors "test string" -foregroundColor red | Out-Null
        Assert-VerifiableMock
    }

    it 'it adds backgroundColor color switcher if specified' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { $Object -eq "test string" -and !$NoNewline  -and !$ForegroundColor -and "red" -eq $BackgroundColor } -Verifiable
        Write-ConsoleColors "test string" -backgroundColor red | Out-Null
        Assert-VerifiableMock
    }

    it 'it adds both color switchers if specified' {
        mock -ModuleName "NLCommon" -CommandName 'Write-Host' -ParameterFilter { $Object -eq "test string" -and !$NoNewline  -and "blue" -eq $ForegroundColor -and "red" -eq $BackgroundColor } -Verifiable
        Write-ConsoleColors "test string" -backgroundColor red -foregroundColor blue | Out-Null
        Assert-VerifiableMock
    }
}

describe 'Write-Console' {

    it 'it accepts empty string' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { !$inputString -and !$NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-Console "" | Out-Null
        Assert-VerifiableMock
    }

    it 'it passes strings with no tokens' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "test string" -and !$NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-Console "test string" | Out-Null
        Assert-VerifiableMock
    }

    it 'it passes NoNewLine switch' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "test string" -and $NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-Console "test string" -NoNewLine | Out-Null
        Assert-VerifiableMock
    }

    it 'it manages a color token at the beginning of a string' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { !$inputString -and $NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "test string" -and !$NoNewline -and "red" -eq $ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-Console "{c:red}test string" | Out-Null
        Assert-VerifiableMock
    }

    it 'it manages a color token in the middle of a string' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "test" -and $NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq " string" -and !$NoNewline -and "red" -eq $ForegroundColor -and !$BackgroundColor } -Verifiable
        Write-Console "test{c:red} string" | Out-Null
        Assert-VerifiableMock
    }

    it 'it manages foreground and background tokens' {
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "test" -and $NoNewline -and !$ForegroundColor -and !$BackgroundColor } -Verifiable
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq " str" -and $NoNewline -and "red" -eq $ForegroundColor -and !$BackgroundColor } -Verifiable
        mock -CommandName 'Write-ConsoleColors' -ModuleName NLCommon -ParameterFilter { $inputString -eq "ing" -and !$NoNewline -and "red" -eq $ForegroundColor -and "blue" -eq $BackgroundColor } -Verifiable
        Write-Console "test{c:red} str{b:blue}ing" | Out-Null
        Assert-VerifiableMock
    }
}

describe 'NLedgerLocation' {

    it 'it returns the path to production deployment if exists' { 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        $result = NLedgerLocation        
        $result | should be ([System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\.."))
        Assert-VerifiableMock
    }

    it 'it returns the path to debug deployment if it only exists' {
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        $result = NLedgerLocation        
        $result | should be ([System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release"))
        Assert-VerifiableMock
    }

    it 'it returns the path to release deployment if it only exists' {
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        $result = NLedgerLocation        
        $result | should be ([System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug"))
        Assert-VerifiableMock
    }

    it 'it raises exception if neither production nor debug nor release exists' {
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        { $result = NLedgerLocation } | should throw "Cannot find NLedger-cli.exe"
        Assert-VerifiableMock
    }

    it 'it returns the path to release deployment if it is earlier than debug' {
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Get-Item' { return ([PsCustomObject]@{ LastWriteTime=(Get-Date -Year 2018 -Month 1 -Day 1 )}) } -ParameterFilter { $Path -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") } 
        mock -ModuleName "NLCommon" -CommandName 'Get-Item' { return ([PsCustomObject]@{ LastWriteTime=(Get-Date -Year 2018 -Month 2 -Day 1 )}) } -ParameterFilter { $Path -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") } 
        $result = NLedgerLocation        
        $result | should be ([System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release"))
        Assert-VerifiableMock
    }

    it 'it returns the path to debug deployment if it is earlier than release' {
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $false } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Test-Path' { return $true } -ParameterFilter { $LiteralPath -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") -and $PathType -eq "Leaf" } 
        mock -ModuleName "NLCommon" -CommandName 'Get-Item' { return ([PsCustomObject]@{ LastWriteTime=(Get-Date -Year 2018 -Month 2 -Day 1 )}) } -ParameterFilter { $Path -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug\NLedger-cli.exe") } 
        mock -ModuleName "NLCommon" -CommandName 'Get-Item' { return ([PsCustomObject]@{ LastWriteTime=(Get-Date -Year 2018 -Month 1 -Day 1 )}) } -ParameterFilter { $Path -eq [System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Release\NLedger-cli.exe") } 
        $result = NLedgerLocation        
        $result | should be ([System.IO.Path]::GetFullPath("$Script:NLCommonScriptPath\..\..\Source\NLedger.CLI\bin\Debug"))
        Assert-VerifiableMock
    }
}