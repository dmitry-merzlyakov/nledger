<#
.SYNOPSIS
Powershell functions for managing local Python deployments

.DESCRIPTION
Provides a collection of functions:
- Python-specific actions: searhing and discovering Python deployment details; managing Python modules by means of Pip;
- Managing Embedded Python deployments;
- Auxiliary functions to support integration custom software with Python.

.NOTES
Available on Windows, Linux and OSX platforms
#>

[bool]$Script:isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
[string]$Script:localAppData = [Environment]::GetFolderPath("LocalApplicationData")

function Search-PyExecutable {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$False)][string]$pyHome)

    if ($pyHome -and !$pyHome.TrimEnd().EndsWith([System.IO.Path]::DirectorySeparatorChar)) { $pyHome = $pyHome + [System.IO.Path]::DirectorySeparatorChar}
    Write-Verbose "Search Python executable; default pyHome: $pyHome"

    if ($Script:isWindowsPlatform) {
        $Env:Path = $Env:Path -replace '\\AppData\\Local\\Microsoft\\WindowsApps',''
        Write-Verbose "Windows platform: fixing unintended opening Windows Store by typing 'python'. Updated PATH variable: $($Env:Path)"
    }

    try { $result = $(& "$($pyHome)python3" -c "import sys;print(sys.executable)") 2>&1 } catch { }    
    if (!($result)) {
        Write-Verbose "'python3' not found; trying 'python' (returned exception: $result)"
        try { $result = $(& "$($pyHome)python" -c "import sys;print(sys.executable)") 2>&1 } catch { }
        if (!($result)) {
            Write-Verbose "'python' not found; python executable not found on this system. (returned exception: $result)"
            return ""
        }
    }

    Write-Verbose "Found python executable: $result"
    return [string]$result
}

function Get-PyVersion {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    [string]$Private:result = $(& "$pyExecutable" "--version") 2>&1
    Write-Verbose "Python returned: $Private:result"
    if ($Private:result -match "Python\s(?<ver>\d+\.\d+\.\d+)") { $Matches["ver"] } else {""}
}

function Get-PyExpandedVersion {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if($(Get-PyVersion -pyExecutable $pyExecutable) -match '(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)'){
        [System.Version]::new($Matches["major"],$Matches["minor"],$Matches["patch"],0)
    } else {$null}
}

function Get-PyPlatform {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    [string]$Private:result = $(& "$pyExecutable" '-c' '"import sys;print(sys.maxsize > 2**32)"') 2>&1
    Write-Verbose "Python returned: $Private:result"
    return $(if($Private:result -eq "True"){"x64"}else{"x86"})
}

function Get-PipVersion {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    [string]$Private:result = $(& "$pyExecutable" "-m" "pip" "--version") 2>&1
    Write-Verbose "Python returned: $Private:result"
    if ($Private:result -match "pip\s(?<pipver>\d+\.\d+(\.\d+)?)\sfrom") { $Matches["pipver"] } else { "" }
}

function Test-PyModuleInstalled {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pyExecutable,
        [Parameter(Mandatory=$True)][string]$pyModule
    )

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    [string]$Private:result = $( $(& "$pyExecutable" "-m" "pip" "show" $pyModule) 2>&1 | Out-String )
    Write-Verbose "Python returned: $Private:result"

    if ($Private:result.StartsWith("Name")) {
        $Private:packageInfo = [PSCustomObject]@{
            Name = $(if($Private:result -match "^Name:\s*(?<name>.*)\n") { $Matches["name"]} else {""})
            Version = $(if($Private:result -match "\nVersion:\s*(?<ver>.*)\n") { $Matches["ver"]} else {""})
            Summary = $(if($Private:result -match "\nSummary:\s*(?<sum>.*)\n") { $Matches["sum"]} else {""})
            HomePage = $(if($Private:result -match "\nHome-page:\s*(?<hom>.*)\n") { $Matches["hom"]} else {""})
            Author = $(if($Private:result -match "\nAuthor:\s*(?<auth>.*)\n") { $Matches["auth"]} else {""})
            AuthorEmail = $(if($Private:result -match "\nAuthor-email:\s*(?<email>.*)\n") { $Matches["email"]} else {""})
            License = $(if($Private:result -match "\nLicense:\s*(?<lic>.*)\n") { $Matches["lic"]} else {""})
            Location = $(if($Private:result -match "\nLocation:\s*(?<loc>.*)\n") { $Matches["loc"]} else {""})
            Requires = $(if($Private:result -match "\nRequires:(?<req>.*)\n") { $Matches["req"]} else {""})
            RequiredBy = $(if($Private:result -match "\nRequired-by:\s*(?<rby>.*)$") { $Matches["rby"]} else {""})
            ExpandedVersion = $null
        }

        if($Private:packageInfo.Version -match "^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)") {
            $Private:packageInfo.ExpandedVersion = [System.Version]::new($Matches["major"],$Matches["minor"],$Matches["patch"],0)
        } else {throw "Incorrect package version $($pyNetModuleInfo.Version)"}    
    
        Write-Verbose "Module info: $Private:packageInfo"
        return $Private:packageInfo
    }

    Write-Verbose "Module $pyModule not installed"
    return $null
}

