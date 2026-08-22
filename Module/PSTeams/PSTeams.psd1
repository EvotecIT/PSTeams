@{
    RootModule           = 'PSTeams.psm1'
    ModuleVersion        = '2.4.1'
    GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
    Author               = 'Przemyslaw Klys'
    CompanyName          = 'Evotec'
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'PSTeams provides typed Teams, Slack, and Discord message composition and delivery through MessageX libraries and thin compiled PowerShell cmdlets.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @('ConvertTo-DiscordJson', 'ConvertTo-SlackJson', 'ConvertTo-TeamsFact', 'ConvertTo-TeamsJson', 'ConvertTo-TeamsSection', 'New-AdaptiveAction', 'New-AdaptiveActionSet', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveColumnSet', 'New-AdaptiveContainer', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveImageSet', 'New-AdaptiveLineBreak', 'New-AdaptiveMedia', 'New-AdaptiveMediaSource', 'New-AdaptiveMention', 'New-AdaptiveRichTextBlock', 'New-AdaptiveTable', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-DiscordAllowedMentions', 'New-DiscordAttachment', 'New-DiscordAuthor', 'New-DiscordChannelTarget', 'New-DiscordConnection', 'New-DiscordDirectMessageTarget', 'New-DiscordFact', 'New-DiscordFooter', 'New-DiscordImage', 'New-DiscordMessage', 'New-DiscordSection', 'New-DiscordThreadTarget', 'New-DiscordWebhookTarget', 'New-HeroCard', 'New-SlackConnection', 'New-SlackConversationTarget', 'New-SlackDivider', 'New-SlackMessage', 'New-SlackSection', 'New-SlackText', 'New-SlackWebhookTarget', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveShowCardAction', 'New-TeamsAdaptiveSubmitAction', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsAdaptiveToggleVisibilityAction', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsCardImage', 'New-TeamsFact', 'New-TeamsHeroCard', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListCard', 'New-TeamsListItem', 'New-TeamsMessage', 'New-TeamsSection', 'New-TeamsThumbnailCard', 'New-TeamsWebhookTarget', 'New-ThumbnailCard', 'Send-DiscordMessage', 'Send-SlackMessage', 'Send-TeamsMessage', 'Send-TeamsMessageBody', 'Test-DiscordInteractionSignature')
    AliasesToExport      = @('ActivityImage', 'ActivityImageLink', 'ActivitySubtitle', 'ActivityText', 'ActivityTitle', 'New-AdaptiveImageGallery', 'New-HeroButton', 'New-HeroImage', 'New-TeamsActivityImageLink', 'New-ThumbnailButton', 'New-ThumbnailImage', 'TeamsActivityImage', 'TeamsActivityImageLink', 'TeamsActivitySubtitle', 'TeamsActivityText', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsMessage', 'TeamsMessageBody', 'TeamsSection', 'New-DiscordEmbed', 'New-DiscordField', 'New-DiscordThumbnail')
    PrivateData          = @{
        PSData = @{
            Tags                       = @('Teams', 'Slack', 'Discord', 'Microsoft', 'MSTeams', 'Notifications', 'Webhook', 'PowerShell', 'Windows', 'MacOS', 'Linux')
            ProjectUri                 = 'https://github.com/EvotecIT/PSTeams'
            ReleaseNotes               = 'The current migration keeps PSTeams command names while moving implementation to MessageX.Teams and MessageX.PowerShell.'
            IconUri                    = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RequiredModules      = @()
    ScriptsToProcess     = @()
}
