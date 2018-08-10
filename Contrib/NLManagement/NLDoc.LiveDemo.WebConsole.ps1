# If you run this script in ISE, you may want to configure Execution Policy by this command:
# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

$ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $ScriptPath\NLCommon.psm1 -Force
Import-Module $ScriptPath\PsHttpListener.psm1 -Force
Import-Module $ScriptPath\NLDoc.LiveDemo.psm1 -Force

trap 
{ 
  write-output $_ 
  exit 1 
}

$Script:actions = @(
    @{ pattern="\/content"; action=[scriptblock]{ getContent $demoConfig.DemoFile } },
    @{ pattern="\/shutdown"; action=[scriptblock]{ getShutdown } },
    @{ pattern="\/act\.actRun\.(.+)"; action=[scriptblock]{ actRun -testid $Global:ActionParams[1] -demoConfig $demoConfig } }
    @{ pattern="\/act\.actEdit\.(.+)"; action=[scriptblock]{ actEdit -testid $Global:ActionParams[1] -demoConfig $demoConfig } }
    @{ pattern="\/act\.actOpen\.(.+)"; action=[scriptblock]{ actOpen -testid $Global:ActionParams[1] -demoConfig $demoConfig } }
    @{ pattern="\/act\.actRevert\.(.+)"; action=[scriptblock]{ actRevert -testid $Global:ActionParams[1] -demoConfig $demoConfig } }
)

Write-Console "{c:white}Welcome to NLedger Live Demo"
Write-Console "{c:gray}This console executes live examples that you can run in a browser."

$demoConfig = getDemoConfig

Write-Console "{c:gray}The browser with demo content is opening automatically."
Write-Console "{c:gray}You can also open demo manually by link: {c:yellow}$($demoConfig.DemoURL)content{c:gray}."
Write-Console "{c:gray}When you close the demo page, this process shuts down."
Write-Console "{c:gray}You can stop this console at any moment by pressing {c:yellow}CTRL-C{c:gray}."

initDemoSandbox -files $demoConfig.Files -sandbox $demoConfig.Sandbox | Out-Null
RunListener -actions $Script:actions -prefix $demoConfig.DemoURL -onListenerStart { openPage $demoConfig | Out-Null }