function Install-PyModule {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][string]$pyExecutable,
        [Parameter(Mandatory)][string]$pyModule,
        [Parameter()][version]$version,
        [Switch]$pre = $False
    )

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}

    $verString = if($version){ "==$([string]$version)" }
    [string]$Private:preFlag = $(if($pre){"--pre"}else{""})

    Write-Verbose "Installing Python module: $pyModule"
    [string]$Private:result = $( $(& "$pyExecutable" "-m" "pip" "install" $pyModule$verString $Private:preFlag) 2>&1 | Out-String )
    Write-Verbose "Python returned: $Private:result"
}

function Uninstall-PyModule {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$pyExecutable,
        [Parameter(Mandatory=$True)][string]$pyModule
    )

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    if (Test-PyModuleInstalled -pyExecutable $pyExecutable -pyModule $pyModule) {
        Write-Verbose "Uninstalling Python module: $pyModule"
        [string]$Private:result = $( $(& "$pyExecutable" "-m" "pip" "uninstall" $pyModule "-y") 2>&1 | Out-String )
        Write-Verbose "Python returned: $Private:result"
        if (Test-PyModuleInstalled -pyExecutable $pyExecutable -pyModule $pyModule) {throw "Cannot uninstall module $pyModule ($pyExecutable)"}
    }
}

function Get-PyPath {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    Write-Verbose "Getting site-packages folder"
    [string]$Private:result = $( $(& "$pyExecutable" "-c" "import sys;print(sys.path)") 2>&1 | Out-String )
    Write-Verbose "Python returned: $Private:result"
    return $($Private:result -split ',' | ForEach-Object{ $_.trim().trim("[").trim("]").trim("'").trim().Replace("\\", "\") })
}

function Get-PyDll {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    if (!(Test-Path -LiteralPath $pyExecutable -PathType Leaf)) { throw "Python executable not found: $pyExecutable"}
    Write-Verbose "Ensuring that 'find-libpython' module is installed"
    if(!(Test-PyModuleInstalled -pyExecutable $pyExecutable -pyModule 'find-libpython')) { $null = Install-PyModule -pyExecutable $pyExecutable -pyModule 'find-libpython' }

    [string]$Private:result = $( $(& "$pyExecutable" "-c" "from find_libpython import find_libpython;print(find_libpython())") 2>&1 | Out-String )
    Write-Verbose "Python returned: $Private:result"

    return $Private:result.Trim()
}

function Get-PySitePackages {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyExecutable)

    return $(Get-PyPath -pyExecutable $pyExecutable | Where-Object{ $_ -match "site-packages" } | Out-String ).Trim()
}

function Get-PyEmbedPackage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform
    )

    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}

    [string]$Private:localRepository = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $(Join-Path $appPrefix "PyEmbedPackages")))
    if (!(Test-Path -LiteralPath $Private:localRepository -PathType Container)) {
        Write-Verbose "Folder with local packages does not exist '$Private:localRepository', creating..."
        $null = New-Item -ItemType Directory -Force -Path $Private:localRepository
        if (!(Test-Path -LiteralPath $Private:localRepository -PathType Container)) { throw "Cannot create folder '$Private:localRepository'" }
    }
    Write-Verbose "Folder with local packages: $Private:localRepository"

    [string]$Private:pyPackageName = "python-$pyVersion-embed-$($pyPlatform).zip"
    Write-Verbose "Package name: $Private:pyPackageName"

    [string]$Private:fullPackageName = [System.IO.Path]::GetFullPath($(Join-Path $Private:localRepository $Private:pyPackageName))
    if (!(Test-Path -LiteralPath $Private:fullPackageName -PathType Leaf)) {
        Write-Verbose "Local package '$Private:fullPackageName' not found; downloading..."

        [string]$Private:tempFileName = [System.IO.Path]::GetTempFileName()
        Write-Verbose "Temp file name: $Private:tempFileName"

        [string]$private:sourceURL = "https://www.python.org/ftp/python/$pyVersion/$Private:pyPackageName"
        Write-Verbose "Source URL: $private:sourceURL"

        try
        {
            $null = Invoke-WebRequest -Uri $private:sourceURL -OutFile $Private:tempFileName
            Move-Item -LiteralPath $Private:tempFileName -Destination $Private:fullPackageName
        }
        catch
        {
            Write-Verbose "Cannot download file: $_"
            Remove-Item -LiteralPath $Private:tempFileName
        }

        if (!(Test-Path -LiteralPath $Private:fullPackageName -PathType Leaf)) { throw "Cannot download package $Private:fullPackageName from $private:sourceURL" }
    }

    Write-Verbose "Full path to Python package: $Private:fullPackageName"
    return $Private:fullPackageName
}

