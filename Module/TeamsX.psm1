$binaryName = "TeamsX.PowerShell.dll"
$developmentPath = Join-Path -Path $PSScriptRoot -ChildPath "..\TeamsX.PowerShell\bin\Debug"
$preferredFolders = if ($PSEdition -eq "Core") {
    @("net8.0", "net10.0", "netstandard2.0")
} else {
    @("net472", "netstandard2.0")
}

$modulePath = $null
foreach ($folder in $preferredFolders) {
    $candidate = Join-Path -Path $developmentPath -ChildPath "$folder\$binaryName"
    if (Test-Path -LiteralPath $candidate) {
        $modulePath = $candidate
        break
    }
}

if (-not $modulePath) {
    $libFolder = if ($PSEdition -eq "Core") { "Core" } else { "Default" }
    $modulePath = Join-Path -Path $PSScriptRoot -ChildPath "Lib\$libFolder\$binaryName"
}

Import-Module -Name $modulePath -Force -ErrorAction Stop
Export-ModuleMember -Cmdlet "*"
