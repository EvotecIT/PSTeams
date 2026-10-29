---
external help file: MessageX-help.xml
Module Name: MessageX
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveFactSet
## SYNOPSIS
Groups label/value facts in a Teams Adaptive Card.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveFactSet [[-Facts] <scriptblock>] [<CommonParameters>]
```

## DESCRIPTION
Groups label/value facts in a Teams Adaptive Card.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveFactSet -Facts { New-TeamsAdaptiveFact -Title 'Incident ID' -Value 'inc-42' }
```


## PARAMETERS

### -Facts
Specifies a value for facts.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
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

- `MessageX.Teams.TeamsAdaptiveFactSet`

## RELATED LINKS

- None
