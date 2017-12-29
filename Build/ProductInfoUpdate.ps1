# This script updates the source code files with the latest product information (product name, version, licensing).
# It takes this information from ProductInfo.xml and updates all the source code files in case of any changes.
# Use the following command if you need to enable scripts on your machine:
#   set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)][string]$sourceCodePath = "..\Source\",
    [Parameter(Mandatory=$False)][string]$productInfoPath = "ProductInfo.xml",
    [Switch]$verify = $False
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

# Update all assembly info files

Write-Verbose "Update AssemblyInfo.cs files"

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

foreach ($Private:asmInfo in (Get-ChildItem -Path $Script:absSourceCodePath -Filter AssemblyInfo.cs -Recurse -ErrorAction Stop -Force | %{$_.FullName})) {
    Write-Verbose "Processing $Private:asmInfo"
    $Private:asmInfoContent = (Get-Content $Private:asmInfo | Out-String ).Trim()
    [bool]$Private:HasChanges = $False
    foreach($Private:infoAttr in $script:productInfoContent.SelectNodes("/ProductInfo/AssemblyInfo/*")) {
        [string]$Private:regex = "\n(\[assembly:\s*$($Private:infoAttr.Name)\(\"")([^""]*)(\""\)\])"
        Write-Verbose "Try regex '$Private:regex'"
        if ($Private:infoAttr.remove) {
            Write-Verbose "Removing attribute '$($Private:infoAttr.Name)'"
            if ($Private:infoAttr.InnerText) { throw "Inner text is not allowed for an attribute to be removed" }
            if ($Private:asmInfoContent -match $Private:regex) {
			    $Private:asmInfoContent = $Private:asmInfoContent -replace $Private:regex, "`n"
			    $Private:HasChanges = $True
            }
        }
        else {
	        [string]$Private:replacementText = ApplyGlobals $Private:infoAttr.InnerText
            Write-Verbose "Process attribute '$($Private:infoAttr.Name)' with value '$Private:replacementText'"
            if ($Private:asmInfoContent -match $Private:regex) {
			    if ($Matches[2] -eq $Private:replacementText) {
				    Write-Verbose "The value of the found attribute is the same as in ProductInfo.xml; no changes."
			    }
			    else {
				    Write-Verbose "The value '$($Matches[2])' of the found attribute does not match ProductInfo.xml; updating..."
				    $Private:asmInfoContent = $Private:asmInfoContent -replace $Private:regex, "$($Matches[1])$($Private:replacementText)$($Matches[3])"
				    $Private:HasChanges = $True
			    }
            } else {
                Write-Verbose "No matches found; adding the attribute"
                $Private:asmInfoContent += "`r`n[assembly: $($Private:infoAttr.Name)(""$($($Private:replacementText))"")]"
                $Private:HasChanges = $True
            }
        }
    }
    if ($Private:HasChanges) {
        if ($verify) { throw "Verification fails. File $Private:asmInfo needs to be updated." }        
        ($Private:asmInfoContent -join "`r`n") | Set-Content -Encoding UTF8 -Path $Private:asmInfo -ErrorAction Stop
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

Write-Verbose "Done.";




