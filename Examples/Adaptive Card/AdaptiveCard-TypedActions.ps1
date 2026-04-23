. $PSScriptRoot\..\Import-PSTeams.ps1

$card = New-TeamsAdaptiveCard -FallbackText 'Build failed' -Body @(
    New-TeamsAdaptiveTextBlock -Text 'Build failed' -Weight Bolder -Color Attention
    New-TeamsAdaptiveFactSet -Facts @(
        New-TeamsAdaptiveFact -Title 'Run' -Value '42'
        New-TeamsAdaptiveFact -Title 'Status' -Value 'Failed'
    )
) -Actions @(
    New-TeamsAdaptiveOpenUrlAction -Title 'Open build' -Url 'https://example.test/build/42'
    New-TeamsAdaptiveSubmitAction -Title 'Acknowledge'
    New-TeamsAdaptiveShowCardAction -Title 'Details' -Body @(
        New-TeamsAdaptiveTextBlock -Text 'Nested details'
    ) -Actions @(
        New-TeamsAdaptiveSubmitAction -Title 'Confirm'
    )
)

$message = New-TeamsMessage -Summary 'Build notification' -AdaptiveCard $card
$message | ConvertTo-TeamsJson
