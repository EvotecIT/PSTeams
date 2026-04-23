. $PSScriptRoot\..\Import-PSTeams.ps1

$target = New-TeamsWebhookTarget -Uri 'https://example.test/webhook'

$heroCard = New-TeamsHeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text 'Monorail text' -Images @(
    New-TeamsCardImage -Url 'https://example.test/monorail.jpg' -AlternateText 'Monorail'
) -Buttons @(
    New-CardListButton -Type OpenUrl -Title 'Official website' -Value 'https://example.test'
)

$thumbnailCard = New-TeamsThumbnailCard -Title 'Bender' -SubTitle 'robot' -Text 'Futurama' -Images @(
    New-TeamsCardImage -Url 'https://example.test/bender.png' -AlternateText 'Bender'
) -Buttons @(
    New-CardListButton -Type ImBack -Title 'Thumbs Up' -Value 'I like it'
)

$listCard = New-TeamsListCard -Title 'Card Title' -Items @(
    New-CardListItem -Type File -Title 'Report' -SubTitle 'teams > new > design' -TapType OpenUrl -TapValue 'https://contoso.example/report.xlsx' -TapAction editOnline
    New-CardListItem -Type Person -Title 'John Doe' -SubTitle 'Manager' -TapType ImBack -TapValue 'JohnDoe@contoso.com' -TapAction whois
) -Buttons @(
    New-CardListButton -Type OpenUrl -Title 'Show' -Value 'https://evotec.xyz'
)

# Send-TeamsMessage -HeroCard $heroCard -Target $target
# Send-TeamsMessage -ThumbnailCard $thumbnailCard -Target $target
# Send-TeamsMessage -ListCard $listCard -Target $target

$heroCard | ConvertTo-TeamsJson
$thumbnailCard | ConvertTo-TeamsJson
$listCard | ConvertTo-TeamsJson
