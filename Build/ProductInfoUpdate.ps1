# This script updates the source code files with the latest product information (product name, version, licensing).
# It takes this information from ProductInfo.xml and updates all the source code files in case of any changes.
# Use the following command if you need to enable scripts on your machine:
#   set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$sourceCodePath = "..\Source\",
    [Parameter(Mandatory=$False)][string]$productInfoPath = "ProductInfo.xml",
    [Switch]$verify = $False,
    [Switch]$outversion = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
[string]$Script:absSourceCodePath = Resolve-Path (Join-Path $Script:ScriptPath $sourceCodePath) -ErrorAction Stop
[string]$Script:absProductInfoPath = Resolve-Path (Join-Path $Script:absSourceCodePath $productInfoPath) -ErrorAction Stop

Write-Verbose "Source code path: $Script:absSourceCodePath"
Write-Verbose "Product info path: $Script:absProductInfoPath"

[xml]$script:productInfoContent = Get-Content "$Script:absProductInfoPath"

# Update all project files

Write-Verbose "Update *.csproj files"

function ApplyVar {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)] [string]$text,
        [Parameter(Mandatory=$True)] [string]$variable,
        [Parameter(Mandatory=$True)] [string]$value
    )
    Write-Verbose "Applying variable '$variable' with value '$value' for text '$text'"
    [string]$Private:regex = "\{\{$variable\}\}"
    Write-Verbose "Trying regex '$Private:regex'"
    if ($text -match [string]$Private:regex) { $text = $text -replace [string]$Private:regex,$value }
    Write-Verbose "Result text '$text'"
    return $text
}

function ApplyGlobals {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)] [string]$innerText
    )
    foreach($Private:globalAttr in $script:productInfoContent.SelectNodes("/ProductInfo/General/*")) { $innerText = (ApplyVar $innerText $Private:globalAttr.Name $Private:globalAttr.InnerText) }
    $innerText = (ApplyVar $innerText "Year" (get-date –f yyyy))
    return $innerText
}

foreach ($Private:asmInfo in (Get-ChildItem -Path $Script:absSourceCodePath -Filter *.csproj -Recurse -ErrorAction Stop -Force | %{$_.FullName})) {
    Write-Verbose "Processing $Private:asmInfo"
    [xml]$Private:asmInfoContent = (Get-Content $Private:asmInfo)
    [bool]$Private:HasChanges = $False
    foreach($Private:infoAttr in $script:productInfoContent.SelectNodes("/ProductInfo/AssemblyInfo/*")) {
        [string]$Private:replacementText = ApplyGlobals $Private:infoAttr.InnerText
        Write-Verbose "Process attribute '$($Private:infoAttr.Name)' with value '$Private:replacementText'"
        $Private:asmInfoAttr = $Private:asmInfoContent.SelectNodes("/Project/PropertyGroup/$($Private:infoAttr.Name)")
        if (!$Private:asmInfoAttr) { throw "Cannot find element /Project/PropertyGroup/$($Private:infoAttr.Name) in file $Private:asmInfo"}
        if ($Private:asmInfoAttr.InnerText -ne $Private:replacementText) {
            Write-Verbose "Detected differences; updating..."           
            $Private:asmInfoAttr.InnerText = $Private:replacementText
            $Private:HasChanges = $True
        } else {
            Write-Verbose "The value of the found attribute is the same as in ProductInfo.xml; no changes."
        }
    }
    if ($Private:HasChanges) {
        if ($verify) { throw "Verification fails. File $Private:asmInfo needs to be updated." }        
        $Private:asmInfoContent | Set-Content -Path $Private:asmInfo -ErrorAction Stop
        Write-Output "File $Private:asmInfo has been updated with recent product info."
    } else {
        Write-Verbose "File $Private:asmInfo has the latest product info; no changes."
    }
}

# Update copyright and license notes

Write-Verbose "Update copyright and license notes in all files"

[string]$headerText = ($script:productInfoContent.ProductInfo.SourceLicensing.FileHeader.Trim() -replace "(\n|\r)+", "{{CR}}") -replace "\s*\/\/","//"
$headerText = ApplyGlobals $headerText
$headerText = $headerText -replace "\{\{CR\}\}", "`r`n"

foreach ($Private:csFile in (Get-ChildItem -Path $Script:absSourceCodePath -Filter *.cs -Recurse -ErrorAction Stop -Force | ? { $_.FullName -notmatch "\\obj\\" } | %{$_.FullName})) {
    Write-Verbose "Processing $Private:csFile"
    $Private:csContent = (Get-Content $Private:csFile | Out-String ).Trim()
    if (!$Private:csContent.StartsWith($headerText)) {
        [int]$pos = $Private:csContent.IndexOf("using");
        if ($pos -gt 0) { $Private:csContent = $Private:csContent.Substring($pos) }
        $Private:csContent = $headerText + "`r`n" + $Private:csContent
        Write-Output "File $Private:csFile is updated with actual copyright notices."
        if ($verify) { throw "Verification fails. File $Private:csFile needs to be updated." }        
        $Private:csContent | Set-Content -Encoding UTF8 -Path $Private:csFile -ErrorAction Stop
    } else {
        Write-Verbose "File $Private:csFile already has actual copyright notices. No changes"
    }
}

# Prepare version auto-generation attributes

Write-Verbose "Build version autogeneration"

[DateTime]$Script:BasePatchDate = $script:productInfoContent.ProductInfo.BuildVersionAutoGeneration.BasePatchDate
[int]$Script:BuildNumber = [DateTime]::UtcNow.Subtract($Script:BasePatchDate).TotalMinutes / 20
Write-Verbose "Build number: $Script:BuildNumber"

[string]$Script:VersionPrefix = "$(ApplyGlobals $script:productInfoContent.ProductInfo.AssemblyInfo.VersionPrefix)"
Write-Verbose "VersionPrefix: $Script:VersionPrefix"

[string]$Script:VersionSuffix = "$(ApplyGlobals $script:productInfoContent.ProductInfo.AssemblyInfo.VersionSuffix)"
Write-Verbose "VersionSuffix: $Script:VersionSuffix"

[string]$Script:SourceRevisionId = (git rev-parse --short HEAD)
Write-Verbose "SourceRevisionId: $Script:SourceRevisionId"

if ($outversion) {
    write-output $Script:VersionPrefix
    write-output $Script:SourceRevisionId
    write-output $Script:VersionSuffix
    write-output $Script:BuildNumber
}


Write-Verbose "Done."