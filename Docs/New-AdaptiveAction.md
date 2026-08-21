---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveAction
## SYNOPSIS
Creates a legacy-named adaptive action backed by the TeamsX model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveAction [-Body <scriptblock>] [-Actions <scriptblock>] [-Type <string>] [-ActionUrl <string>] [-Title <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive action backed by the TeamsX model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveAction -Actions { }
```


## PARAMETERS

### -Actions
Specifies a value for actions.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ActionUrl
Specifies a value for action url.

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

### -Body
Specifies a value for body.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
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

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Type
Specifies a value for type.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Action.ShowCard, Action.Submit, Action.OpenUrl, Action.ToggleVisibility

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

- `TeamsX.TeamsAdaptiveAction`

## RELATED LINKS

- None
