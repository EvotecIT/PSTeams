---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# ConvertTo-TeamsFact
## SYNOPSIS
Converts dictionaries and PowerShell objects into Teams facts.

## SYNTAX
### __AllParameterSets
```powershell
ConvertTo-TeamsFact [-InputObject] <Object> [<CommonParameters>]
```

## DESCRIPTION
Converts dictionaries and PowerShell objects into Teams facts.

## EXAMPLES

### EXAMPLE 1
```powershell
ConvertTo-TeamsFact -InputObject 'Value'
```


## PARAMETERS

### -InputObject
Specifies a value for input object.

```yaml
Type: Object
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `System.Object`

## OUTPUTS

- `MessageX.Teams.TeamsMessageFact`

## RELATED LINKS

- None
