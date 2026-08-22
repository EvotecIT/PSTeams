---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# ConvertTo-SlackJson
## SYNOPSIS
Serializes a typed Slack message using the exact provider payload contract.

## SYNTAX
### __AllParameterSets
```powershell
ConvertTo-SlackJson [-Message] <SlackMessageRequest> [-Target] <SlackMessageTarget> [<CommonParameters>]
```

## DESCRIPTION
Serializes a typed Slack message using the exact provider payload contract.

## EXAMPLES

### EXAMPLE 1
```powershell
ConvertTo-SlackJson -Message 'Value'
```


## PARAMETERS

### -Message
Slack message to serialize.

```yaml
Type: SlackMessageRequest
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Target
Slack target whose transport determines the payload envelope.

```yaml
Type: SlackMessageTarget
Parameter Sets: __AllParameterSets
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

- `System.String`

## RELATED LINKS

- None
