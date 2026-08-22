Describe 'TeamsX binary cmdlets through PSTeams' {
    BeforeEach {
        Get-Module PSTeams, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
    }

    It 'renders adaptive card JSON from typed cmdlets only' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $richText = New-TeamsAdaptiveRichTextBlock -Inlines @(
            New-TeamsAdaptiveTextRun -Text 'Run ' -Color Default
            New-TeamsAdaptiveTextRun -Text '42' -Weight Bolder -Color Attention
        ) -Id 'summary' -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Hidden

        $body = @(
            New-TeamsAdaptiveTextBlock -Text 'Build failed' -Weight Bolder -Color Attention
            New-TeamsAdaptiveContainer -Style Emphasis -Bleed -MinimumHeight 120 -HorizontalAlignment Center -VerticalContentAlignment center -Height Stretch -Spacing Medium -Separator -Id 'panel' -BackgroundUrl 'https://example.test/background.png' -BackgroundFillMode Cover -BackgroundHorizontalAlignment left -BackgroundVerticalAlignment top -SelectActionUrl 'https://example.test/panel' -SelectActionTitle 'Open panel' -Items @(
                New-TeamsAdaptiveColumnSet -Style Good -Bleed -MinimumHeight 80 -HorizontalAlignment Center -Height Stretch -Spacing Medium -Separator -Columns @(
                    New-TeamsAdaptiveColumn -WidthInWeight 2 -MinimumHeight 90 -HorizontalAlignment Right -VerticalContentAlignment Bottom -Spacing Small -Style Attention -Separator -Hidden -SelectAction 'Action.ToggleVisibility' -SelectActionId 'toggle-column' -SelectActionTitle 'Toggle column' -SelectActionTargetElement 'detailsBlock' -Items @(
                        New-TeamsAdaptiveTextBlock -Text 'Weighted column'
                    )
                    New-TeamsAdaptiveColumn -Width 'auto' -Items @(
                        New-TeamsAdaptiveImage -Url 'https://example.test/status.png' -AltText 'Status'
                    )
                )
            )
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
            New-TeamsAdaptiveSubmitAction -Title 'Acknowledge'
            New-TeamsAdaptiveToggleVisibilityAction -Title 'Toggle details' -TargetElementIds 'detailsBlock', 'detailsFactSet'
            New-TeamsAdaptiveShowCardAction -Title 'More details' -Body @(
                New-TeamsAdaptiveTextBlock -Text 'Nested details'
            ) -Actions @(
                New-TeamsAdaptiveSubmitAction -Title 'Nested acknowledge'
            )
        )

        $mentions = @(
            New-TeamsAdaptiveMention -Text 'Ops Team' -UserPrincipalName 'ops@example.test' -Name 'Ops Team'
        )

        $card = New-TeamsAdaptiveCard -Body $body -Actions $actions -Mentions $mentions -FallbackText 'Fallback text' -MinimumHeight 140 -Speak 'Build failed' -Language 'en' -VerticalContentAlignment center -BackgroundUrl 'https://example.test/card-background.png' -BackgroundFillMode Cover -BackgroundHorizontalAlignment left -BackgroundVerticalAlignment top -SelectActionUrl 'https://example.test/card' -SelectActionTitle 'Open card' -AllowImageExpand -FullWidth
        $message = New-TeamsMessage -Summary 'Build notification' -AdaptiveCard $card
        $json = $message | ConvertTo-TeamsJson

        $json | Should -Match '"type":"AdaptiveCard"'
        $json | Should -Match '"fallbackText":"Fallback text"'
        $json | Should -Match '"minHeight":"140px"'
        $json | Should -Match '"allowExpand":true'
        $json | Should -Match '"width":"Full"'
        $json | Should -Match '"type":"Container"'
        $json | Should -Match '"type":"ColumnSet"'
        $json | Should -Match '"type":"Column"'
        $json | Should -Match '"targetElements":\["detailsBlock"\]'
        $json | Should -Match '"type":"ImageSet"'
        $json | Should -Match '"type":"Media"'
        $json | Should -Match '"type":"RichTextBlock"'
        $json | Should -Match '"type":"Action.Submit"'
        $json | Should -Match '"type":"Action.ShowCard"'
        $json | Should -Match '"card":\{"\$schema":"http://adaptivecards.io/schemas/adaptive-card.json"'
        $json | Should -Match '"type":"Action.ToggleVisibility"'
        $json | Should -Match '"type":"mention"'
        $json | Should -Match '"url":"https://example.test/build/42"'
    }

    It 'creates standard and workflow webhook targets' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $incoming = New-TeamsWebhookTarget -Uri 'https://example.test/incoming'
        $workflow = New-TeamsWebhookTarget -Uri 'https://example.test/workflow' -Workflow

        $incoming.DeliveryMethod.ToString() | Should -Be 'IncomingWebhook'
        $workflow.DeliveryMethod.ToString() | Should -Be 'WorkflowWebhook'
    }

    It 'renders connector-card JSON from typed Teams message cmdlets' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42' -Summary 'Build summary' -Color DodgerBlue -HideOriginalBody -Sections @(
            New-TeamsSection -Title 'Build summary' -ActivityText 'Pipeline failed' -ActivityDetails @(
                New-TeamsFact -Name 'Status' -Value 'Failed'
            ) -Buttons @(
                New-TeamsButton -Name 'Open build' -Link 'https://example.test/build/42' -Type OpenUri
            )
        )

        $json = $message | ConvertTo-TeamsJson

        $json | Should -Match '"themeColor":"#1E90FF"'
        $json | Should -Match '"hideOriginalBody":true'
        $json | Should -Match '"title":"Build failed"'
        $json | Should -Match '"sections":\['
        $json | Should -Match '"name":"Status"'
        $json | Should -Match '"@type":"OpenURI"'
    }

    It 'renders connector-card JSON when connector-only fields are set without sections' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42' -Color AlbescentWhite -HideOriginalBody
        $json = $message | ConvertTo-TeamsJson

        $json | Should -Match '"themeColor":"#E3DAC9"'
        $json | Should -Match '"hideOriginalBody":true'
        $json | Should -Match '"title":"Build failed"'
        $json | Should -Match '"text":"Pipeline 42"'
    }

    It 'treats null sections input as empty when building a typed Teams message' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $sections = $null
        { $message = New-TeamsMessage -Title 'Build failed' -Sections $sections } | Should -Not -Throw
        $message.Sections.Count | Should -Be 0
        $message.UseConnectorCardFormat | Should -BeFalse
    }

    It 'renders typed wrapper-card objects through ConvertTo-TeamsJson' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $heroCard = New-TeamsHeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text 'Monorail text' -Images @(
            New-TeamsCardImage -Url 'https://example.test/monorail.jpg' -AlternateText 'Monorail'
        ) -Buttons @(
            New-CardListButton -Type OpenUrl -Title 'Official website' -Value 'https://example.test'
        )
        $thumbnailCard = New-TeamsThumbnailCard -Title 'Bender' -SubTitle 'robot' -Text 'Futurama' -Images @(
            New-TeamsCardImage -Url 'https://example.test/bender.png' -AlternateText 'Bender'
        ) -Buttons @(
            New-CardListButton -Type ImBack -Title 'Thumbs Up' -Value 'I like it'
        )
        $listCard = New-TeamsListCard -Title 'Card Title' -Items @(
            New-CardListItem -Type File -Title 'Report' -SubTitle 'teams > new > design' -TapType OpenUrl -TapValue 'https://contoso.example/report.xlsx' -TapAction editOnline
            New-CardListItem -Type Person -Title 'John Doe' -SubTitle 'Manager' -TapType ImBack -TapValue 'JohnDoe@contoso.com' -TapAction whois
        ) -Buttons @(
            New-CardListButton -Type OpenUrl -Title 'Show' -Value 'https://evotec.xyz'
        )

        $heroJson = $heroCard | ConvertTo-TeamsJson
        $thumbnailJson = $thumbnailCard | ConvertTo-TeamsJson
        $listJson = $listCard | ConvertTo-TeamsJson

        $heroJson | Should -Match '"contentType":"application/vnd.microsoft.card.hero"'
        $heroJson | Should -Match '"alt":"Monorail"'
        $thumbnailJson | Should -Match '"contentType":"application/vnd.microsoft.card.thumbnail"'
        $thumbnailJson | Should -Match '"type":"imBack"'
        $listJson | Should -Match '"contentType":"application/vnd.microsoft.teams.card.list"'
        $listJson | Should -Match '"value":"editOnline https://contoso.example/report.xlsx"'
        $listJson | Should -Match '"value":"whois JohnDoe@contoso.com"'
    }

    It 'wraps typed wrapper-card JSON through Send-TeamsMessageBody' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $heroCard = New-TeamsHeroCard -Title 'Seattle Center Monorail' -Images @(
            New-TeamsCardImage -Url 'https://example.test/monorail.jpg'
        )
        $wrapped = $heroCard |
            ConvertTo-TeamsJson |
            Send-TeamsMessageBody -Uri 'https://example.test/webhook' -Wrap -Supress:$false -WhatIf

        $wrapped | Should -Match '"type":"message"'
        $wrapped | Should -Match '"attachments":\['
        $wrapped | Should -Match '"contentType":"application/vnd.microsoft.card.hero"'
    }

    It 'supports Send-TeamsMessage in WhatIf mode with typed input' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $message = New-TeamsMessage -Text 'Hello from TeamsX'
        $target = New-TeamsWebhookTarget -Uri 'https://example.test/webhook'

        { Send-TeamsMessage -Message $message -Target $target -WhatIf } | Should -Not -Throw
    }

    It 'supports Send-TeamsMessage in WhatIf mode with typed wrapper-card input' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        $heroCard = New-TeamsHeroCard -Title 'Seattle Center Monorail' -Images @(
            New-TeamsCardImage -Url 'https://example.test/monorail.jpg'
        )
        $thumbnailCard = New-TeamsThumbnailCard -Title 'Bender' -Images @(
            New-TeamsCardImage -Url 'https://example.test/bender.png'
        )
        $listCard = New-TeamsListCard -Title 'Card Title' -Items @(
            New-CardListItem -Type ResultItem -Title 'Report' -SubTitle 'teams > new > design'
        )
        $target = New-TeamsWebhookTarget -Uri 'https://example.test/webhook'

        { Send-TeamsMessage -HeroCard $heroCard -Target $target -WhatIf } | Should -Not -Throw
        { Send-TeamsMessage -ThumbnailCard $thumbnailCard -Target $target -WhatIf } | Should -Not -Throw
        { Send-TeamsMessage -ListCard $listCard -Target $target -WhatIf } | Should -Not -Throw
    }

    It 'exposes the migrated Send-TeamsMessage cmdlet as the active public command' {
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

        (Get-Command Send-TeamsMessage).CommandType | Should -Be 'Cmdlet'
        (Get-Command Send-TeamsMessage).Source | Should -Be 'PSTeams'
    }
}
