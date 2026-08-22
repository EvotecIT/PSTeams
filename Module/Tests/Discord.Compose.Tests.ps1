Describe 'MessageX Discord PowerShell surface' {
    BeforeAll {
        Get-Module PSTeams, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force
    }

    It 'builds safe provider-native embeds and JSON' {
        $message = New-DiscordMessage -Content 'Build <@123456789012345678> failed' -Embeds @(
            New-DiscordSection -Title 'Build failed' -Description 'Pipeline 42' -Color 0xCC3300 -Fields @(
                New-DiscordFact -Name 'Environment' -Value 'Production' -Inline
            ) -Footer (New-DiscordFooter -Text 'MessageX')
        )
        $target = New-DiscordChannelTarget -ChannelId '223456789012345678' -GuildId '323456789012345678'
        $json = $message | ConvertTo-DiscordJson -Target $target

        $json | Should -Match '"title":"Build failed"'
        $json | Should -Match '"parse":\[\]'
        $json | Should -Match '"replied_user":false'
        $target.ThreadId | Should -BeNullOrEmpty
    }

    It 'preserves legacy PSDiscord builder names as aliases' {
        (Get-Command New-DiscordEmbed).CommandType | Should -Be 'Alias'
        (Get-Command New-DiscordField).CommandType | Should -Be 'Alias'
        (Get-Command New-DiscordThumbnail).CommandType | Should -Be 'Alias'
    }

    It 'keeps spoiler filenames and attachment URLs aligned' {
        $attachment = New-DiscordAttachment -Bytes ([byte[]](1, 2, 3)) -FileName 'report.png' -Spoiler
        $message = New-DiscordMessage -Attachments $attachment -Embeds @(
            New-DiscordSection -Title 'Report' -Image (New-DiscordImage -Url 'attachment://report.png')
        )
        $target = New-DiscordChannelTarget -ChannelId '223456789012345678'
        $payload = $message | ConvertTo-DiscordJson -Target $target | ConvertFrom-Json

        $payload.attachments[0].filename | Should -Be 'SPOILER_report.png'
        $payload.attachments[0].PSObject.Properties.Name | Should -Not -Contain 'is_spoiler'
        $payload.embeds[0].image.url | Should -Be 'attachment://SPOILER_report.png'
    }

    It 'creates webhook, channel, thread, and direct-message targets without exposing secrets' {
        $webhook = New-DiscordWebhookTarget -Uri 'https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456' -ThreadId '223456789012345678'
        $channel = New-DiscordChannelTarget -ChannelId '323456789012345678'
        $thread = New-DiscordThreadTarget -ThreadId '423456789012345678'
        $direct = New-DiscordDirectMessageTarget -UserId '523456789012345678'

        $webhook.PSObject.Properties.Name | Should -Not -Contain 'WebhookUri'
        $webhook.ToString() | Should -Not -Match 'abcdefghijklmnopqrstuvwxyz'
        $channel.DeliveryMethod.ToString() | Should -Be 'BotChannel'
        $thread.DeliveryMethod.ToString() | Should -Be 'BotThread'
        $direct.DeliveryMethod.ToString() | Should -Be 'BotDirectMessage'
    }

    It 'creates secure bot connections without exposing the token' {
        $token = ConvertTo-SecureString 'discord-super-secret-token-value' -AsPlainText -Force
        $connection = New-DiscordConnection -BotToken $token -ApplicationId '623456789012345678'

        $connection.PSObject.Properties.Name | Should -Not -Contain 'BotToken'
        $connection.ToString() | Should -Not -Match 'secret'
    }

    It 'supports every send parameter set under WhatIf without network access' {
        $token = ConvertTo-SecureString 'discord-super-secret-token-value' -AsPlainText -Force
        $connection = New-DiscordConnection -BotToken $token
        $message = New-DiscordMessage -Content 'hello'
        $target = New-DiscordChannelTarget -ChannelId '123456789012345678'

        { Send-DiscordMessage -Message $message -Target $target -Connection $connection -WhatIf } | Should -Not -Throw
        { Send-DiscordMessage -Text 'hello' -WebhookUri 'https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456' -WhatIf } | Should -Not -Throw
        { Send-DiscordMessage -Text 'hello' -ChannelId '123456789012345678' -Connection $connection -WhatIf } | Should -Not -Throw
        { Send-DiscordMessage -Text 'hello' -ThreadId '223456789012345678' -Connection $connection -WhatIf } | Should -Not -Throw
        { Send-DiscordMessage -Text 'hello' -UserId '323456789012345678' -Connection $connection -WhatIf } | Should -Not -Throw
    }

    It 'keeps typed messages as the single owner of their mention policy' {
        $typedParameters = (Get-Command Send-DiscordMessage).ParameterSets |
            Where-Object Name -EQ 'Typed' |
            ForEach-Object Parameters |
            ForEach-Object Name

        $typedParameters | Should -Not -Contain 'AllowedMentions'
    }

    It 'requires a bot connection for authenticated targets' {
        $message = New-DiscordMessage -Content 'hello'
        $target = New-DiscordChannelTarget -ChannelId '123456789012345678'

        { Send-DiscordMessage -Message $message -Target $target -WhatIf -ErrorAction Stop } |
            Should -Throw -ErrorId 'DiscordConnectionRequired,MessageX.PowerShell.CmdletSendDiscordMessage'
    }

    It 'exports each Discord command and alias from PSTeams' {
        $commands = @(
            'ConvertTo-DiscordJson', 'New-DiscordAllowedMentions', 'New-DiscordAttachment',
            'New-DiscordAuthor', 'New-DiscordChannelTarget', 'New-DiscordConnection',
            'New-DiscordDirectMessageTarget', 'New-DiscordFact', 'New-DiscordFooter',
            'New-DiscordImage', 'New-DiscordMessage', 'New-DiscordSection',
            'New-DiscordThreadTarget', 'New-DiscordWebhookTarget', 'Send-DiscordMessage',
            'Test-DiscordInteractionSignature'
        )
        foreach ($command in $commands) {
            (Get-Command $command).Source | Should -Be 'PSTeams'
        }
    }
}
