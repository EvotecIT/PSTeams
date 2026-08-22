---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackConversationTarget
## SYNOPSIS
Creates a Slack channel, direct-message, multiparty-message, or user target.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackConversationTarget [-ConversationId] <string> [-DisplayName <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack channel, direct-message, multiparty-message, or user target.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackConversationTarget -ConversationId 'Value'
```


## PARAMETERS

### -ConversationId
Slack provider identifier, such as a channel, conversation, or user ID.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Optional safe display label.

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

- `MessageX.Slack.SlackMessageTarget`

## RELATED LINKS

- None
