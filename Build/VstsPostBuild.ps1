# This script performs post-build activities on hosted agents like VSTS build environment:
# - it composes an archive with log files
# - uploads both the package and log files to DropBox folder
# - adds a message about CI build to CI log file
# - commits updated CI log file.
#
# Use the following command if you need to enable scripts on your machine:
#   set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)][string]$buildOutputPath,
    [Parameter(Mandatory=$true)][string]$dropboxAccessToken,
    [Parameter(Mandatory=$false)][int]$buildID = $env:BUILD_BUILDID,
    [Parameter(Mandatory=$false)][string]$buildStatus = $env:AGENT_JOBSTATUS,
    [Parameter(Mandatory=$false)][string]$buildQueuedBy = $env:BUILD_QUEUEDBY,
    [Parameter(Mandatory=$false)][string]$buildReason = $env:BUILD_REASON,
    [Parameter(Mandatory=$false)][string]$buildReqFor = $env:BUILD_REQUESTEDFOR,
    [Parameter(Mandatory=$false)][string]$buildReqForEMail = $env:BUILD_REQUESTEDFOREMAIL,
    [Parameter(Mandatory=$false)][string]$buildSourceVersion = $env:BUILD_SOURCEVERSION,
    [Parameter(Mandatory=$false)][string]$ciBuildLogPath = "..\_CI.BuildLog.md"
)

trap 
{ 
  write-error $_ 
  exit 1 
}

if ($buildID -eq 0) { $buildID = 1 }
if (!($buildStatus)) { $buildStatus = "Failed" }
if (!($buildQueuedBy)) { $buildQueuedBy = "\*" }
if (!($buildReason)) { $buildReason = "\*" }
if (!($buildReqFor)) { $buildReqFor = "\*" }
if (!($buildReqForEMail)) { $buildReqForEMail = "\*" }
if (!($buildSourceVersion)) { $buildSourceVersion = "\*" }

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $ScriptPath\ProductInfo.psm1 -Force

[string]$Script:absBuildOutputPath = if (![System.IO.Path]::IsPathRooted($buildOutputPath)) { Resolve-Path (Join-Path $Script:ScriptPath $buildOutputPath) -ErrorAction Stop } else { $buildOutputPath }
if (!(Test-Path $Script:absBuildOutputPath -PathType Container)) { throw "Cannot find build output folder: $Script:absBuildOutputPath" }

[string]$Script:absCIBuildLogPath = if (![System.IO.Path]::IsPathRooted($ciBuildLogPath)) { Resolve-Path (Join-Path $Script:ScriptPath $ciBuildLogPath) -ErrorAction Stop } else { $ciBuildLogPath }
if (!(Test-Path $Script:absCIBuildLogPath -PathType Leaf)) { throw "Cannot CI Build Log file: $Script:absCIBuildLogPath" }


Add-Type -Assembly System.IO.Compression.FileSystem

# Zip specified folder
function ZipFiles {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)] [string]$zipfilename,
        [Parameter(Mandatory = $true)] [string]$sourcedir
    )

   $Private:compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir, $zipfilename, $Private:compressionLevel, $true) | Out-Null
}

# Rename the file; add build ID before an extension
function addBuildID {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)] [string]$fileName,
        [Parameter(Mandatory = $true)] [int]$buildID
    )
    if (Test-Path $fileName -PathType Leaf) {
        $private:item = Get-Item -Path $fileName
        [string]$private:newName = "$($private:item.DirectoryName)\$($private:item.BaseName).Build.$buildID$($private:item.Extension)"
        Rename-Item $private:item -NewName $private:newName -ErrorAction Stop | Out-Null
        $fileName = $private:newName
    }
    return $fileName
}

