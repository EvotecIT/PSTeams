---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Add-DiscordReaction
## SYNOPSIS
Adds the authenticated Discord bot's reaction to a message.

## SYNTAX
### __AllParameterSets
```powershell
Add-DiscordReaction [-Reference] <MessageReference> [-Reaction] <string> -Connection <DiscordConnection> [-PassThru] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Adds the authenticated Discord bot's reaction to a message.

## EXAMPLES

### EXAMPLE 1
```powershell
$connection = New-DiscordConnection -BotToken (Read-Host -AsSecureString); $target = New-DiscordChannelTarget -ChannelId '123456789012345678'; $message = New-DiscordMessage -Content 'Review ready'; $reference = (Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Add-DiscordReaction -Reference $reference -Reaction '👀' -Connection $connection
```


## PARAMETERS

### -Connection
Authenticated Discord bot connection.

```yaml
Type: DiscordConnection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Returns the typed operation result.

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

### -Proxy
HTTP proxy used for provider requests.

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

### -Reaction
Unicode emoji or custom emoji coordinate in name:id form.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Reference
Durable Discord message reference.

```yaml
Type: MessageReference
Parameter Sets: __AllParameterSets
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
Parameter Sets: __AllParameterSets
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

- `MessageX.Core.MessageReference`

## OUTPUTS

- `MessageX.Discord.DiscordDeliveryResult`

## RELATED LINKS

- None
