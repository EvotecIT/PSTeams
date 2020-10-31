@{
    AliasesToExport      = @('New-HeroButton', 'ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage', 'ActivitySubtitle', 'TeamsActivitySubtitle', 'ActivityText', 'TeamsActivityText', 'ActivityTitle', 'TeamsActivityTitle', 'TeamsBigImage', 'TeamsButton', 'TeamsFact', 'TeamsImage', 'TeamsList', 'TeamsListItem', 'TeamsSection', 'TeamsMessage', 'TeamsMessageBody')
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @()
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2020 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Simple project to send messages to Microsoft Teams Channel.'
    FunctionsToExport    = @('ConvertTo-TeamsFact', 'ConvertTo-TeamsSection', 'New-AdaptiveCard', 'New-AdaptiveColumn', 'New-AdaptiveFact', 'New-AdaptiveFactSet', 'New-AdaptiveImage', 'New-AdaptiveRichTextBlock', 'New-AdaptiveSection', 'New-AdaptiveTextBlock', 'New-CardList', 'New-CardListButton', 'New-CardListItem', 'New-HeroCard', 'New-HeroImage1', 'New-TeamsActivityImage', 'New-TeamsActivitySubtitle', 'New-TeamsActivityText', 'New-TeamsActivityTitle', 'New-TeamsBigImage', 'New-TeamsButton', 'New-TeamsFact', 'New-TeamsImage', 'New-TeamsList', 'New-TeamsListItem', 'New-TeamsSection', 'Send-TeamsMessage', 'Send-TeamsMessageBody')
    GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
    ModuleVersion        = '2.0.0'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            Tags       = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'Windows', 'macOS', 'Linux')
            ProjectUri = 'https://github.com/EvotecIT/PSTeams'
            IconUri    = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
            Prerelease = 'Alpha1'
        }
    }
    RootModule           = 'PSTeams.psm1'
}