$binaryName = 'TeamsX.PowerShell.dll'
$developmentPath = Join-Path -Path $PSScriptRoot -ChildPath '..\..\TeamsX.PowerShell\bin\Debug'
$preferredFolders = if ($PSEdition -eq 'Core') {
    $runtimeMajor = [System.Environment]::Version.Major
    if ($runtimeMajor -ge 10) {
        @('net10.0', 'net8.0', 'netstandard2.0')
    } elseif ($runtimeMajor -ge 8) {
        @('net8.0', 'netstandard2.0')
    } else {
        @('netstandard2.0')
    }
} else {
    @('net472', 'netstandard2.0')
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
    $libFolder = if ($PSEdition -eq 'Core') { 'Core' } else { 'Default' }
    $modulePath = Join-Path -Path $PSScriptRoot -ChildPath "Lib\$libFolder\$binaryName"
}

Import-Module -Name $modulePath -Force -ErrorAction Stop

$binaryAliases = @{
    ActivityImage                = 'New-TeamsActivityImage'
    ActivityImageLink            = 'New-TeamsActivityImage'
    ActivitySubtitle             = 'New-TeamsActivitySubtitle'
    ActivityText                 = 'New-TeamsActivityText'
    ActivityTitle                = 'New-TeamsActivityTitle'
    'New-AdaptiveImageGallery'   = 'New-AdaptiveImageSet'
    'New-HeroButton'             = 'New-CardListButton'
    'New-HeroImage'              = 'New-AdaptiveImage'
    'New-TeamsActivityImageLink' = 'New-TeamsActivityImage'
    'New-ThumbnailButton'        = 'New-CardListButton'
    'New-ThumbnailImage'         = 'New-AdaptiveImage'
    TeamsActivityImage           = 'New-TeamsActivityImage'
    TeamsActivityImageLink       = 'New-TeamsActivityImage'
    TeamsActivitySubtitle        = 'New-TeamsActivitySubtitle'
    TeamsActivityText            = 'New-TeamsActivityText'
    TeamsActivityTitle           = 'New-TeamsActivityTitle'
    TeamsBigImage                = 'New-TeamsBigImage'
    TeamsButton                  = 'New-TeamsButton'
    TeamsFact                    = 'New-TeamsFact'
    TeamsImage                   = 'New-TeamsImage'
    TeamsList                    = 'New-TeamsList'
    TeamsListItem                = 'New-TeamsListItem'
    TeamsSection                 = 'New-TeamsSection'
    TeamsMessage                 = 'Send-TeamsMessage'
    TeamsMessageBody             = 'Send-TeamsMessageBody'
}

foreach ($alias in $binaryAliases.GetEnumerator()) {
    Set-Alias -Name $alias.Key -Value $alias.Value -Scope Local
}

Export-ModuleMember -Alias * -Cmdlet *
