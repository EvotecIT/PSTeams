Describe 'TeamsX module' {
    BeforeEach {
        Get-Module TeamsX, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
    }

    It 'exports cmdlets only' {
        $module = Import-Module "$PSScriptRoot\..\TeamsX.psd1" -Force -PassThru

        $module.ExportedFunctions.Count | Should -Be 0
        $module.ExportedAliases.Count | Should -Be 0

        $module.ExportedCmdlets.Keys | Should -Contain 'ConvertTo-TeamsJson'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveActionSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveColumn'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveColumnSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveContainer'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveFact'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveFactSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveImage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveImageSet'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveMedia'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveMediaSource'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveMention'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveOpenUrlAction'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveRichTextBlock'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveTextBlock'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveTextRun'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveToggleVisibilityAction'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsMessage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsWebhookTarget'
        $module.ExportedCmdlets.Keys | Should -Contain 'Send-TeamsMessage'
    }
}
