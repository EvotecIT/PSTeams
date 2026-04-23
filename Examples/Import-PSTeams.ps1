$currentDirectory = [System.IO.DirectoryInfo]$PSScriptRoot

while ($null -ne $currentDirectory) {
    $modulePath = Join-Path $currentDirectory.FullName 'Module\PSTeams\PSTeams.psd1'
    if (Test-Path -LiteralPath $modulePath) {
        Import-Module -Name $modulePath -Force
        return
    }

    $currentDirectory = $currentDirectory.Parent
}

throw "Unable to locate Module\\PSTeams\\PSTeams.psd1 from '$PSScriptRoot'."