function Remove-PyEmbedPackage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform
    )

    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}

    [string]$Private:localAppFolder = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $appPrefix))
    [string]$Private:localRepository = [System.IO.Path]::GetFullPath($(Join-Path $Private:localAppFolder "PyEmbedPackages"))

    if (Test-Path -LiteralPath $Private:localAppFolder -PathType Container) {
        if (Test-Path -LiteralPath $Private:localRepository -PathType Container) {

            Write-Verbose "Folder with local packages: $Private:localRepository"

            [string]$Private:pyPackageName = "python-$pyVersion-embed-$($pyPlatform).zip"
            Write-Verbose "Package name: $Private:pyPackageName"
        
            [string]$Private:fullPackageName = [System.IO.Path]::GetFullPath($(Join-Path $Private:localRepository $Private:pyPackageName))
            if (Test-Path -LiteralPath $Private:fullPackageName -PathType Leaf) {
                $null = Remove-Item -LiteralPath $Private:fullPackageName
                if (Test-Path -LiteralPath $Private:fullPackageName -PathType Leaf) { throw "Cannot remove package '$Private:fullPackageName'"}
                Write-Verbose "Local package is removed: '$Private:fullPackageName'"
            } else { Write-Verbose "Local package does not exist: '$Private:fullPackageName'" }

            if ((Get-ChildItem -Path $Private:localRepository | Measure-Object).Count -eq 0) {
                Write-Verbose "Package folder '$Private:localRepository' is empty, removing..."
                $null = Remove-Item -LiteralPath $Private:localRepository
                if (Test-Path -LiteralPath $Private:localRepository -PathType Container) { throw "Cannot remove package folder $Private:localRepository"}
            }
        } else { Write-Verbose "Folder with local packages does not exist '$Private:localRepository'" }

        if ((Get-ChildItem -Path $Private:localAppFolder | Measure-Object).Count -eq 0) {
            Write-Verbose "Local app data folder '$Private:localAppFolder' is empty, removing..."
            $null = Remove-Item -LiteralPath $Private:localAppFolder
            if (Test-Path -LiteralPath $Private:localAppFolder -PathType Container) { throw "Cannot remove app data folder $Private:localAppFolder"}
        }

    } else { Write-Verbose "Local app data folder does not exist '$Private:localAppFolder'" }
}

function Get-PyEmbedPackages {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$appPrefix)

    [string]$Private:localRepository = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $(Join-Path $appPrefix "PyEmbedPackages")))
    if (Test-Path -LiteralPath $Private:localRepository -PathType Container) {
        Write-Verbose "Folder with local packages: '$Private:localRepository'; reading..."

        return $(Get-ChildItem -Path $Private:localRepository -Filter "python*embed*.zip" | 
            Where-Object { !$_.PSIsContainer -and $_.Name -match "python-(?<ver>\d+\.\d+\.\d+)-embed-(?<plat>amd64|win32).zip" } |
            ForEach-Object { 
                $null = $($_.Name -match "python-(?<ver>\d+\.\d+\.\d+)-embed-(?<plat>amd64|win32).zip")
                return [PSCustomObject]@{
                    Name = $_.Name
                    Path = $_.FullName
                    Version = $Matches["ver"]
                    Platform = $Matches["plat"]
                }
            })
    } else { Write-Verbose "Folder with local packages does not exist '$Private:localRepository'" }
}

function Search-PyEmbedPthFile {
    [CmdletBinding()]
    Param([Parameter(Mandatory=$True)][string]$pyHome)

    if (!(Test-Path -LiteralPath $pyHome -PathType Container)) { throw "Python home folder not found: $pyExecutable"}
    return [string]$(Get-ChildItem -Path $pyHome -Filter "python*._pth" | Where-Object { !$_.PSIsContainer -and $_.Name -match "python\d+\._pth" } | ForEach-Object { $_.FullName })
}

