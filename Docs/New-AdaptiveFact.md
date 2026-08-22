---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveFact
## SYNOPSIS
Creates a legacy-named adaptive fact backed by the MessageX.Teams model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveFact [[-Title] <string>] [[-Value] <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive fact backed by the MessageX.Teams model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveFact -Title 'Value'
```


## PARAMETERS

### -Title
Specifies a value for title.

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

### -Value
Specifies a value for value.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsAdaptiveFact`

## RELATED LINKS

- None
