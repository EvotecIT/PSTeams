---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Get-DiscordMessage
## SYNOPSIS
Retrieves an application-owned Discord message through a bot or owning webhook.

## SYNTAX
### Bot (Default)
```powershell
Get-DiscordMessage [-Reference] <MessageReference> -Connection <DiscordConnection> [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [<CommonParameters>]
```

### Webhook
```powershell
Get-DiscordMessage [-Reference] <MessageReference> -WebhookTarget <DiscordMessageTarget> [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves an application-owned Discord message through a bot or owning webhook.

## EXAMPLES

### EXAMPLE 1
```powershell
$connection = New-DiscordConnection -BotToken (Read-Host -AsSecureString); $target = New-DiscordChannelTarget -ChannelId '123456789012345678'; $message = New-DiscordMessage -Content 'Current status'; $reference = (Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Get-DiscordMessage -Reference $reference -Connection $connection
```


### EXAMPLE 2
```powershell
$target = New-DiscordWebhookTarget -Uri $webhookUri; $message = New-DiscordMessage -Content 'Current status'; $reference = (Send-DiscordMessage -Message $message -Target $target -PassThru).Reference; Get-DiscordMessage -Reference $reference -WebhookTarget $target
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
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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
Default value: 100 (valid range: 1-3600)
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

- `MessageX.Core.MessageReference`

## OUTPUTS

- `MessageX.Discord.DiscordRetrievedMessage`

## RELATED LINKS

- None