# Upload to Dropbox; return the new remote file name
function UploadToDropbox {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)] [string]$fileName,
        [Parameter(Mandatory = $true)] [string]$targetPath,
        [Parameter(Mandatory = $true)] [string]$dropboxAccessToken
    )

    Write-Verbose "Uploading to Dropbox"
    Write-Verbose "Local file: $fileName"
    Write-Verbose "Target file: $targetPath"

    #$fileName = $fileName.Replace("[", "``[").Replace("]", "``]")
    if (!(Test-Path $fileName -PathType Leaf)) { return "" }

    [string]$private:arg = '{ "path": "' + $targetPath + '", "mode": "add", "autorename": true, "mute": false }'
    $private:headers = @{ "Authorization"="Bearer $dropboxAccessToken"; "Dropbox-API-Arg"=$private:arg; "Content-Type"='application/octet-stream' }
    $private:result = Invoke-RestMethod -Uri https://content.dropboxapi.com/2/files/upload -Method Post -InFile $fileName -Headers $headers -ErrorAction Stop

    Write-Verbose "Specified target file: $($private:result.path_lower)"
    return $private:result.path_lower
}

# Get Share Link to uploaded file
function GetShareLink {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$targetPath,
        [Parameter(Mandatory = $true)] [string]$dropboxAccessToken
    )

    Write-Verbose "Getting share link:"
    Write-Verbose "Uploaded file: $targetPath"

    if (!($targetPath)) { return "" }

    [string]$private:arg = '{ "path": "' + $targetPath + '", "settings": {} }'
    $private:headers = @{ "Authorization"="Bearer $dropboxAccessToken"; "Content-Type"='application/json' }

    $private:result = Invoke-RestMethod -Uri https://api.dropboxapi.com/2/sharing/create_shared_link_with_settings -Method Post -Headers $headers -Body $private:arg -ErrorAction Stop 

    Write-Verbose "Share link is: $($private:result.url)"
    return $private:result.url
}

# Determine the current product version

$Script:VersionInfo = Get-VersionInfo
[string]$Script:ver = $Script:VersionInfo.Version
[string]$Script:VersionPrefix = $Script:VersionInfo.VersionPrefix
[string]$Script:VersionSuffix = $Script:VersionInfo.VersionSuffix
Write-Verbose "Found product version: '$Script:ver'; version prefix: '$Script:VersionPrefix'; version suffix: '$Script:VersionSuffix'"

# Create a package with build log files

[string]$Script:installPackageZip = "$Script:absBuildOutputPath\NLedger-v$Script:ver.zip"
[string]$Script:installPackageMsi = "$Script:absBuildOutputPath\NLedger-v$Script:ver.msi"
[string]$Script:logsPackageZip = "$Script:absBuildOutputPath\NLedger-BuildLogs-v$Script:ver.zip"
[string]$Script:installPackageNuget = "$Script:absBuildOutputPath\NLedger.$Script:VersionPrefix-$Script:VersionSuffix.nupkg"

Write-Verbose "Create a folder for log files"
[string]$Script:logsFolder = "$Script:absBuildOutputPath\logs"
if (Test-Path $Script:logsFolder -PathType Container) { throw "Invalid build flow: the folder with log files already exists. Clean up the build folder before running the build." }
New-Item -Path $Script:logsFolder -ItemType Directory -ErrorAction Stop | Out-Null
[string]$Script:logFilesFolder = "$Script:logsFolder\Build-$(get-date -f yyyyMMdd-HHmmss)\"
New-Item -Path $Script:logFilesFolder -ItemType Directory -ErrorAction Stop | Out-Null

Write-Verbose "Move log files"
Move-Item "$Script:absBuildOutputPath\*.log" $Script:logFilesFolder
Move-Item "$Script:absBuildOutputPath\*.html" $Script:logFilesFolder
Move-Item "$Script:absBuildOutputPath\*.trx" $Script:logFilesFolder

Write-Verbose "Create zip archive for log files"
ZipFiles -zipfilename $Script:logsPackageZip -sourcedir $Script:logFilesFolder

# Calculate MD5 for zip archives

