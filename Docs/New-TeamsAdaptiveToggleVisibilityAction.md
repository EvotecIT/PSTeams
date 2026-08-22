---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveToggleVisibilityAction
## SYNOPSIS
New-TeamsAdaptiveToggleVisibilityAction [-Title] <string> [-TargetElementIds] <string[]> [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveToggleVisibilityAction [-Title] <string> [-TargetElementIds] <string[]> [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveToggleVisibilityAction [-Title] <string> [-TargetElementIds] <string[]> [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveToggleVisibilityAction -TargetElementIds @('Value')
```


## PARAMETERS

### -TargetElementIds
Specifies one or more values for target element ids.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Specifies a value for title.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `TeamsX.TeamsAdaptiveToggleVisibilityAction`

## RELATED LINKS

- None
