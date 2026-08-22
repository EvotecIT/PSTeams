---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Update-DiscordMessage
## SYNOPSIS
Updates an application-owned Discord message through a bot or owning webhook.

## SYNTAX
### Bot (Default)
```powershell
Update-DiscordMessage [-Message] <DiscordMessageRequest> [-Reference] <MessageReference> -Connection <DiscordConnection> [-PassThru] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Webhook
```powershell
Update-DiscordMessage [-Message] <DiscordMessageRequest> [-Reference] <MessageReference> -WebhookTarget <DiscordMessageTarget> [-PassThru] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Updates an application-owned Discord message through a bot or owning webhook.

## EXAMPLES

### EXAMPLE 1
```powershell
Update-DiscordMessage -Connection 'Value'
```


### EXAMPLE 2
```powershell
Update-DiscordMessage -WebhookTarget 'Value'
```


## PARAMETERS

### -Connection
Authenticated Discord bot connection.

```yaml
Type: DiscordConnection
Parameter Sets: Bot
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Message
Replacement Discord message.

```yaml
Type: DiscordMessageRequest
Parameter Sets: Bot, Webhook
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru
Returns the typed operation result.

```yaml
Type: SwitchParameter
Parameter Sets: Bot, Webhook
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
Parameter Sets: Bot, Webhook
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Reference
Durable Discord message reference.

```yaml
Type: MessageReference
Parameter Sets: Bot, Webhook
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
Parameter Sets: Bot, Webhook
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
Parameter Sets: Bot, Webhook
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookTarget
Credential-bearing Discord webhook target kept only in memory.

```yaml
Type: DiscordMessageTarget
Parameter Sets: Webhook
Aliases: None
Possible values:

Required: True
Position: named
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
