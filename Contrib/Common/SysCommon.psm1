<#
.SYNOPSIS
SysCommon.psm1 - a module containing common auxiliary procedures.

.NOTES
Author: Dmitry Merzlyakov
Date:   December 14, 2023
#>  

[CmdletBinding()]
Param()

<#
.SYNOPSIS
Ensures that the pipeline output is not empty

.DESCRIPTION
Allows you to build a pipeline with checkpoints that verify that 
previous steps have produced at least one output of the pipeline.
If it detects that the output is empty, an exception is thrown.

.PARAMETER errorMessage
An optional error message to throw if the pipeline output is empty.
If this parameter is omitted, the error message "Expected a non-empty value" will appear.

.PARAMETER val
Pipeline input

.EXAMPLE
Get-Process | Where-Object { $_.ProcessName -eq "ACME" } | Assert-IsNotEmpty -errorMessage "ACME process not found" 

Ensures that a process named ACME is found before moving on to the next steps.
#>
function Assert-IsNotEmpty {
  Param(
      [Parameter()][AllowEmptyString()][string]$errorMessage = "Expected a non-empty value",
      [Parameter(ValueFromPipeline)]$val
  )

  Begin { $hitCounter = 0 }

  Process { $hitCounter++ ; if (!$val) { throw $errorMessage } ; $val }

  End { if (!$hitCounter) { throw $errorMessage } }
}

# Global variable that can contain a default failure recovering function for Assert-CommandCompleted.
# The function receives $_ parameter containing an exception object
$Global:AssertionCommandCompletedFailure = $null

function Assert-CommandCompleted {
  Param(
      [Parameter(Mandatory)][scriptblock]$body,
      [Parameter()][scriptblock]$failureRecovering = $Global:AssertionCommandCompletedFailure
  )

  try { . $body }
  catch {
    Write-Verbose $_
    if ($failureRecovering) { $failureRecovering.InvokeWithContext($null, [psvariable]::new("_", $_))[0] }
    else { Write-Output "[ERROR] $($_.exception.message)" }
  }

}

function Out-YesNo {
  Param([Parameter(ValueFromPipeline)]$val)

  Process { if($val) {"Yes"} else {"No"} }
}

function Out-MessageWhenEmpty {
  Param(
    [Parameter()][AllowEmptyString()][string]$message = "[None]",
    [Parameter(ValueFromPipeline)]$val
  )

  Process { if($val) {$val} else {$message} }
}

function Get-PsEscapedString {
    Param([Parameter(ValueFromPipeline)][string]$str)

    Process { if($str -match "\s|'|""|``") { "'$($str -replace "'","''")'" } else {$str} }
}

function Get-CommandExpression {
    Param(
      [Parameter(Mandatory)][string]$command,
      [Parameter()][string[]]$commandArguments)

    "& $($command | Get-PsEscapedString) $(($commandArguments | Get-PsEscapedString) -join ' ')"
}
