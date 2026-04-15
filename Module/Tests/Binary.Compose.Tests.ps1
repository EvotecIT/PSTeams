Describe 'TeamsX binary cmdlets' {
    BeforeEach {
        Get-Module TeamsX, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
    }

    It 'renders adaptive card JSON from typed cmdlets only' {
        Import-Module "$PSScriptRoot\..\TeamsX.psd1" -Force

        $richText = New-TeamsAdaptiveRichTextBlock -Inlines @(
            New-TeamsAdaptiveTextRun -Text 'Run ' -Color Default
            New-TeamsAdaptiveTextRun -Text '42' -Weight Bolder -Color Attention
        )

        $body = @(
            New-TeamsAdaptiveTextBlock -Text 'Build failed' -Weight Bolder -Color Attention
            $richText
            New-TeamsAdaptiveImageSet -ImageSize Medium -Images @(
                New-TeamsAdaptiveImage -Url 'https://example.test/image-1.png' -AltText 'First'
                New-TeamsAdaptiveImage -Url 'https://example.test/image-2.png' -AltText 'Second'
            )
            New-TeamsAdaptiveMedia -PosterUrl 'https://example.test/poster.png' -AlternateText 'Build walkthrough' -Sources @(
                New-TeamsAdaptiveMediaSource -Type 'video/mp4' -Url 'https://example.test/video.mp4'
            )
        )

        $actions = @(
            New-TeamsAdaptiveOpenUrlAction -Title 'Open build' -Url 'https://example.test/build/42'
            New-TeamsAdaptiveToggleVisibilityAction -Title 'Toggle details' -TargetElementIds 'detailsBlock', 'detailsFactSet'
        )

        $mentions = @(
            New-TeamsAdaptiveMention -Text 'Ops Team' -UserPrincipalName 'ops@example.test' -Name 'Ops Team'
        )

        $card = New-TeamsAdaptiveCard -Body $body -Actions $actions -Mentions $mentions
        $message = New-TeamsMessage -Summary 'Build notification' -AdaptiveCard $card
        $json = $message | ConvertTo-TeamsJson

        $json | Should -Match '"type":"AdaptiveCard"'
        $json | Should -Match '"type":"ImageSet"'
        $json | Should -Match '"type":"Media"'
        $json | Should -Match '"type":"RichTextBlock"'
        $json | Should -Match '"type":"Action.ToggleVisibility"'
        $json | Should -Match '"type":"mention"'
        $json | Should -Match '"url":"https://example.test/build/42"'
    }

    It 'creates standard and workflow webhook targets' {
        Import-Module "$PSScriptRoot\..\TeamsX.psd1" -Force

        $incoming = New-TeamsWebhookTarget -Uri 'https://example.test/incoming'
        $workflow = New-TeamsWebhookTarget -Uri 'https://example.test/workflow' -Workflow

        $incoming.DeliveryMethod.ToString() | Should -Be 'IncomingWebhook'
        $workflow.DeliveryMethod.ToString() | Should -Be 'WorkflowWebhook'
    }

    It 'supports Send-TeamsMessage in WhatIf mode with typed input' {
        Import-Module "$PSScriptRoot\..\TeamsX.psd1" -Force

        $message = New-TeamsMessage -Text 'Hello from TeamsX'
        $target = New-TeamsWebhookTarget -Uri 'https://example.test/webhook'

        { Send-TeamsMessage -Message $message -Target $target -WhatIf } | Should -Not -Throw
    }
}
