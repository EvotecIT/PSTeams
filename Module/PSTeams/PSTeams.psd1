@{
    RootModule           = 'PSTeams.psm1'
    ModuleVersion        = '2.4.1'
    GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
    Author               = 'Przemyslaw Klys'
    CompanyName          = 'Evotec'
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'PSTeams is being migrated 1:1 from PowerShell functions to C# cmdlets over the reusable TeamsX .NET library while the shipping module shell stays in Module\PSTeams.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @('ConvertTo-TeamsFact', 'ConvertTo-TeamsJson', 'ConvertTo-TeamsSection', 'New-AdaptiveAction', 'New-AdaptiveActionSet', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveColumnSet', 'New-AdaptiveContainer', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveImageSet', 'New-AdaptiveLineBreak', 'New-AdaptiveMedia', 'New-AdaptiveMediaSource', 'New-AdaptiveMention', 'New-AdaptiveRichTextBlock', 'New-AdaptiveTable', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-HeroCard', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveShowCardAction', 'New-TeamsAdaptiveSubmitAction', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsAdaptiveToggleVisibilityAction', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsCardImage', 'New-TeamsFact', 'New-TeamsGraphTarget', 'New-TeamsHeroCard', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListCard', 'New-TeamsListItem', 'New-TeamsMessage', 'New-TeamsSection', 'New-TeamsThumbnailCard', 'New-TeamsWebhookTarget', 'New-ThumbnailCard', 'Send-TeamsMessage', 'Send-TeamsMessageBody')
    AliasesToExport      = @('New-HeroImage', 'New-ThumbnailImage', 'New-AdaptiveImageGallery', 'New-HeroButton', 'New-ThumbnailButton', 'ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage', 'ActivitySubtitle', 'TeamsActivitySubtitle', 'ActivityText', 'TeamsActivityText', 'ActivityTitle', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsSection', 'TeamsMessage', 'TeamsMessageBody')
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
