Describe 'MessageX Teams Workflow cmdlets' {
    BeforeEach {
        Get-Module MessageX, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\MessageX\MessageX.psd1" -Force
    }

    It 'creates a send-only Workflow target by default' {
        $target = New-TeamsWebhookTarget -Uri 'https://example.test/workflow/secret' -Destination Channel
        $target.DeliveryMethod.ToString() | Should -Be 'WorkflowWebhook'
        $target.WorkflowDestination.ToString() | Should -Be 'Channel'
        $target.Capabilities.ToString() | Should -Be 'Send'
        $target.ToString() | Should -Not -Match 'secret'
    }

    It 'renders a Workflow Adaptive Card with a report link and plain text fallback' {
        $card = New-TeamsAdaptiveCard -Body {
            New-TeamsAdaptiveTextBlock -Text 'Build failed'
            New-TeamsAdaptiveFactSet -Facts {
                New-TeamsAdaptiveFact -Title 'Incident ID' -Value 'inc-42'
            }
        } -Actions {
            New-TeamsAdaptiveOpenUrlAction -Title 'Open report' -Url 'https://example.test/reports/inc-42'
        } -FallbackText 'Build failed. Incident inc-42. Open https://example.test/reports/inc-42'
        $json = New-TeamsMessage -AdaptiveCard $card -Summary 'Build failed' | ConvertTo-TeamsJson

        $payload = $json | ConvertFrom-Json
        $payload.type | Should -Be 'message'
        $payload.attachments[0].contentType | Should -Be 'application/vnd.microsoft.card.adaptive'
        $payload.attachments[0].content.actions[0].type | Should -Be 'Action.OpenUrl'
        $payload.attachments[0].content.fallbackText | Should -Match 'inc-42'
        $payload.PSObject.Properties.Name | Should -Not -Contain 'sections'
    }

    It 'keeps underscores literal in the JSON text' {
        $card = New-TeamsAdaptiveCard -Body {
            New-TeamsAdaptiveTextBlock -Text 'server_name'
        }
        $json = New-TeamsMessage -AdaptiveCard $card | ConvertTo-TeamsJson
        $json | Should -Match 'server_name'
    }

    It 'composes nested elements with PowerShell script blocks' {
        $card = New-TeamsAdaptiveCard -Body {
            New-TeamsAdaptiveContainer -Items {
                New-TeamsAdaptiveColumnSet -Columns {
                    New-TeamsAdaptiveColumn -Items {
                        New-TeamsAdaptiveTextBlock -Text 'Service failed'
                    }
                }
                New-TeamsAdaptiveActionSet -Actions {
                    New-TeamsAdaptiveOpenUrlAction -Title 'Open report' -Url 'https://example.test/report'
                }
            }
            New-TeamsAdaptiveImageSet -Images {
                New-TeamsAdaptiveImage -Url 'https://example.test/status.png' -AltText 'Status'
            }
        }
        $json = New-TeamsMessage -AdaptiveCard $card | ConvertTo-TeamsJson
        $json | Should -Match '"type":"ColumnSet"'
        $json | Should -Match '"type":"ImageSet"'
        $json | Should -Match '"type":"Action.OpenUrl"'
    }

    It 'rejects unrelated values from nested script blocks' {
        { New-TeamsAdaptiveCard -Body { 'not an adaptive element' } -ErrorAction Stop } | Should -Throw
    }

    It 'supports WhatIf with enterprise transport options and no network send' {
        $target = New-TeamsWebhookTarget -Uri 'https://example.test/workflow/secret'
        $card = New-TeamsAdaptiveCard -Body { New-TeamsAdaptiveTextBlock -Text 'Build failed' }
        $message = New-TeamsMessage -AdaptiveCard $card
        { Send-TeamsMessage -Message $message -Target $target -Proxy 'http://proxy.example.test:8080' -TimeoutSeconds 15 -UserAgent 'MessageX.Tests/1.0' -WhatIf } | Should -Not -Throw
    }

    It 'exposes proxy and timeout parameters on the Workflow send command' {
        $parameters = (Get-Command Send-TeamsMessage).Parameters
        $parameters.ContainsKey('Proxy') | Should -BeTrue
        $parameters.ContainsKey('TimeoutSeconds') | Should -BeTrue
        $parameters.ContainsKey('UserAgent') | Should -BeTrue
    }
}
