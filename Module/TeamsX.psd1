@{
    RootModule           = 'TeamsX.psm1'
    ModuleVersion        = '0.1.0'
    GUID                 = '2ce3429c-d55a-4d79-97d2-a4ac17549936'
    Author               = 'Przemyslaw Klys'
    CompanyName          = 'Evotec'
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'TeamsX is a binary-first PowerShell module for composing and sending Microsoft Teams messages using typed C# cmdlets over the TeamsX .NET library.'
    PowerShellVersion    = '5.1'
    CompatiblePSEditions = @('Desktop', 'Core')
    FunctionsToExport    = @()
    CmdletsToExport      = @('ConvertTo-TeamsJson', 'New-TeamsAdaptiveActionSet', 'New-TeamsAdaptiveCard', 'New-TeamsAdaptiveColumn', 'New-TeamsAdaptiveColumnSet', 'New-TeamsAdaptiveContainer', 'New-TeamsAdaptiveFact', 'New-TeamsAdaptiveFactSet', 'New-TeamsAdaptiveImage', 'New-TeamsAdaptiveImageSet', 'New-TeamsAdaptiveMedia', 'New-TeamsAdaptiveMediaSource', 'New-TeamsAdaptiveMention', 'New-TeamsAdaptiveOpenUrlAction', 'New-TeamsAdaptiveRichTextBlock', 'New-TeamsAdaptiveTextBlock', 'New-TeamsAdaptiveTextRun', 'New-TeamsAdaptiveToggleVisibilityAction', 'New-TeamsMessage', 'New-TeamsWebhookTarget', 'Send-TeamsMessage')
    AliasesToExport      = @()
    PrivateData          = @{
        PSData = @{
            Tags                       = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'PowerShell', 'Windows', 'MacOS', 'Linux')
            ProjectUri                 = 'https://github.com/EvotecIT/PSTeams'
            ReleaseNotes               = 'Binary-first TeamsX PowerShell module for the main branch.'
            IconUri                    = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RequiredModules      = @()
    ScriptsToProcess     = @()
}
