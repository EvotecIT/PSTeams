---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackMessage
## SYNOPSIS
Creates a provider-native Slack message.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackMessage [[-Text] <string>] [-Blocks <SlackBlock[]>] [-ThreadTimestamp <string>] [-ReplyBroadcast] [-UnfurlLinks <Boolean>] [-UnfurlMedia <Boolean>] [<CommonParameters>]
```

## DESCRIPTION
Creates a provider-native Slack message.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackMessage -Blocks @('Value')
```


## PARAMETERS

### -Blocks
Slack Block Kit blocks.

```yaml
Type: SlackBlock[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplyBroadcast
Broadcasts a thread reply to the conversation.

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

### -Text
Top-level message text and accessibility fallback for blocks.

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

### -ThreadTimestamp
Parent Slack message timestamp for a thread reply.

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

### -UnfurlLinks
Controls link unfurling when explicitly supplied.

```yaml
Type: Boolean
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UnfurlMedia
Controls media unfurling when explicitly supplied.

```yaml
Type: Boolean
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

- `MessageX.Slack.SlackMessageRequest`

## RELATED LINKS

- None
