---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsListCard
## SYNOPSIS
New-TeamsListCard [-Title <string>] [-Items <TeamsListCardItem[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsListCard [-Title <string>] [-Items <TeamsListCardItem[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsListCard [-Title <string>] [-Items <TeamsListCardItem[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsListCard -Buttons @('Value')
```


## PARAMETERS

### -Buttons
Specifies one or more values for buttons.

```yaml
Type: TeamsCardButton[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Items
Specifies one or more values for items.

```yaml
Type: TeamsListCardItem[]
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsListCard`

## RELATED LINKS

- None
