@{
    RootModule           = 'MessageX.psm1'
    ModuleVersion        = '0.1.0'
    GUID                 = 'd62b583e-c92b-4ac1-b83d-b7d710e48cb5'
    Author               = 'Przemyslaw Klys'
    CompanyName          = 'Evotec'
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Compose and deliver Teams, Slack, and Discord messages with typed MessageX libraries and compiled PowerShell cmdlets.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @('Add-DiscordReaction', 'Add-SlackReaction', 'ConvertTo-DiscordJson', 'ConvertTo-SlackJson', 'ConvertTo-TeamsJson', 'Get-DiscordMessage', 'New-DiscordActionRow', 'New-DiscordAllowedMentions', 'New-DiscordAttachment', 'New-DiscordAuthor', 'New-DiscordButton', 'New-DiscordChannelTarget', 'New-DiscordConnection', 'New-DiscordDirectMessageTarget', 'New-DiscordEmbed', 'New-DiscordField', 'New-DiscordFooter', 'New-DiscordImage', 'New-DiscordMessage', 'New-DiscordModal', 'New-DiscordSelectOption', 'New-DiscordStringSelect', 'New-DiscordTextInput', 'New-DiscordThreadTarget', 'New-DiscordWebhookTarget', 'New-SlackActions', 'New-SlackButton', 'New-SlackConnection', 'New-SlackContext', 'New-SlackConversationTarget', 'New-SlackDivider', 'New-SlackHeader', 'New-SlackInput', 'New-SlackMessage', 'New-SlackModal', 'New-SlackPlainTextInput', 'New-SlackSection', 'New-SlackText', 'New-SlackWebhookTarget', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsMessage', 'New-TeamsWebhookTarget', 'Remove-DiscordMessage', 'Remove-DiscordReaction', 'Remove-SlackMessage', 'Remove-SlackReaction', 'Resolve-SlackConversation', 'Send-DiscordMessage', 'Send-SlackFile', 'Send-SlackMessage', 'Send-TeamsMessage', 'Test-DiscordInteractionSignature', 'Update-DiscordMessage', 'Update-SlackMessage')
    AliasesToExport      = @()
    PrivateData          = @{
        PSData = @{
            Tags                       = @('Teams', 'Slack', 'Discord', 'Microsoft', 'MSTeams', 'Notifications', 'Webhook', 'PowerShell', 'Windows', 'MacOS', 'Linux')
            ProjectUri                 = 'https://github.com/EvotecIT/PSTeams'
            ReleaseNotes               = 'Initial MessageX candidate. See MIGRATION.md for the transition from historical PSTeams and PSDiscord modules.'
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RequiredModules      = @()
}
