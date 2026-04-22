. (Join-Path $PSScriptRoot '..\Import-PSTeams.ps1')

$message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42 stopped in the release stage.' -Summary 'Build summary' -Color DodgerBlue -HideOriginalBody -Sections @(
    New-TeamsSection -Title 'Build summary' -ActivityTitle 'Release pipeline' -ActivitySubtitle 'Run 42' -ActivityText 'Deployment stopped after test failures.' -ActivityDetails @(
        New-TeamsFact -Name 'Status' -Value 'Failed'
        New-TeamsFact -Name 'Environment' -Value 'Production'
        New-TeamsFact -Name 'Owner' -Value 'Platform Team'
    ) -Buttons @(
        New-TeamsButton -Name 'Open build' -Link 'https://example.test/build/42' -Type OpenUri
    )
)

$json = $message | ConvertTo-TeamsJson
$json | ConvertFrom-Json | ConvertTo-Json -Depth 20

# $target = New-TeamsWebhookTarget -Uri $Env:TEAMSPESTERID
# Send-TeamsMessage -Message $message -Target $target
