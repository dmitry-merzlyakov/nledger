# Helper functions that manage NLedger build by means of ProductInfo.xml
[CmdletBinding()]
Param()

trap 
{ 
  write-error $_ 
  exit 1 
} 

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

[string]$Script:absSourceCodePath = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..\Source")
if (!(Test-Path -LiteralPath $Script:absSourceCodePath -PathType Container)) { throw "Cannot find folder $Script:absSourceCodePath"}

[string]$Script:absProductInfoPath = [System.IO.Path]::GetFullPath("$Script:absSourceCodePath\ProductInfo.xml")
if (!(Test-Path -LiteralPath $Script:absProductInfoPath -PathType Leaf)) { throw "Cannot find file $Script:absSourceCodePath"}

Write-Verbose "Source code path: $Script:absSourceCodePath"
Write-Verbose "Product info path: $Script:absProductInfoPath"

[xml]$script:productInfoContent = Get-Content -Encoding UTF8 "$Script:absProductInfoPath"

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
    $innerText = (ApplyVar $innerText "Year" (get-date -f yyyy))
    return $innerText
}

function Update-ProjectFiles {
    [CmdletBinding()]
    Param([Switch]$verify = $False)

    Write-Verbose "Update *.csproj files"

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
}

function Update-LicenseNotes {
    [CmdletBinding()]
    Param([Switch]$verify = $False)
    
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
}

function Test-VersionInfoAndLicenseNotes {
    [CmdletBinding()]
    Param()
    $null = Update-ProjectFiles -verify
    $null = Update-LicenseNotes -verify
}

function Get-VersionInfo {
    [CmdletBinding()]
    Param()

    [string]$Private:Version = "$(ApplyGlobals $script:productInfoContent.ProductInfo.General.Version)"
    Write-Verbose "VersionPrefix: $Private:Version"

    Write-Verbose "Build version autogeneration"
    [DateTime]$Private:BasePatchDate = $script:productInfoContent.ProductInfo.BuildVersionAutoGeneration.BasePatchDate
    [int]$Private:BuildNumber = [DateTime]::UtcNow.Subtract($Private:BasePatchDate).TotalMinutes / 20
    Write-Verbose "Build number: $Private:BuildNumber"
    
    [string]$Private:VersionPrefix = "$(ApplyGlobals $script:productInfoContent.ProductInfo.AssemblyInfo.VersionPrefix)"
    Write-Verbose "VersionPrefix: $Private:VersionPrefix"
    
    [string]$Private:VersionSuffix = "$(ApplyGlobals $script:productInfoContent.ProductInfo.AssemblyInfo.VersionSuffix)"
    Write-Verbose "VersionSuffix: $Private:VersionSuffix"
    
    [string]$Private:SourceRevisionId = (git rev-parse --short HEAD)
    Write-Verbose "SourceRevisionId: $Private:SourceRevisionId"

    [PsCustomObject]@{
        VersionPrefix = $Private:VersionPrefix
        SourceRevisionId = $Private:SourceRevisionId
        VersionSuffix = $Private:VersionSuffix  
        BuildNumber = $Private:BuildNumber
        Version = $Private:Version
     }    
}

function Get-VersionInfoArray {
    [CmdletBinding()]
    Param()

    $Private:VersionInfo = Get-VersionInfo

    write-output $Private:VersionInfo.VersionPrefix
    write-output $Private:VersionInfo.SourceRevisionId
    write-output $Private:VersionInfo.VersionSuffix
    write-output $Private:VersionInfo.BuildNumber
    write-output $Private:VersionInfo.Version
}