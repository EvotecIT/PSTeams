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
    CmdletsToExport      = @('Add-DiscordReaction', 'Add-SlackReaction', 'ConvertTo-DiscordJson', 'ConvertTo-SlackJson', 'ConvertTo-TeamsFact', 'ConvertTo-TeamsJson', 'ConvertTo-TeamsSection', 'Get-DiscordMessage', 'New-AdaptiveAction', 'New-AdaptiveActionSet', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveColumnSet', 'New-AdaptiveContainer', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveImageSet', 'New-AdaptiveLineBreak', 'New-AdaptiveMedia', 'New-AdaptiveMediaSource', 'New-AdaptiveMention', 'New-AdaptiveRichTextBlock', 'New-AdaptiveTable', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-DiscordActionRow', 'New-DiscordAllowedMentions', 'New-DiscordAttachment', 'New-DiscordAuthor', 'New-DiscordButton', 'New-DiscordChannelTarget', 'New-DiscordConnection', 'New-DiscordDirectMessageTarget', 'New-DiscordFact', 'New-DiscordFooter', 'New-DiscordImage', 'New-DiscordMessage', 'New-DiscordModal', 'New-DiscordSection', 'New-DiscordSelectOption', 'New-DiscordStringSelect', 'New-DiscordTextInput', 'New-DiscordThreadTarget', 'New-DiscordWebhookTarget', 'New-HeroCard', 'New-SlackActions', 'New-SlackButton', 'New-SlackConnection', 'New-SlackContext', 'New-SlackConversationTarget', 'New-SlackDivider', 'New-SlackHeader', 'New-SlackInput', 'New-SlackMessage', 'New-SlackModal', 'New-SlackPlainTextInput', 'New-SlackSection', 'New-SlackText', 'New-SlackWebhookTarget', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveExecuteAction', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRefresh', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveShowCardAction', 'New-TeamsAdaptiveSubmitAction', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsAdaptiveToggleVisibilityAction', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsCardImage', 'New-TeamsFact', 'New-TeamsHeroCard', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListCard', 'New-TeamsListItem', 'New-TeamsMessage', 'New-TeamsSection', 'New-TeamsThumbnailCard', 'New-TeamsWebhookTarget', 'New-ThumbnailCard', 'Remove-DiscordMessage', 'Remove-DiscordReaction', 'Remove-SlackMessage', 'Remove-SlackReaction', 'Resolve-SlackConversation', 'Send-DiscordMessage', 'Send-SlackFile', 'Send-SlackMessage', 'Send-TeamsMessage', 'Send-TeamsMessageBody', 'Test-DiscordInteractionSignature', 'Update-DiscordMessage', 'Update-SlackMessage')
    AliasesToExport      = @('ActivityImage', 'ActivityImageLink', 'ActivitySubtitle', 'ActivityText', 'ActivityTitle', 'New-AdaptiveImageGallery', 'New-HeroButton', 'New-HeroImage', 'New-TeamsActivityImageLink', 'New-ThumbnailButton', 'New-ThumbnailImage', 'TeamsActivityImage', 'TeamsActivityImageLink', 'TeamsActivitySubtitle', 'TeamsActivityText', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsMessage', 'TeamsMessageBody', 'TeamsSection', 'New-DiscordEmbed', 'New-DiscordField', 'New-DiscordThumbnail')
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
    ScriptsToProcess     = @()
}
