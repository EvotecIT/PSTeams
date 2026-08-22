---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackDivider
## SYNOPSIS
Creates a Slack Block Kit divider.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackDivider [-BlockId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit divider.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackDivider -BlockId 'Value'
```


## PARAMETERS

### -BlockId
Optional unique Slack block identifier.

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

- `MessageX.Slack.SlackDividerBlock`

## RELATED LINKS

- None
