Describe 'MessageX Slack PowerShell surface' {
    BeforeEach {
        Get-Module PSTeams, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force
    }

    It 'creates Block Kit messages and renders provider-native JSON' {
        $fields = @(
            New-SlackText -PlainText 'Pipeline 42'
            New-SlackText -Markdown '*Failed*'
        )
        $blocks = @(
            New-SlackSection -Markdown '*Build failed*' -Fields $fields -BlockId 'summary' -Expand
            New-SlackDivider -BlockId 'separator'
        )
        $message = New-SlackMessage -Text 'Build failed' -Blocks $blocks -ThreadTimestamp '1712345678.123456' -ReplyBroadcast -UnfurlLinks:$false
        $target = New-SlackConversationTarget -ConversationId 'C0123456789' -DisplayName 'Release alerts'

        $json = $message | ConvertTo-SlackJson -Target $target
        $payload = $json | ConvertFrom-Json

        $payload.channel | Should -Be 'C0123456789'
        $payload.text | Should -Be 'Build failed'
        $payload.thread_ts | Should -Be '1712345678.123456'
        $payload.reply_broadcast | Should -BeTrue
        $payload.unfurl_links | Should -BeFalse
        $payload.blocks[0].type | Should -Be 'section'
        $payload.blocks[0].text.type | Should -Be 'mrkdwn'
        $payload.blocks[0].fields.Count | Should -Be 2
        $payload.blocks[1].type | Should -Be 'divider'
    }

    It 'creates a token-safe authenticated connection' {
        $secureToken = ConvertTo-SecureString 'xoxb-secret-token' -AsPlainText -Force
        $connection = New-SlackConnection -BotToken $secureToken -WorkspaceId 'T0123'

        $connection.WorkspaceId | Should -Be 'T0123'
        $connection.Capabilities.ToString() | Should -Match 'Send'
        $connection.ToString() | Should -Not -Match 'secret-token'
        $connection.PSObject.Properties.Name | Should -Not -Contain 'BotToken'
    }

    It 'supports simple webhook and conversation WhatIf flows' {
        $secureToken = ConvertTo-SecureString 'xoxb-secret-token' -AsPlainText -Force
        $connection = New-SlackConnection -BotToken $secureToken

        { Send-SlackMessage -WebhookText 'hello' -WebhookUri 'https://hooks.slack.com/services/T/B/secret' -WhatIf } |
            Should -Not -Throw
        { Send-SlackMessage -ConversationText 'hello' -ConversationId 'C0123456789' -Connection $connection -WhatIf } |
            Should -Not -Throw
    }

    It 'supports typed webhook and conversation WhatIf flows' {
        $message = New-SlackMessage -Text 'hello'
        $webhookTarget = New-SlackWebhookTarget -Uri 'https://hooks.slack.com/services/T/B/secret'
        $conversationTarget = New-SlackConversationTarget -ConversationId 'C0123456789'
        $secureToken = ConvertTo-SecureString 'xoxb-secret-token' -AsPlainText -Force
        $connection = New-SlackConnection -BotToken $secureToken

        { Send-SlackMessage -Message $message -Target $webhookTarget -WhatIf } | Should -Not -Throw
        { Send-SlackMessage -Message $message -Target $conversationTarget -Connection $connection -WhatIf } | Should -Not -Throw
    }

    It 'does not expose webhook credentials through target properties or labels' {
        $target = New-SlackWebhookTarget -Uri 'https://hooks.slack.com/services/T/B/secret-token'

        $target.PSObject.Properties.Name | Should -Not -Contain 'WebhookUri'
        $target.ToString() | Should -Not -Match 'secret-token'
    }

    It 'requires an authenticated connection for conversation targets' {
        $message = New-SlackMessage -Text 'hello'
        $target = New-SlackConversationTarget -ConversationId 'C0123456789'

        { Send-SlackMessage -Message $message -Target $target -WhatIf -ErrorAction Stop } |
            Should -Throw -ErrorId 'SlackConnectionRequired,MessageX.PowerShell.CmdletSendSlackMessage'
    }

    It 'keeps raw Slack provider bodies out of PowerShell delivery errors' {
        $result = [MessageX.Slack.SlackDeliveryResult]::new()
        $result.StatusCode = 429
        $result.ErrorKind = [MessageX.Core.MessageErrorKind]::RateLimited
        $result.ErrorMessage = 'Slack request was rate limited.'
        $result.CorrelationId = 'slack-request-42'
        $result.RetryAfter = [TimeSpan]::FromSeconds(30)
        $result.Target = 'Release alerts'
        $result.ResponseBody = 'rejected xoxb-secret-token'

        $supportType = [MessageX.PowerShell.CmdletSendSlackMessage].Assembly.GetType(
            'MessageX.PowerShell.SlackPowerShellDeliverySupport',
            $true)
        $flags = [System.Reflection.BindingFlags]'Public, Static'
        $method = $supportType.GetMethod('CreateDeliveryFailureError', $flags)
        $errorRecord = $method.Invoke($null, [object[]]@($result, 'Send-SlackMessage'))

        $errorRecord.ErrorDetails.Message | Should -Match 'RateLimited'
        $errorRecord.ErrorDetails.Message | Should -Match 'slack-request-42'
        $errorRecord.ErrorDetails.Message | Should -Not -Match 'secret-token'
        $errorRecord.Exception.ToString() | Should -Not -Match 'secret-token'
    }

    It 'exports each Slack cmdlet from PSTeams' {
        $expected = @(
            'ConvertTo-SlackJson'
            'New-SlackConnection'
            'New-SlackConversationTarget'
            'New-SlackDivider'
            'New-SlackMessage'
            'New-SlackSection'
            'New-SlackText'
            'New-SlackWebhookTarget'
            'Send-SlackMessage'
        )

        foreach ($name in $expected) {
            (Get-Command $name -Module PSTeams).CommandType | Should -Be 'Cmdlet'
        }
    }
}