[string]$Script:installPackageMD5 = if (Test-Path $Script:installPackageZip -PathType Leaf) { (Get-FileHash $Script:installPackageZip -Algorithm MD5).Hash } else { "none" }
[string]$Script:installMsiMD5 = if (Test-Path $Script:installPackageMsi -PathType Leaf) { (Get-FileHash $Script:installPackageMsi -Algorithm MD5).Hash } else { "none" }
[string]$Script:logsPackageMD5 = if (Test-Path $Script:logsPackageZip -PathType Leaf) { (Get-FileHash $Script:logsPackageZip -Algorithm MD5).Hash } else { "none" }
[string]$Script:installNugetMD5 = if (Test-Path $Script:installPackageNuget -PathType Leaf) { (Get-FileHash $Script:installPackageNuget -Algorithm MD5).Hash } else { "none" }
Write-Verbose "MD5: $Script:installPackageMD5 (file $Script:installPackageZip)"
Write-Verbose "MD5: $Script:installMsiMD5 (file $Script:installMsiZip)"
Write-Verbose "MD5: $Script:logsPackageMD5 (file $Script:logsPackageZip)"
Write-Verbose "MD5: $Script:installNugetMD5 (file $Script:installPackageNuget)"

# Rename packages; add build ID

$Script:installPackageZip = AddBuildID $Script:installPackageZip $buildID
$Script:installPackageMsi = AddBuildID $Script:installPackageMsi $buildID
$Script:logsPackageZip = AddBuildID $Script:logsPackageZip $buildID
$Script:installPackageNuget = AddBuildID $Script:installPackageNuget $buildID
Write-Verbose "Msi installation package is renamed as $Script:installPackageMsi"
Write-Verbose "Zip installation package is renamed as $Script:installPackageZip"
Write-Verbose "Zip logs package is renamed as $Script:logsPackageZip"
Write-Verbose "Nuget package is renamed as $Script:installPackageNuget"

# Upload to Dropbox

[string]$Script:targetInstallPackageZip = "/NLedger/$([System.IO.Path]::GetFileName($Script:installPackageZip))"
[string]$Script:targetInstallPackageMsi = "/NLedger/$([System.IO.Path]::GetFileName($Script:installPackageMsi))"
[string]$Script:targetlogsPackageZip = "/NLedger/$([System.IO.Path]::GetFileName($Script:logsPackageZip))"
[string]$Script:targetInstallPackageNuget = "/NLedger/$([System.IO.Path]::GetFileName($Script:installPackageNuget))"
Write-Verbose "Msi installation package - target DropBox name is $Script:targetInstallPackageMsi"
Write-Verbose "Zip installation package - target DropBox name is $Script:targetInstallPackageZip"
Write-Verbose "Zip logs package - target DropBox name is $Script:targetlogsPackageZip"
Write-Verbose "Nuget package - target DropBox name is $Script:targetInstallPackageNuget"
$Script:targetInstallPackageMsi = UploadToDropbox -fileName $Script:installPackageMsi -targetPath $Script:targetInstallPackageMsi -dropboxAccessToken $dropboxAccessToken
$Script:targetInstallPackageZip = UploadToDropbox -fileName $Script:installPackageZip -targetPath $Script:targetInstallPackageZip -dropboxAccessToken $dropboxAccessToken
$Script:targetlogsPackageZip = UploadToDropbox -fileName $Script:logsPackageZip -targetPath $Script:targetlogsPackageZip -dropboxAccessToken $dropboxAccessToken
$Script:targetInstallPackageNuget = UploadToDropbox -fileName $Script:installPackageNuget -targetPath $Script:targetInstallPackageNuget -dropboxAccessToken $dropboxAccessToken
Write-Verbose "Msi installation package - uploaded target DropBox name is $Script:targetInstallPackageMsi"
Write-Verbose "Zip installation package - uploaded target DropBox name is $Script:targetInstallPackageZip"
Write-Verbose "Zip logs package - uploaded target DropBox name is $Script:targetlogsPackageZip"
Write-Verbose "Nuget package - uploaded target DropBox name is $Script:targetInstallPackageNuget"

# Get share links for uploaded files

