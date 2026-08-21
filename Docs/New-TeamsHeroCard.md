---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsHeroCard
## SYNOPSIS
New-TeamsHeroCard [-Title <string>] [-SubTitle <string>] [-Text <string>] [-Images <TeamsCardImage[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsHeroCard [-Title <string>] [-SubTitle <string>] [-Text <string>] [-Images <TeamsCardImage[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsHeroCard [-Title <string>] [-SubTitle <string>] [-Text <string>] [-Images <TeamsCardImage[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsHeroCard -Buttons @('Value')
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

### -Images
Specifies one or more values for images.

```yaml
Type: TeamsCardImage[]
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

### -Text
Specifies a value for text.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsHeroCard`

## RELATED LINKS

- None
