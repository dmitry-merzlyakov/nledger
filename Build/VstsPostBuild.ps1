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
[string]$Script:installPackagePythonNuget = "$Script:absBuildOutputPath\NLedger.Extensibility.Python.$Script:VersionPrefix-$Script:VersionSuffix.nupkg"
[string]$Script:installPackagePythonWheel = "$Script:absBuildOutputPath\ledger-$Script:VersionPrefix-py3-none-any.whl"

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

function Publish-OnDropbox {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory=$True)][string]$fileName,
      [Parameter(Mandatory=$True)][string]$buildID,
      [Parameter(Mandatory=$True)][string]$dropboxAccessToken
    )

    Write-Verbose "Publish on Dropbox: Processing file $fileName (build ID $buildID)"

    [string]$Private:fileMD5 = if (Test-Path $fileName -PathType Leaf) { (Get-FileHash $fileName -Algorithm MD5).Hash } else { "none" }
    Write-Verbose "Calculated MD5: $Private:fileMD5"

    [string]$Private:targetName = "/NLedger/Build_$buildID/$([System.IO.Path]::GetFileName($fileName))"
    Write-Verbose "Target DropBox name is $Private:targetName"

    [string]$Private:targetName = UploadToDropbox -fileName $fileName -targetPath $Private:targetName -dropboxAccessToken $dropboxAccessToken
    Write-Verbose "Uploaded target DropBox name is $Private:targetName"

    [string]$Private:targetLink = GetShareLink -targetPath $Private:targetName -dropboxAccessToken $dropboxAccessToken
    Write-Verbose "Shared Dropbox link is $Private:targetLink"

    [string]$Private:formattedLink = if (!($Private:targetLink)) { "Not created" } else { "[$([System.IO.Path]::GetFileName($fileName))]($Private:targetLink) MD5: $Private:fileMD5" } 
    
    return $Private:formattedLink
}

# Upload to Dropbox and get result links

$Script:uploadedFiles = @{}
@($Script:installPackageZip, $Script:installPackageMsi, $Script:logsPackageZip, $Script:installPackageNuget, $Script:installPackagePythonNuget, $Script:installPackagePythonWheel) | 
  ForEach-Object { $Script:uploadedFiles[$_] = Publish-OnDropbox -fileName $_ -buildID $buildID -dropboxAccessToken $dropboxAccessToken }

# Create CI Log content

[string]$Script:statusColor = if ($buildStatus -eq "Succeeded") { "green" } else { "red" }
$Script:buildDate = (get-date -f "yyyy/MM/dd HH:mm:ss")

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
$Script:logRecord += "Build logs: $Script:uploadedFiles[$Script:logsPackageZip]`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Install package: $Script:uploadedFiles[$Script:installPackageZip]`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "MSI package: $Script:uploadedFiles[$Script:installPackageMsi]`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Nuget package: $Script:uploadedFiles[$Script:installPackageNuget]`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Nuget Python package: $Script:uploadedFiles[$Script:installPackagePythonNuget]`r`n"
$Script:logRecord += "`r`n"
$Script:logRecord += "Wheel Python package: $Script:uploadedFiles[$Script:installPackagePythonWheel]`r`n"
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