[string]$Script:targetInstallMsiLink = GetShareLink -targetPath $Script:targetInstallPackageMsi -dropboxAccessToken $dropboxAccessToken
[string]$Script:targetInstallPackageLink = GetShareLink -targetPath $Script:targetInstallPackageZip -dropboxAccessToken $dropboxAccessToken
[string]$Script:targetlogsPackageLink = GetShareLink -targetPath $Script:targetlogsPackageZip -dropboxAccessToken $dropboxAccessToken
[string]$Script:targetInstallNugetLink = GetShareLink -targetPath $Script:targetInstallPackageNuget -dropboxAccessToken $dropboxAccessToken
Write-Verbose "Msi installation package link is $Script:targetInstallMsiLink"
Write-Verbose "Zip installation package link is $Script:targetInstallPackageLink"
Write-Verbose "Zip logs package link is $Script:targetlogsPackageLink"
Write-Verbose "Nuget package link is $Script:targetInstallNugetLink"

# Create CI Log content

[string]$Script:statusColor = if ($buildStatus -eq "Succeeded") { "green" } else { "red" }
$Script:buildDate = (get-date -f "yyyy/MM/dd HH:mm:ss")

[string]$Script:buildLogLink = if (!($Script:targetlogsPackageLink)) { "Not created" } else { "[$([System.IO.Path]::GetFileName($Script:logsPackageZip))]($Script:targetlogsPackageLink) MD5: $Script:logsPackageMD5" } 
[string]$Script:buildPackageLink = if (!($Script:targetInstallPackageLink)) { "Not created" } else { "[$([System.IO.Path]::GetFileName($Script:installPackageZip))]($Script:targetInstallPackageLink) MD5: $Script:installPackageMD5" } 
[string]$Script:buildMsiLink = if (!($Script:targetInstallMsiLink)) { "Not created" } else { "[$([System.IO.Path]::GetFileName($Script:installPackageMsi))]($Script:targetInstallMsiLink) MD5: $Script:installMsiMD5" } 
[string]$Script:buildNugetLink = if (!($Script:targetInstallNugetLink)) { "Not created" } else { "[$([System.IO.Path]::GetFileName($Script:installPackageNuget))]($Script:targetInstallNugetLink) MD5: $Script:installNugetMD5" } 

[string]$Script:logRecord = "***`r`n"
$Script:logRecord += "#### ![$buildStatus](https://img.shields.io/badge/Build-$buildStatus-$Script:statusColor.svg) Build [$buildID] $Script:buildDate`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += ">Build #$buildID; Status:$buildStatus`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += ">Queued by $buildQueuedBy; Reason:$buildReason`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += ">Requested for $buildReqFor (Email:$buildReqForEMail)`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += ">Latest commit: $buildSourceVersion`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Build logs: $Script:buildLogLink`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Install package: $Script:buildPackageLink`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "MSI package: $Script:buildMsiLink`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Nuget package: $Script:buildNugetLink`r`n"
Write-Verbose "Prepared a build log record:==="
Write-Verbose $Script:logRecord
Write-Verbose "==============================="

# Update CI Log file

[string]$Script:ciBuildLogContent = Get-Content -Path $Script:absCIBuildLogPath | Out-String
[int]$script:pos = $Script:ciBuildLogContent.IndexOf("***")
$Script:ciBuildLogContent = if ($script:pos -ge 0) { $Script:ciBuildLogContent.Insert($script:pos,$Script:logRecord) } else { $Script:ciBuildLogContent + $Script:logRecord }
Set-Content -Path $Script:absCIBuildLogPath $Script:ciBuildLogContent -ErrorAction Stop | Out-Null
Write-Verbose "Build log file is updated: $Script:absCIBuildLogPath"

# Commit updated CI Log file

[string]$Script:commitComment = "Build #$buildID is $buildStatus;***NO_CI***"
Write-Verbose "Setting the username and email for a single repository"
& git config user.email "dmitry.merzlyakov@gmail.com" 2>&1 | Write-Host
& git config --global user.name "dmitry-merzlyakov" 2>&1 | Write-Host
Write-Verbose "Committing updated log file: git commit -m $Script:commitComment $Script:absCIBuildLogPath"
& git commit -m $Script:commitComment $Script:absCIBuildLogPath 2>&1 | Write-Host
Write-Verbose "Pushing updated log file: git push origin HEAD:next-dev"
& git push origin HEAD:next-dev 2>&1 | Write-Host
Write-Verbose "Everything is done"
