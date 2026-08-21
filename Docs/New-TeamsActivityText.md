---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsActivityText
## SYNOPSIS
Creates a typed activity-text directive for connector-card sections.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsActivityText [[-Text] <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a typed activity-text directive for connector-card sections.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsActivityText -Text 'Value'
```


## PARAMETERS

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

- `MessageX.Teams.TeamsMessageSectionDirective`

## RELATED LINKS

- None
