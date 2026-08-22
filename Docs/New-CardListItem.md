---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-CardListItem
## SYNOPSIS
Creates one Teams list-card item.

## SYNTAX
### __AllParameterSets
```powershell
New-CardListItem -Type <TeamsListCardItemKind> [-Icon <string>] [-Title <string>] [-SubTitle <string>] [-TapAction <string>] [-TapType <TeamsCardButtonActionType>] [-TapValue <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates one Teams list-card item.

## EXAMPLES

### EXAMPLE 1
```powershell
New-CardListItem -Type 'Value'
```


## PARAMETERS

### -Icon
Specifies a value for icon.

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

### -SubTitle
Specifies a value for sub title.

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

### -TapAction
Specifies a value for tap action.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: whois, editOnline

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TapType
Specifies a value for tap type.

```yaml
Type: TeamsCardButtonActionType
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: ImBack, OpenUrl, File

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TapValue
Specifies a value for tap value.

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
Type: TeamsListCardItemKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: File, ResultItem, Section, Person

Required: True
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

- `MessageX.Teams.TeamsListCardItem`

## RELATED LINKS

- None
