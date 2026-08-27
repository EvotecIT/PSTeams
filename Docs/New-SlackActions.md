---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackActions
## SYNOPSIS
Creates a Slack Block Kit actions block.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackActions [-Elements] <SlackBlockElement[]> [-BlockId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit actions block.

## EXAMPLES

### EXAMPLE 1
```powershell
$button = New-SlackButton -Text 'Approve' -ActionId 'approve' -Style Primary; New-SlackActions -Elements $button -BlockId 'approval-actions'
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
Interactive Block Kit elements.

```yaml
Type: SlackBlockElement[]
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

- `MessageX.Slack.SlackActionsBlock`

## RELATED LINKS

- None
