---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveColumnSet
## SYNOPSIS
New-TeamsAdaptiveColumnSet [-Columns <TeamsAdaptiveColumn[]>] [-Style <string>] [-MinimumHeight <int>] [-Bleed] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveColumnSet [-Columns <TeamsAdaptiveColumn[]>] [-Style <string>] [-MinimumHeight <int>] [-Bleed] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveColumnSet [-Columns <TeamsAdaptiveColumn[]>] [-Style <string>] [-MinimumHeight <int>] [-Bleed] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveColumnSet -Bleed
```


## PARAMETERS

### -Bleed
Specifies the bleed switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
Specifies one or more values for columns.

```yaml
Type: TeamsAdaptiveColumn[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Specifies a value for height.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Stretch, Automatic

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HorizontalAlignment
Specifies a value for horizontal alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Left, Center, Right

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinimumHeight
Specifies a value for minimum height.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Separator
Specifies the separator switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Spacing
Specifies a value for spacing.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Small, Default, Medium, Large, ExtraLarge, Padding

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style
Specifies a value for style.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Accent, Default, Emphasis, Good, Warning, Attention

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

- `MessageX.Teams.TeamsAdaptiveColumnSet`

## RELATED LINKS

- None
