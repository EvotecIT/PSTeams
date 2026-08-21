---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsList
## SYNOPSIS
Builds a legacy list fact from typed list items.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsList [[-List] <scriptblock>] [[-Name] <string>] [<CommonParameters>]
```

## DESCRIPTION
Builds a legacy list fact from typed list items.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsList -Name 'Name'
```


## PARAMETERS

### -List
Specifies a value for list.

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

### -Name
Specifies a value for name.

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