function Search-PyEmbedInstalled {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyPlatform,
        [Switch]$fullPath = $False
    )

    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}
    if (!$Script:isWindowsPlatform) {throw "Embedded Python is available on Windows platform only."}
    
    Write-Verbose "Searching installed embedded python deployments for application $appPrefix (expected python platform $pyPlatform"

    $Private:root = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $appPrefix))
    if (Test-Path -LiteralPath $Private:root -PathType Container) {
        foreach($Private:embedFolder in (Get-ChildItem -LiteralPath $Private:root -Filter "python-*-embed-$pyPlatform" | Where-Object { $_.PSIsContainer })) {
            if ($Private:embedFolder.Name -match "python-(\d+\.\d+\.\d+)-embed-$pyPlatform") { 
                Write-Output $(if($fullPath){"$($Private:embedFolder.FullName)\python.exe"}else{$Matches[1]})
            }
        }
    }
}

function Test-PyEmbedInstalled {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform
    )

    if ($pyVersion -notmatch "\d+\.\d+\.\d+") { throw "Incorrect Python version: $pyVersion"}    
    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}
    if (!$Script:isWindowsPlatform) {throw "Embedded Python is available on Windows platform only."}
    
    Write-Verbose "Testing embedded python deployment for application $appPrefix (expected python version $pyVersion; platform $pyPlatform"

    [string]$Private:pyHome = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $(Join-Path $appPrefix "python-$pyVersion-embed-$pyPlatform")))
    Write-Verbose "Python folder: $Private:pyHome"

    if (Test-Path -LiteralPath $Private:pyHome -PathType Container) {
        $Private:pyExecutable = Search-PyExecutable -pyHome $Private:pyHome
        if ($Private:pyExecutable) {
            $Private:pyVersion = Get-PyVersion -pyExecutable $Private:pyExecutable
            if ($Private:pyVersion -eq $pyVersion) {
                if(Get-PipVersion -pyExecutable $Private:pyExecutable) {
                    $Private:pyPth = Search-PyEmbedPthFile -pyHome $Private:pyHome
                    if ($Private:pyPth) {
                        [string]$private:pyPthContent = Get-Content $private:pyPth | Out-String
                        if (!($private:pyPthContent -match "(?ms)#\s*import\s+site\s*")) {
                            Write-Verbose "Python in '$Private:pyHome' is properly installed"
                            return $Private:pyHome
                        } else { Write-Verbose "'import site' is uncommented in $Private:pyPth file"} 
                    } else { Write-Verbose "PTH file not found in '$Private:pyHome'" }
                } else { Write-Verbose "Pip is not installed in '$Private:pyHome'" }
            } else { Write-Verbose "Python version $Private:pyVersion in '$Private:pyHome' does not match expected $pyVersion" }
        } else { Write-Verbose "Python executable not found in '$Private:pyHome'" }
    } else { Write-Verbose "Path '$Private:pyHome' not found" }

    return $null
}

