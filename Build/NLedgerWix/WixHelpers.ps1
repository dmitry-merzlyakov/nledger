# This script contains function that help to manage Wix configuration
# If it is run with -verify switch, the function ValidateDirectoryContent will be called.
# In this case, the parameter "packageFolder" must be specified too.

# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
  [Parameter(Mandatory=$False)][string]$packageFolder,
  [Switch]$verify = $False
)

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path

trap 
{ 
  write-error $_ 
  exit 1 
} 

# Just for manual running when you do not want to specify "packageFolder" every time.
if (!$packageFolder) {
     throw "Comment it out in caase of manual run"
     $packageFolder = Get-ChildItem -LiteralPath "$Script:ScriptPath\..\..\" | ?{ $_.Name -match "Build\-" } | %{ $_.Name } | Sort-Object -Descending | Select-Object -first 1
     $packageFolder = [System.IO.Path]::GetFullPath("$Script:ScriptPath\..\..\$packageFolder\package")
     Write-Verbose "Package folder is set to $packageFolder"
}

# Observes the content of a given folder and returns the list files for wix configuration
function ComposeFileItemForSubFolder {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)][string]$sourcePath,
    [Parameter(Mandatory=$False)][string]$prefix = '$(var.PackageFolder)'
  )

  [string]$private:localPath = [System.IO.Path]::GetFullPath("$packageFolder\$sourcePath")
  if (!(Test-Path -LiteralPath $private:localPath -PathType Container)) { throw "Path not found" }

  $private:subFolder = $private:localPath.Substring($packageFolder.Length)

  foreach($private:file in Get-ChildItem $private:localPath) {
    $private:normalizedName = $private:file.Name.Replace("-","_").Replace(".","_")
    Write-Output "<File Id=""File_$private:normalizedName"" Name=""$($private:file.Name)"" Source=""$($prefix)$($private:subFolder)\$($private:file.Name)""/>"    
  }
}

# Wix Components Validation Exception
$Script:WixComponentValidationExceptions = @(
  "NLedger\LICENSE.RTF",
  "NLedger\NLManagement\NonDeliverable\"
)

# Validates the content of wix components
# It verifies that every File element has consistent Name and Source values (file names must be equal)
# and all the files in a package folder are referenced in Wix configuration (Components.wxs) or added to
# the list of exceptions ($Script:WixComponentValidationExceptions)
function ValidateDirectoryContent {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$False)][string]$packageFolder = $packageFolder,
    [Parameter(Mandatory=$False)][string]$componentsFileName = "$Script:ScriptPath\Components.wxs",
    [Parameter(Mandatory=$False)][string]$prefix = '$(var.PackageFolder)'
  )

  if (!(Test-Path -LiteralPath $packageFolder -PathType Container)) { throw "Package folder not found" }
  if (!(Test-Path -LiteralPath $componentsFileName -PathType Leaf)) { throw "Wix component file not found" }

  $packageFolder = [System.IO.Path]::GetFullPath($packageFolder)
  if (!$packageFolder.Trim().EndsWith("\")) { $packageFolder += "\" }

  [xml]$private:componentsXml = Get-Content -Encoding UTF8 -LiteralPath $componentsFileName
  $private:ProgramFilesFolder = $private:componentsXml.Wix.Fragment.DirectoryRef.Directory
  if ($private:ProgramFilesFolder.Id -ne "ProgramFilesFolder") { throw "Wix component file does not have a root Program Files directory" }

  # Collect wix files

  $private:wixFiles = @()
  foreach ($private:xmlFile in $private:ProgramFilesFolder.GetElementsByTagName("File")) {
    $private:fileName = $private:xmlFile.Name
    $private:sourceName = $private:xmlFile.Source.Substring($prefix.Length)
    $private:sourceFileName = [System.IO.Path]::GetFileName($private:sourceName)
    
    # Validation: File names must be equal
    if ($private:fileName -ne $private:sourceFileName) { throw "Not equal file names in Wix: $private:fileName vs $private:sourceFileName" }

    $private:wixFiles += $private:sourceName
  }

  # Collect package files

  $private:packageFiles = Get-ChildItem -LiteralPath $Script:PackageFolder -Recurse | ?{ !$_.PSIsContainer } | %{ $_.FullName.Substring($packageFolder.Length) }

  # Compare file sets

  $private:missedFiles = @()
  foreach($private:packageFileName in $private:packageFiles) {

     # Process validation exceptions
     if ($Script:WixComponentValidationExceptions | ?{ $private:packageFileName.StartsWith($_) }) { continue }

     if (!$private:wixFiles.Contains($private:packageFileName)) { 
        $private:missedFiles += $private:packageFileName
     }
  }

  # Validation: all package files must be in Wix configuration or in the exception list
  if ($private:missedFiles) { throw "Some package files are not in Wix configuration: $private:missedFiles" }
}


#Example how to use ComposeFileItemForSubFolder
#ComposeFileItemForSubFolder "NLedger\Install"

if ($verify) { ValidateDirectoryContent -packageFolder $packageFolder }