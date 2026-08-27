---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordMessage
## SYNOPSIS
Creates a provider-native Discord message.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordMessage [[-Content] <string>] [-Embeds <DiscordEmbed[]>] [-Attachments <DiscordAttachment[]>] [-Components <DiscordActionRow[]>] [-AllowedMentions <DiscordAllowedMentions>] [-ReplyToMessageId <string>] [-AllowMissingReply] [-Nonce <string>] [-EnforceNonce] [-WebhookUsername <string>] [-WebhookAvatarUrl <uri>] [-TextToSpeech] [<CommonParameters>]
```

## DESCRIPTION
Creates a provider-native Discord message.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordMessage -AllowedMentions 'Value'
```


## PARAMETERS

### -AllowedMentions
Explicit mention policy. Defaults to notifying nobody.

```yaml
Type: DiscordAllowedMentions
Parameter Sets: __AllParameterSets
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
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Attachments
Files uploaded with the message.

```yaml
Type: DiscordAttachment[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Components
Interactive Discord action rows.

```yaml
Type: DiscordActionRow[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Content
Plain or Discord-markdown message content.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Embeds
Rich Discord embeds.

```yaml
Type: DiscordEmbed[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EnforceNonce
Asks Discord to enforce nonce uniqueness for recent messages.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Nonce
Optional nonce used for correlation or deduplication.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplyToMessageId
Existing message identifier when creating a reply.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextToSpeech
Requests text-to-speech output.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookAvatarUrl
Optional incoming-webhook avatar override.

```yaml
Type: Uri
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebhookUsername
Optional incoming-webhook username override.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Discord.DiscordMessageRequest`

## RELATED LINKS

- None
