---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-CardListButton
## SYNOPSIS
Creates a button for ListCard, HeroCard, and ThumbnailCard payloads.

## SYNTAX
### __AllParameterSets
```powershell
New-CardListButton [-Type <TeamsCardButtonActionType>] [-Title <string>] [-Value <string>] [-Image <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a button for ListCard, HeroCard, and ThumbnailCard payloads.

## EXAMPLES

### EXAMPLE 1
```powershell
New-CardListButton -Image 'Value'
```


## PARAMETERS

### -Image
Specifies a value for image.

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

### -Value
Specifies a value for value.

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

- `MessageX.Teams.TeamsCardButton`

## RELATED LINKS

- None
