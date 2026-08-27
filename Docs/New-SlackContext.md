---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackContext
## SYNOPSIS
Creates a Slack Block Kit text context row.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackContext [-Elements] <SlackTextObject[]> [-BlockId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit text context row.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackContext -Elements (New-SlackText -Markdown '*Environment:* Production') -BlockId 'deployment-context'
```


## PARAMETERS

### -BlockId
Optional unique block identifier.

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

### -Elements
Plain-text or mrkdwn context elements.

```yaml
Type: SlackTextObject[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackContextBlock`

## RELATED LINKS

- None
