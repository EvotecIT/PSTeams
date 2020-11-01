@{
    AliasesToExport      = @('New-HeroImage', 'New-ThumbnailImage', 'New-HeroButton', 'New-ThumbnailButton', 'ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage', 'ActivitySubtitle', 'TeamsActivitySubtitle', 'ActivityText', 'TeamsActivityText', 'ActivityTitle', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsSection', 'TeamsMessage', 'TeamsMessageBody')
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @()
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2020 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Simple project to send messages to Microsoft Teams Channel.'
    FunctionsToExport    = @('ConvertTo-TeamsFact', 'ConvertTo-TeamsSection', 'New-AdaptiveAction', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveColumnSet', 'New-AdaptiveContainer', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveMedia', 'New-AdaptiveMediaSource', 'New-AdaptiveRichTextBlock', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-HeroCard', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsFact', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListItem', 'New-TeamsSection', 'New-ThumbnailCard', 'Send-TeamsMessage', 'Send-TeamsMessageBody')
    GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
    ModuleVersion        = '2.0.0'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            Tags                       = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'Windows', 'macOS', 'Linux')
            ProjectUri                 = 'https://github.com/EvotecIT/PSTeams'
            IconUri                    = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
            Prerelease                 = 'Alpha1'
            ExternalModuleDependencies = @('Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility')
        }
    }
    RequiredModules      = @(@{
            ModuleVersion = '0.0.186'
            ModuleName    = 'PSSharedGoods'
            Guid          = 'ee272aa8-baaa-4edf-9f45-b6d6f7d844fe'
        }, 'Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility')
    RootModule           = 'PSTeams.psm1'
}