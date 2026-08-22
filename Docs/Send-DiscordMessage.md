---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Send-DiscordMessage
## SYNOPSIS
Sends simple or typed messages through Discord incoming webhooks or authenticated bot REST.

## SYNTAX
### Typed (Default)
```powershell
Send-DiscordMessage [-Message] <DiscordMessageRequest> [-Target] <DiscordMessageTarget> [-PassThru] [-Connection <DiscordConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### WebhookText
```powershell
Send-DiscordMessage [-Text] <string> [-WebhookUri] <uri> [-ThreadId <string>] [-AllowedMentions <DiscordAllowedMentions>] [-PassThru] [-Connection <DiscordConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ChannelText
```powershell
Send-DiscordMessage [-Text] <string> [-ChannelId] <string> [-GuildId <string>] [-ReplyToMessageId <string>] [-AllowMissingReply] [-AllowedMentions <DiscordAllowedMentions>] [-PassThru] [-Connection <DiscordConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ThreadText
```powershell
Send-DiscordMessage [-Text] <string> [-ThreadId] <string> [-GuildId <string>] [-ReplyToMessageId <string>] [-AllowMissingReply] [-AllowedMentions <DiscordAllowedMentions>] [-PassThru] [-Connection <DiscordConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### DirectMessageText
```powershell
Send-DiscordMessage [-Text] <string> [-UserId] <string> [-ReplyToMessageId <string>] [-AllowMissingReply] [-AllowedMentions <DiscordAllowedMentions>] [-PassThru] [-Connection <DiscordConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sends simple or typed messages through Discord incoming webhooks or authenticated bot REST.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-DiscordMessage -AllowedMentions 'Value'
```


## PARAMETERS

### -AllowedMentions
Explicit mention policy. Defaults to notifying nobody.

```yaml
Type: DiscordAllowedMentions
Parameter Sets: WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AllowMissingReply
Allows a reply to proceed if the referenced message no longer exists.

```yaml
Type: SwitchParameter
Parameter Sets: ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ChannelId
Discord channel identifier for the simple bot channel flow.

```yaml
Type: String
Parameter Sets: ChannelText
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Authenticated Discord bot connection used for channel, thread, and direct-message targets.

```yaml
Type: DiscordConnection
Parameter Sets: Typed, WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GuildId
Optional guild identifier retained in channel and thread references.

```yaml
Type: String
Parameter Sets: ChannelText, ThreadText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Message
Typed Discord message.

```yaml
Type: DiscordMessageRequest
Parameter Sets: Typed
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru
Returns the typed delivery result.

```yaml
Type: SwitchParameter
Parameter Sets: Typed, WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Proxy
HTTP proxy used for provider requests.

```yaml
Type: Uri
Parameter Sets: Typed, WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplyToMessageId
Existing message identifier when creating a bot reply.

```yaml
Type: String
Parameter Sets: ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Typed Discord webhook, channel, thread, or direct-message target.

```yaml
Type: DiscordMessageTarget
Parameter Sets: Typed
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Simple message text.

```yaml
Type: String
Parameter Sets: WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThreadId
Discord thread identifier for webhook or bot thread delivery.

```yaml
Type: String
Parameter Sets: WebhookText, ThreadText
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
HTTP request timeout in seconds.

```yaml
Type: Int32
Parameter Sets: Typed, WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserAgent
Optional product user-agent sent with provider requests.

```yaml
Type: String
Parameter Sets: Typed, WebhookText, ChannelText, ThreadText, DirectMessageText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserId
Discord user identifier for the simple direct-message flow.

```yaml
Type: String
Parameter Sets: DirectMessageText
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookUri
Secret incoming-webhook URI for the simple webhook flow.

```yaml
Type: Uri
Parameter Sets: WebhookText
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `MessageX.Discord.DiscordMessageRequest`

## OUTPUTS

- `MessageX.Discord.DiscordDeliveryResult`

## RELATED LINKS

- None
