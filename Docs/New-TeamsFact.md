---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsFact
## SYNOPSIS
Creates a connector-card fact item.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsFact [[-Name] <string>] [[-Value] <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a connector-card fact item.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsFact -Name 'Status' -Value 'Failed'
```


## PARAMETERS

### -Name
Fact label displayed in the section.

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
Fact value displayed beside the label.

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

- `MessageX.Teams.TeamsMessageFact`

## RELATED LINKS

- None
