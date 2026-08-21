---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsListItem
## SYNOPSIS
Creates a typed legacy list item for connector-card facts.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsListItem [[-Text] <string>] [[-Level] <int>] [-Numbered] [<CommonParameters>]
```

## DESCRIPTION
Creates a typed legacy list item for connector-card facts.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsListItem -Level 1
```


## PARAMETERS

### -Level
Specifies a value for level.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Numbered
Specifies the numbered switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Specifies a value for text.

```yaml
Type: String
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

- `TeamsX.TeamsMessageListItem`

## RELATED LINKS

- None