function Install-PyEmbed {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform
    )

    Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Initialization"
    Write-Verbose "Installing embedded python for application $appPrefix (python version $pyVersion; platform $pyPlatform"

    if ($pyVersion -notmatch "\d+\.\d+\.\d+") { throw "Incorrect Python version: $pyVersion"}    
    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}
    if (!$Script:isWindowsPlatform) {throw "Embedded Python is available on Windows platform only."}

    [string]$Private:pyHome = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $(Join-Path $appPrefix "python-$pyVersion-embed-$pyPlatform")))
    Write-Verbose "Python folder: $Private:pyHome"

    if (Test-Path -LiteralPath $Private:pyHome -PathType Container) { throw "Python is already installed by path: $Private:pyHome" }

    Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Getting deployment package"
    [string]$Private:packageName = Get-PyEmbedPackage -appPrefix $appPrefix -pyVersion $pyVersion -pyPlatform $pyPlatform
    Write-Verbose "Python package name: ]$Private:packageName"

    Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Extracting deployment package content"
    Write-Verbose "Extracting python package..."
    $null = Expand-Archive -LiteralPath $Private:packageName -DestinationPath $Private:pyHome
    if (!(Test-Path -LiteralPath $Private:pyHome -PathType Container)) { throw "Cannot extract python package to $Private:pyHome" }

    [string]$Private:pyExecutable = Search-PyExecutable -pyHome $Private:pyHome
    if (!$Private:pyExecutable) { throw "Cannot find Python executable in $Private:pyHome"}

    if (!(Get-PipVersion -pyExecutable $Private:pyExecutable)) {
        Write-Verbose "Pip is not installed; installing..."
        Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Package structure verification"

        [string]$Private:pyPth = Search-PyEmbedPthFile -pyHome $Private:pyHome
        if(!$Private:pyPth) { throw "Python _PTH file not found in $Private:pyHome"}
        Write-Verbose "Target path to _pth file: $Private:pyPth"

        Write-Verbose "Ensure that 'import site' is uncommented in _pth file"
        [string]$private:pyPthContent = Get-Content $Private:pyPth | Out-String
        if ($private:pyPthContent -match "(?ms)#\s*import\s+site\s*") {
            Write-Verbose "'import site' is commented. Uncommenting..."
            $private:pyPthContent = $private:pyPthContent.Replace($Matches[0], "import site")
            $null = ($private:pyPthContent | Set-Content -Path $Private:pyPth)
            Write-Verbose "File $Private:pyPth is updated"
        }

        Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Getting PIP installation script"
        Write-Verbose "Downloading get-pip.py"

        [string]$Private:tempFileName = [System.IO.Path]::GetTempFileName()
        Write-Verbose "Temp file name: $Private:tempFileName"

        [string]$private:getPipTargetPath = [System.IO.Path]::GetFullPath($(Join-Path $Private:pyHome "get-pip.py"))
        Write-Verbose "Target path for get-pip.py: $private:getPipTargetPath"

        try
        {
            $null = Invoke-WebRequest -Uri "https://bootstrap.pypa.io/get-pip.py" -OutFile $Private:tempFileName
            Move-Item -LiteralPath $Private:tempFileName -Destination $private:getPipTargetPath
        }
        catch
        {
            Write-Verbose "Error downloading file: $_"
            Remove-Item -LiteralPath $Private:tempFileName
        }

        Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Installing PIP"
        Write-Verbose "Running get-pip.py"
        $null = (& $Private:pyExecutable $private:getPipTargetPath "--no-warn-script-location")
        if (!(Get-PipVersion -pyExecutable $Private:pyExecutable)) { throw "Was not able to install Pip"}
    }

    Write-Progress -Activity "Installing Embedded Python $pyVersion" -Status "Complete" -Completed
    Write-Verbose "Embedded python is installed by path $Private:pyHome"
    return $Private:pyHome
}

function Uninstall-PyEmbed {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform,
        [Switch]$withPackage = $False
    )

    Write-Verbose "Uninstalling embedded python for application $appPrefix (python version $pyVersion; platform $pyPlatform"

    if ($pyVersion -notmatch "\d+\.\d+\.\d+") { throw "Incorrect Python version: $pyVersion"}    
    if ($pyPlatform -ne 'amd64' -and $pyPlatform -ne 'win32') { throw "Invalid Python Platform '$pyPlatform' - expected either 'amd64' or 'win32'"}
    if (!$Script:isWindowsPlatform) {throw "Embedded Python is available on Windows platform only."}

    [string]$Private:pyHome = [System.IO.Path]::GetFullPath($(Join-Path $localAppData $(Join-Path $appPrefix "python-$pyVersion-embed-$pyPlatform")))
    Write-Verbose "Python folder: $Private:pyHome"
    
    if (Test-Path -LiteralPath $Private:pyHome -PathType Container) {
        Write-Verbose "Deleting Python Home folder: $Private:pyHome"
        $null = Remove-Item -LiteralPath $Private:pyHome -Force -Recurse
        if (Test-Path -LiteralPath $Private:pyHome -PathType Container) { throw "Cannot remove Python home folder: $Private:pyHome" }
    }

    if ($withPackage) {
        Write-Verbose "Removing corresponded package"
        $null = Remove-PyEmbedPackage -appPrefix $appPrefix -pyVersion $pyVersion -pyPlatform $pyPlatform        
    }

    Write-Verbose "Embedded python is removed"
}

function Get-PyEmbed {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)][string]$appPrefix,
        [Parameter(Mandatory=$True)][string]$pyVersion,
        [Parameter(Mandatory=$True)][string]$pyPlatform
    )

    Write-Verbose "Getting embedded python for application $appPrefix (expected python version $pyVersion; platform $pyPlatform"

    [string]$Private:pyHome = Test-PyEmbedInstalled -appPrefix $appPrefix -pyVersion $pyVersion -pyPlatform $pyPlatform
    if (!$Private:pyHome) {
        Write-Verbose "Embedded python not found; installing..."
        $null = Uninstall-PyEmbed -appPrefix $appPrefix -pyVersion $pyVersion -pyPlatform $pyPlatform        
        $Private:pyHome = Install-PyEmbed -appPrefix $appPrefix -pyVersion $pyVersion -pyPlatform $pyPlatform        
    }

    Write-Verbose "Embedded python is available by path $Private:pyHome"
    return $Private:pyHome
}

