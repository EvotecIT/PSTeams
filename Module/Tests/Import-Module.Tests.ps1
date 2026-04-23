Describe 'PSTeams module migration shell' {
    BeforeAll {
        $script:baselinePath = Join-Path -Path $PSScriptRoot -ChildPath 'Baselines'
    }

    BeforeEach {
        Get-Module PSTeams, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
    }

    It 'exports legacy functions and migrated cmdlets together' {
        $module = Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force -PassThru

        $module.ExportedFunctions.Keys | Should -BeNullOrEmpty
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveCard'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveAction'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveActionSet'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveCard'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveColumn'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveColumnSet'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveContainer'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveFact'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveFactSet'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveImage'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveImageSet'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveLineBreak'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveMedia'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveMediaSource'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveMention'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveRichTextBlock'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveTable'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-AdaptiveTextBlock'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'ConvertTo-TeamsFact'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'ConvertTo-TeamsSection'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsSection'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsFact'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsButton'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-CardList'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-CardListButton'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-CardListItem'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-HeroCard'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsActivityTitle'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsActivitySubtitle'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsActivityText'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsActivityImage'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsImage'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsBigImage'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsList'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-TeamsListItem'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'New-ThumbnailCard'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'Send-TeamsMessage'
        $module.ExportedFunctions.Keys | Should -Not -Contain 'Send-TeamsMessageBody'

        $module.ExportedAliases.Keys | Should -Contain 'TeamsMessage'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsSection'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsFact'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsButton'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsActivityTitle'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsActivitySubtitle'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsActivityText'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsActivityImage'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsImage'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsBigImage'
        $module.ExportedAliases.Keys | Should -Contain 'New-HeroButton'
        $module.ExportedAliases.Keys | Should -Contain 'New-HeroImage'
        $module.ExportedAliases.Keys | Should -Contain 'New-AdaptiveImageGallery'
        $module.ExportedAliases.Keys | Should -Contain 'New-ThumbnailButton'
        $module.ExportedAliases.Keys | Should -Contain 'New-ThumbnailImage'
        $module.ExportedAliases.Keys | Should -Contain 'TeamsMessageBody'

        (Get-Alias -Name 'New-HeroImage').Definition | Should -Be 'New-AdaptiveImage'
        (Get-Alias -Name 'New-ThumbnailImage').Definition | Should -Be 'New-AdaptiveImage'
        (Get-Command -Name 'Convert-Color' -Module PSTeams -ErrorAction SilentlyContinue) | Should -BeNullOrEmpty

        $module.ExportedCmdlets.Keys | Should -Contain 'ConvertTo-TeamsFact'
        $module.ExportedCmdlets.Keys | Should -Contain 'ConvertTo-TeamsJson'
        $module.ExportedCmdlets.Keys | Should -Contain 'ConvertTo-TeamsSection'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveAction'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveActionSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveColumn'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveColumnSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveContainer'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveFact'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveFactSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveImageSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveLineBreak'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveMedia'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveMediaSource'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveMention'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveRichTextBlock'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveTable'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-AdaptiveTextBlock'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-CardList'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-CardListButton'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-CardListItem'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-HeroCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveRichTextBlock'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveShowCardAction'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveSubmitAction'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsActivityTitle'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsActivitySubtitle'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsActivityText'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsActivityImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsCardImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsBigImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsList'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsListItem'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsButton'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsFact'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsGraphTarget'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsHeroCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsMessage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsSection'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsListCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsThumbnailCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsWebhookTarget'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-ThumbnailCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'Send-TeamsMessage'
        $module.ExportedCmdlets.Keys | Should -Contain 'Send-TeamsMessageBody'
    }

    It 'preserves every legacy command name on main' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force | Out-Null

        $legacyNamesJson = Get-Content (Join-Path -Path $baselinePath -ChildPath 'LegacyCommands.json') -Raw
        $legacyNames = @(ConvertFrom-Json $legacyNamesJson | ForEach-Object { $_ })
        $currentNames = @(Get-Command -Module PSTeams | Select-Object -ExpandProperty Name | Sort-Object)

        $missing = @($legacyNames | Where-Object { $_ -notin $currentNames } | Sort-Object)
        $missing | Should -BeNullOrEmpty
    }

    It 'preserves every legacy alias target on main' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force | Out-Null

        $legacyAliasesJson = Get-Content (Join-Path -Path $baselinePath -ChildPath 'LegacyAliases.json') -Raw
        $legacyAliases = @(ConvertFrom-Json $legacyAliasesJson | ForEach-Object { $_ })
        $currentAliases = @(Get-Alias | Where-Object Source -eq 'PSTeams' | ForEach-Object { '{0}=>{1}' -f $_.Name, $_.Definition } | Sort-Object)

        $missing = @($legacyAliases | Where-Object { $_ -notin $currentAliases } | Sort-Object)
        $missing | Should -BeNullOrEmpty
    }
}
