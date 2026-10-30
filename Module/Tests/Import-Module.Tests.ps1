Describe 'MessageX module exports' {
    It 'exports only compiled commands and no aliases' {
        Get-Module MessageX, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        $module = Import-Module "$PSScriptRoot\..\MessageX\MessageX.psd1" -Force -PassThru
        $module.ExportedFunctions.Keys | Should -BeNullOrEmpty
        $module.ExportedAliases.Keys | Should -BeNullOrEmpty
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsWebhookTarget'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-TeamsAdaptiveCard'
        $module.ExportedCmdlets.Keys | Should -Contain 'Send-TeamsMessage'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-DiscordEmbed'
        $module.ExportedCmdlets.Keys | Should -Contain 'New-DiscordField'
        $module.ExportedCmdlets.Keys | Should -Not -Contain 'New-AdaptiveCard'
        $module.ExportedCmdlets.Keys | Should -Not -Contain 'New-TeamsSection'
        $module.ExportedCmdlets.Keys | Should -Not -Contain 'Send-TeamsMessageBody'
    }
}
