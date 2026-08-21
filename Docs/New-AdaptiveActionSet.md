---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveActionSet
## SYNOPSIS
Creates a legacy-named adaptive action set backed by the TeamsX model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveActionSet [[-Action] <scriptblock>] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive action set backed by the TeamsX model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveActionSet -Action { }
```


## PARAMETERS

### -Action
Specifies a value for action.

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

- `TeamsX.TeamsAdaptiveActionSet`

## RELATED LINKS

- None
