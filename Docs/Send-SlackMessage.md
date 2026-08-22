---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Send-SlackMessage
## SYNOPSIS
Sends simple or typed messages through Slack incoming webhooks or the authenticated Web API.

## SYNTAX
### Typed (Default)
```powershell
Send-SlackMessage [-Message] <SlackMessageRequest> [-Target] <SlackMessageTarget> [-PassThru] [-Connection <SlackConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### WebhookText
```powershell
Send-SlackMessage [-WebhookText] <string> [-WebhookUri] <uri> [-PassThru] [-Connection <SlackConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ConversationText
```powershell
Send-SlackMessage [-ConversationText] <string> [-ConversationId] <string> [-ThreadTimestamp <string>] [-ReplyBroadcast] [-PassThru] [-Connection <SlackConnection>] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sends simple or typed messages through Slack incoming webhooks or the authenticated Web API.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-SlackMessage -Connection 'Value'
```


## PARAMETERS

### -Connection
Authenticated Slack bot connection used for Web API targets.

```yaml
Type: SlackConnection
Parameter Sets: Typed, WebhookText, ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConversationId
Slack channel, direct-message, multiparty-message, or user identifier.

```yaml
Type: String
Parameter Sets: ConversationText
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConversationText
Simple message text sent through the Slack Web API.

```yaml
Type: String
Parameter Sets: ConversationText
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Message
Typed Slack message.

```yaml
Type: SlackMessageRequest
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
Parameter Sets: Typed, WebhookText, ConversationText
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
Parameter Sets: Typed, WebhookText, ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplyBroadcast
Broadcasts a simple conversation reply to the parent conversation.

```yaml
Type: SwitchParameter
Parameter Sets: ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Typed Slack webhook or conversation target.

```yaml
Type: SlackMessageTarget
Parameter Sets: Typed
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThreadTimestamp
Parent Slack timestamp when sending a simple conversation reply.

```yaml
Type: String
Parameter Sets: ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
HTTP request timeout in seconds.

```yaml
Type: Int32
Parameter Sets: Typed, WebhookText, ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: 100 (valid range: 1-3600)
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserAgent
Optional product user-agent sent with provider requests.

```yaml
Type: String
Parameter Sets: Typed, WebhookText, ConversationText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookText
Simple message text sent to an incoming webhook.

```yaml
Type: String
Parameter Sets: WebhookText
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookUri
Secret incoming-webhook URI used by the simple webhook flow.

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

- `MessageX.Slack.SlackMessageRequest`

## OUTPUTS

- `MessageX.Slack.SlackDeliveryResult`

## RELATED LINKS

- None
