@{
    RootModule           = 'PSTeams.psm1'
    ModuleVersion        = '2.4.1'
    GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
    Author               = 'Przemyslaw Klys'
    CompanyName          = 'Evotec'
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'PSTeams provides typed Microsoft Teams message composition and delivery through the reusable TeamsX library and compiled PowerShell cmdlets.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @('ConvertTo-TeamsFact', 'ConvertTo-TeamsJson', 'ConvertTo-TeamsSection', 'New-AdaptiveAction', 'New-AdaptiveActionSet', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveColumnSet', 'New-AdaptiveContainer', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveImageSet', 'New-AdaptiveLineBreak', 'New-AdaptiveMedia', 'New-AdaptiveMediaSource', 'New-AdaptiveMention', 'New-AdaptiveRichTextBlock', 'New-AdaptiveTable', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-HeroCard', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveShowCardAction', 'New-TeamsAdaptiveSubmitAction', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsAdaptiveToggleVisibilityAction', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsCardImage', 'New-TeamsFact', 'New-TeamsHeroCard', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListCard', 'New-TeamsListItem', 'New-TeamsMessage', 'New-TeamsSection', 'New-TeamsThumbnailCard', 'New-TeamsWebhookTarget', 'New-ThumbnailCard', 'Send-TeamsMessage', 'Send-TeamsMessageBody')
    AliasesToExport      = @('ActivityImage', 'ActivityImageLink', 'ActivitySubtitle', 'ActivityText', 'ActivityTitle', 'New-AdaptiveImageGallery', 'New-HeroButton', 'New-HeroImage', 'New-TeamsActivityImageLink', 'New-ThumbnailButton', 'New-ThumbnailImage', 'TeamsActivityImage', 'TeamsActivityImageLink', 'TeamsActivitySubtitle', 'TeamsActivityText', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsMessage', 'TeamsMessageBody', 'TeamsSection')
    PrivateData          = @{
        PSData = @{
            Tags                       = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'Webhook', 'PowerShell', 'Windows', 'MacOS', 'Linux')
            ProjectUri                 = 'https://github.com/EvotecIT/PSTeams'
            ReleaseNotes               = 'Main branch now ships the PSTeams module shell from Module\PSTeams and migrates functionality incrementally to TeamsX-based cmdlets.'
            IconUri                    = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RequiredModules      = @()
    ScriptsToProcess     = @()
}
