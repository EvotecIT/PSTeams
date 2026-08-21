---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# ConvertTo-TeamsSection
## SYNOPSIS
Converts dictionaries and PowerShell objects into Teams sections.

## SYNTAX
### __AllParameterSets
```powershell
ConvertTo-TeamsSection [-InputObject] <Object> [[-SectionTitleProperty] <string>] [<CommonParameters>]
```

## DESCRIPTION
Converts dictionaries and PowerShell objects into Teams sections.

## EXAMPLES

### EXAMPLE 1
```powershell
ConvertTo-TeamsSection -InputObject 'Value'
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

### -SectionTitleProperty
Specifies a value for section title property.

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

- `System.Object`

## OUTPUTS

- `MessageX.Teams.TeamsMessageSection`

## RELATED LINKS

- None
