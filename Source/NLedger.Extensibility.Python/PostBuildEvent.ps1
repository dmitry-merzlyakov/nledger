[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$ProjectDir,
    [Parameter(Mandatory=$True)][string]$TargetPath
)

write-host "PostBuildEvent.ps1 is executing"
write-host $ProjectDir
write-host $TargetPath