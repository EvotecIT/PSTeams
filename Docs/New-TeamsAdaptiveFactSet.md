---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveFactSet
## SYNOPSIS
New-TeamsAdaptiveFactSet [-Facts <TeamsAdaptiveFact[]>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveFactSet [-Facts <TeamsAdaptiveFact[]>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveFactSet [-Facts <TeamsAdaptiveFact[]>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveFactSet -Facts @('Value')
```


## PARAMETERS

### -Facts
Specifies one or more values for facts.

```yaml
Type: TeamsAdaptiveFact[]
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

- `MessageX.Teams.TeamsAdaptiveFactSet`

## RELATED LINKS

- None
