. (Join-Path $PSScriptRoot '..\Import-PSTeams.ps1')

$message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42 stopped in the release stage.' -Summary 'Build summary'
$target = New-TeamsGraphTarget -ChatId '19:testchat@thread.v2' -AccessTokenVariableName 'TEAMSX_GRAPH_TOKEN' -DisplayName 'Ops Chat'

Send-TeamsMessage -Message $message -Target $target

# Graph chat and channel messages currently use HTML body rendering for plain text/message-card content,
# or adaptive-card attachments when -AdaptiveCard is present. Adaptive cards should stick to Action.OpenUrl for now.
# You can also use -SecureAccessToken instead of -AccessTokenVariableName when you already have a SecureString token.
# For normal posting, use a delegated Graph access token. Microsoft documents application permissions here as migration-only.
