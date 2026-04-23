# Get public and private function definition files.
$Public = @(Get-ChildItem -Path $PSScriptRoot\Public\*.ps1 -ErrorAction SilentlyContinue -Recurse -File)
$Private = @(Get-ChildItem -Path $PSScriptRoot\Private\*.ps1 -ErrorAction SilentlyContinue -Recurse -File)
$Classes = @(Get-ChildItem -Path $PSScriptRoot\Classes\*.ps1 -ErrorAction SilentlyContinue -Recurse -File)
$Enums = @(Get-ChildItem -Path $PSScriptRoot\Enums\*.ps1 -ErrorAction SilentlyContinue -Recurse -File)

$binaryModuleName = 'TeamsX.PowerShell.dll'
$binaryModules = @(
    $binaryModuleName
)

# Keep the source-tree module usable during development. Prefer the PowerShell 7.x support build.
# Production packaging is handled by Build-Module.ps1.
$development = $true
$developmentPath = Join-Path -Path $PSScriptRoot -ChildPath '..\..\TeamsX.PowerShell\bin\Debug'
$developmentFolderDefault = 'net472'
$preferredDevelopmentCoreFolders = if ([System.Environment]::Version.Major -ge 8) {
    @('net8.0', 'net10.0', 'netstandard2.0')
} else {
    @('net8.0', 'netstandard2.0')
}

$developmentFolderCore = foreach ($folder in $preferredDevelopmentCoreFolders) {
    if (Test-Path -LiteralPath (Join-Path -Path $developmentPath -ChildPath "$folder\$binaryModuleName")) {
        $folder
        break
    }
}
if (-not $developmentFolderCore) {
    $developmentFolderCore = $preferredDevelopmentCoreFolders[0]
}

# Lets find which libraries we need to load when running from a built module layout.
$default = $false
$core = $false
$standard = $false
$assemblyFolders = @(Get-ChildItem -Path $PSScriptRoot\Lib -Directory -ErrorAction SilentlyContinue)
foreach ($folder in $assemblyFolders.Name) {
    if ($folder -eq 'Default') {
        $default = $true
    } elseif ($folder -eq 'Core') {
        $core = $true
    } elseif ($folder -eq 'Standard') {
        $standard = $true
    }
}

if ($standard -and $core -and $default) {
    $framework = 'Standard'
    $frameworkNet = 'Default'
} elseif ($standard -and $core) {
    $framework = 'Standard'
    $frameworkNet = 'Standard'
} elseif ($core -and $default) {
    $framework = 'Core'
    $frameworkNet = 'Default'
} elseif ($standard -and $default) {
    $framework = 'Standard'
    $frameworkNet = 'Default'
} elseif ($standard) {
    $framework = 'Standard'
    $frameworkNet = 'Standard'
} elseif ($core) {
    $framework = 'Core'
    $frameworkNet = ''
} elseif ($default) {
    $framework = ''
    $frameworkNet = 'Default'
} else {
    $framework = ''
    $frameworkNet = ''
}

$binaryDev = @(
    foreach ($binaryModule in $binaryModules) {
        if ($PSEdition -eq 'Core') {
            $path = Resolve-Path (Join-Path -Path $developmentPath -ChildPath "$developmentFolderCore\$binaryModule") -ErrorAction SilentlyContinue
        } else {
            $path = Resolve-Path (Join-Path -Path $developmentPath -ChildPath "$developmentFolderDefault\$binaryModule") -ErrorAction SilentlyContinue
        }

        if ($path) {
            $path
        }
    }
)

$assemblies = @(
    if ($framework -and $PSEdition -eq 'Core') {
        Get-ChildItem -Path $PSScriptRoot\Lib\$framework\*.dll -ErrorAction SilentlyContinue -Recurse -File
    }
    if ($frameworkNet -and $PSEdition -ne 'Core') {
        Get-ChildItem -Path $PSScriptRoot\Lib\$frameworkNet\*.dll -ErrorAction SilentlyContinue -Recurse -File
    }
)

$foundErrors = @(
    if ($development -and $binaryDev.Count -gt 0) {
        foreach ($binaryModule in $binaryDev) {
            try {
                Import-Module -Name $binaryModule -Force -ErrorAction Stop
            } catch {
                Write-Warning "Failed to import module $($binaryModule): $($_.Exception.Message)"
                $true
            }
        }
    } else {
        foreach ($binaryModule in $binaryModules) {
            try {
                if ($framework -and $PSEdition -eq 'Core') {
                    Import-Module -Name "$PSScriptRoot\Lib\$framework\$binaryModule" -Force -ErrorAction Stop
                }
                if ($frameworkNet -and $PSEdition -ne 'Core') {
                    Import-Module -Name "$PSScriptRoot\Lib\$frameworkNet\$binaryModule" -Force -ErrorAction Stop
                }
            } catch {
                Write-Warning "Failed to import module $($binaryModule): $($_.Exception.Message)"
                $true
            }
        }
    }

    foreach ($import in @($assemblies)) {
        try {
            Add-Type -Path $import.FullName -ErrorAction Stop
        } catch [System.Reflection.ReflectionTypeLoadException] {
            Write-Warning "Processing $($import.Name) exception: $($_.Exception.Message)"
            foreach ($loaderException in ($_.Exception.LoaderExceptions | Sort-Object -Unique)) {
                Write-Warning "Processing $($import.Name) LoaderExceptions: $($loaderException.Message)"
            }
            $true
        } catch {
            Write-Warning "Processing $($import.Name) exception: $($_.Exception.Message)"
            foreach ($loaderException in ($_.Exception.LoaderExceptions | Sort-Object -Unique)) {
                Write-Warning "Processing $($import.Name) LoaderExceptions: $($loaderException.Message)"
            }
            $true
        }
    }

    # Dot source the files.
    foreach ($import in @($Classes + $Enums + $Private + $Public)) {
        try {
            . $import.FullName
        } catch {
            Write-Error -Message "Failed to import functions from $($import.FullName): $_"
            $true
        }
    }
)

if ($foundErrors.Count -gt 0) {
    $moduleName = (Get-ChildItem $PSScriptRoot\*.psd1).BaseName
    Write-Warning "Importing module $moduleName failed. Fix errors before continuing."
    break
}

Export-ModuleMember -Function * -Alias * -Cmdlet *
