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

        it 'Accepts foreground color Black' { Convert-ColorTokensToAnsi "ABC{c:Black}DEF" | Should -BeExactly "ABC$([char]27)[30mDEF" }
        it 'Accepts foreground color DarkBlue' { Convert-ColorTokensToAnsi "ABC{c:DarkBlue}DEF" | Should -BeExactly "ABC$([char]27)[34mDEF" }
        it 'Accepts foreground color DarkGreen' { Convert-ColorTokensToAnsi "ABC{c:DarkGreen}DEF" | Should -BeExactly "ABC$([char]27)[32mDEF" }
        it 'Accepts foreground color DarkCyan' { Convert-ColorTokensToAnsi "ABC{c:DarkCyan}DEF" | Should -BeExactly "ABC$([char]27)[36mDEF" }
        it 'Accepts foreground color DarkRed' { Convert-ColorTokensToAnsi "ABC{c:DarkRed}DEF" | Should -BeExactly "ABC$([char]27)[31mDEF" }
        it 'Accepts foreground color DarkMagenta' { Convert-ColorTokensToAnsi "ABC{c:DarkMagenta}DEF" | Should -BeExactly "ABC$([char]27)[35mDEF" }
        it 'Accepts foreground color DarkYellow' { Convert-ColorTokensToAnsi "ABC{c:DarkYellow}DEF" | Should -BeExactly "ABC$([char]27)[33mDEF" }
        it 'Accepts foreground color Gray' { Convert-ColorTokensToAnsi "ABC{c:Gray}DEF" | Should -BeExactly "ABC$([char]27)[37mDEF" }
        it 'Accepts foreground color DarkGray' { Convert-ColorTokensToAnsi "ABC{c:DarkGray}DEF" | Should -BeExactly "ABC$([char]27)[30;1mDEF" }
        it 'Accepts foreground color Blue' { Convert-ColorTokensToAnsi "ABC{c:Blue}DEF" | Should -BeExactly "ABC$([char]27)[34;1mDEF" }
        it 'Accepts foreground color Green' { Convert-ColorTokensToAnsi "ABC{c:Green}DEF" | Should -BeExactly "ABC$([char]27)[32;1mDEF" }
        it 'Accepts foreground color Cyan' { Convert-ColorTokensToAnsi "ABC{c:Cyan}DEF" | Should -BeExactly "ABC$([char]27)[36;1mDEF" }
        it 'Accepts foreground color Red' { Convert-ColorTokensToAnsi "ABC{c:Red}DEF" | Should -BeExactly "ABC$([char]27)[31;1mDEF" }
        it 'Accepts foreground color Magenta' { Convert-ColorTokensToAnsi "ABC{c:Magenta}DEF" | Should -BeExactly "ABC$([char]27)[35;1mDEF" }
        it 'Accepts foreground color Yellow' { Convert-ColorTokensToAnsi "ABC{c:Yellow}DEF" | Should -BeExactly "ABC$([char]27)[33;1mDEF" }
        it 'Accepts foreground color White' { Convert-ColorTokensToAnsi "ABC{c:White}DEF" | Should -BeExactly "ABC$([char]27)[37;1mDEF" }
        it 'Accepts several foreground colors' { Convert-ColorTokensToAnsi "ABC{c:White}DEF123{c:Black}456" | Should -BeExactly "ABC$([char]27)[37;1mDEF123$([char]27)[30m456" }
        it 'Disallows wrong foreground color' { { Convert-ColorTokensToAnsi "ABC{c:Purple}DEF" } | Should -Throw "Invalid color in token: Purple" }

        it 'Accepts background color Black' { Convert-ColorTokensToAnsi "ABC{b:Black}DEF" | Should -BeExactly "ABC$([char]27)[40mDEF" }
        it 'Accepts background color DarkBlue' { Convert-ColorTokensToAnsi "ABC{b:DarkBlue}DEF" | Should -BeExactly "ABC$([char]27)[44mDEF" }
        it 'Accepts background color DarkGreen' { Convert-ColorTokensToAnsi "ABC{b:DarkGreen}DEF" | Should -BeExactly "ABC$([char]27)[42mDEF" }
        it 'Accepts background color DarkCyan' { Convert-ColorTokensToAnsi "ABC{b:DarkCyan}DEF" | Should -BeExactly "ABC$([char]27)[46mDEF" }
        it 'Accepts background color DarkRed' { Convert-ColorTokensToAnsi "ABC{b:DarkRed}DEF" | Should -BeExactly "ABC$([char]27)[41mDEF" }
        it 'Accepts background color DarkMagenta' { Convert-ColorTokensToAnsi "ABC{b:DarkMagenta}DEF" | Should -BeExactly "ABC$([char]27)[45mDEF" }
        it 'Accepts background color DarkYellow' { Convert-ColorTokensToAnsi "ABC{b:DarkYellow}DEF" | Should -BeExactly "ABC$([char]27)[43mDEF" }
        it 'Accepts background color Gray' { Convert-ColorTokensToAnsi "ABC{b:Gray}DEF" | Should -BeExactly "ABC$([char]27)[47mDEF" }
        it 'Accepts background color DarkGray' { Convert-ColorTokensToAnsi "ABC{b:DarkGray}DEF" | Should -BeExactly "ABC$([char]27)[40;1mDEF" }
        it 'Accepts background color Blue' { Convert-ColorTokensToAnsi "ABC{b:Blue}DEF" | Should -BeExactly "ABC$([char]27)[44;1mDEF" }
        it 'Accepts background color Green' { Convert-ColorTokensToAnsi "ABC{b:Green}DEF" | Should -BeExactly "ABC$([char]27)[42;1mDEF" }
        it 'Accepts background color Cyan' { Convert-ColorTokensToAnsi "ABC{b:Cyan}DEF" | Should -BeExactly "ABC$([char]27)[46;1mDEF" }
        it 'Accepts background color Red' { Convert-ColorTokensToAnsi "ABC{b:Red}DEF" | Should -BeExactly "ABC$([char]27)[41;1mDEF" }
        it 'Accepts background color Magenta' { Convert-ColorTokensToAnsi "ABC{b:Magenta}DEF" | Should -BeExactly "ABC$([char]27)[45;1mDEF" }
        it 'Accepts background color Yellow' { Convert-ColorTokensToAnsi "ABC{b:Yellow}DEF" | Should -BeExactly "ABC$([char]27)[43;1mDEF" }
        it 'Accepts background color White' { Convert-ColorTokensToAnsi "ABC{b:White}DEF" | Should -BeExactly "ABC$([char]27)[47;1mDEF" }
        it 'Accepts several background colors' { Convert-ColorTokensToAnsi "ABC{b:White}DEF123{b:Black}456" | Should -BeExactly "ABC$([char]27)[47;1mDEF123$([char]27)[40m456" }
        it 'Disallows wrong background color' { { Convert-ColorTokensToAnsi "ABC{b:Purple}DEF" } | Should -Throw "Invalid color in token: Purple" }

        it 'Accepts format color Normal' { Convert-ColorTokensToAnsi "ABC{f:Normal}DEF" | Should -BeExactly "ABC$([char]27)[0mDEF" }
        it 'Accepts format color Bold' { Convert-ColorTokensToAnsi "ABC{f:Bold}DEF" | Should -BeExactly "ABC$([char]27)[1mDEF" }
        it 'Accepts format color Underline' { Convert-ColorTokensToAnsi "ABC{f:Underline}DEF" | Should -BeExactly "ABC$([char]27)[4mDEF" }
        it 'Accepts format color Blink' { Convert-ColorTokensToAnsi "ABC{f:Blink}DEF" | Should -BeExactly "ABC$([char]27)[5mDEF" }
        it 'Accepts several format colors' { Convert-ColorTokensToAnsi "ABC{f:Normal}DEF123{f:Blink}456" | Should -BeExactly "ABC$([char]27)[0mDEF123$([char]27)[5m456" }
        it 'Disallows wrong format color' { { Convert-ColorTokensToAnsi "ABC{f:Purple}DEF" } | Should -Throw "Invalid color in token: Purple" }

        it 'Accepts several foreground, background and format colors' { Convert-ColorTokensToAnsi "ABC{c:Black}DEF{b:White}KLM{f:Normal}" | Should -BeExactly "ABC$([char]27)[30mDEF$([char]27)[47;1mKLM$([char]27)[0m" }
    }
    Context 'Run with ANSI_Colorization disabled' {
        BeforeEach { $Global:ANSI_Colorization = $false }
        AfterAll { $Global:ANSI_Colorization = $true }
        it 'Accepts empty strings' { Convert-ColorTokensToAnsi "" | Should -BeNullOrEmpty }
        it 'Removes any foreground, background and format colors' { Convert-ColorTokensToAnsi "ABC{c:Black}DEF{b:White}KLM{f:Normal}" | Should -BeExactly "ABCDEFKLM" }
        it 'Ignores wrong colors' { Convert-ColorTokensToAnsi "ABC{c:Purple}DEF{b:Purple}KLM{f:Purple}" | Should -BeExactly "ABCDEFKLM" }
    }
}

describe 'Out-AnsiString' {
    Context 'Run with ANSI_Colorization enabled' {
        BeforeEach { $Global:ANSI_Colorization = $true }
        AfterAll { $Global:ANSI_Colorization = $true }

        it 'Accepts empty strings in pipeline' { "" | Out-AnsiString | Should -BeNullOrEmpty }
        it 'Accepts several foreground, background and format colors in pipeline' { "ABC{c:Black}DEF{b:White}KLM{f:Normal}" | Out-AnsiString | Should -BeExactly "ABC$([char]27)[30mDEF$([char]27)[47;1mKLM$([char]27)[0m" }
    }
    Context 'Run with ANSI_Colorization disabled' {
        BeforeEach { $Global:ANSI_Colorization = $false }
        AfterAll { $Global:ANSI_Colorization = $true }

        it 'Accepts empty strings in pipeline' { "" | Out-AnsiString | Should -BeNullOrEmpty }
        it 'Removes any foreground, background and format colors in pipeline' { "ABC{c:Black}DEF{b:White}KLM{f:Normal}" | Out-AnsiString | Should -BeExactly "ABCDEFKLM" }
    }
}