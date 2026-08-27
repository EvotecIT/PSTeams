# This example builds provider-native incident notifications without embedding credentials.
# Set only the environment variable for the provider you want to exercise.

$incident = [ordered] @{
    Name        = 'DNS service unavailable'
    Environment = 'Test'
    Severity    = 'Critical'
    StartedAt   = [DateTimeOffset]::UtcNow
}

if ($Env:MESSAGEX_TEAMS_WORKFLOW_URL) {
    $teamsTarget = New-TeamsWebhookTarget `
        -Uri $Env:MESSAGEX_TEAMS_WORKFLOW_URL `
        -Workflow `
        -Destination Channel `
        -DisplayName 'Incident channel'
    $teamsCard = New-TeamsAdaptiveCard -Body @(
        New-TeamsAdaptiveTextBlock -Text $incident.Name -Weight Bolder -Size Large
        New-TeamsAdaptiveFactSet -Facts @(
            New-TeamsAdaptiveFact -Title 'Environment' -Value $incident.Environment
            New-TeamsAdaptiveFact -Title 'Severity' -Value $incident.Severity
        )
    )
    Send-TeamsMessage `
        -Message (New-TeamsMessage -Summary $incident.Name -AdaptiveCard $teamsCard) `
        -Target $teamsTarget `
        -PassThru
}

if ($Env:MESSAGEX_SLACK_BOT_TOKEN -and $Env:MESSAGEX_SLACK_CHANNEL_ID) {
    $slackConnection = New-SlackConnection `
        -BotToken (ConvertTo-SecureString $Env:MESSAGEX_SLACK_BOT_TOKEN -AsPlainText -Force)
    $slackTarget = New-SlackConversationTarget -ConversationId $Env:MESSAGEX_SLACK_CHANNEL_ID
    $slackMessage = New-SlackMessage -Text $incident.Name -Blocks @(
        New-SlackHeader -Text $incident.Name
        New-SlackSection -Fields @(
            New-SlackText -Markdown "*Environment*`n$($incident.Environment)"
            New-SlackText -Markdown "*Severity*`n$($incident.Severity)"
        )
    )
    Send-SlackMessage -Message $slackMessage -Target $slackTarget -Connection $slackConnection -PassThru
}

if ($Env:MESSAGEX_DISCORD_BOT_TOKEN -and $Env:MESSAGEX_DISCORD_CHANNEL_ID) {
    $discordConnection = New-DiscordConnection `
        -BotToken (ConvertTo-SecureString $Env:MESSAGEX_DISCORD_BOT_TOKEN -AsPlainText -Force)
    $discordTarget = New-DiscordChannelTarget -ChannelId $Env:MESSAGEX_DISCORD_CHANNEL_ID
    $discordMessage = New-DiscordMessage -Content $incident.Name -Embeds @(
        New-DiscordSection -Title $incident.Name -Color 0xD13438 -Fields @(
            New-DiscordFact -Name 'Environment' -Value $incident.Environment -Inline
            New-DiscordFact -Name 'Severity' -Value $incident.Severity -Inline
        )
    )
    Send-DiscordMessage -Message $discordMessage -Target $discordTarget -Connection $discordConnection -PassThru
}
