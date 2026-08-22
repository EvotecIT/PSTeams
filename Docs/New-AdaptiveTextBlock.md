---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveTextBlock
## SYNOPSIS
Creates a legacy-named adaptive text block backed by the TeamsX model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveTextBlock [[-Text] <string>] [-Color <string>] [-FontType <string>] [-HorizontalAlignment <string>] [-Subtle] [-MaximumLines <Int32>] [-Size <string>] [-Weight <string>] [-Highlight] [-Italic] [-StrikeThrough] [-Wrap] [-Height <string>] [-Separator] [-Spacing <string>] [-Id <string>] [-Hidden] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive text block backed by the TeamsX model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveTextBlock -Color 'Value'
```


## PARAMETERS

### -Color
Specifies a value for color.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Accent, Default, Dark, Light, Good, Warning, Attention

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontType
Specifies a value for font type.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Default, Monospace

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

### -Hidden
Specifies the hidden switch.

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

### -Highlight
Specifies the highlight switch.

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

### -Id
Specifies a value for id.

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

### -Italic
Specifies the italic switch.

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

### -MaximumLines
Specifies a value for maximum lines.

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

### -Size
Specifies a value for size.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: FontSize
Possible values: Small, Default, Medium, Large, ExtraLarge

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

### -StrikeThrough
Specifies the strike through switch.

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

### -Subtle
Specifies the subtle switch.

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

### -Weight
Specifies a value for weight.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: FontWeight
Possible values: Lighter, Default, Bolder

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Wrap
Specifies the wrap switch.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `TeamsX.TeamsAdaptiveTextBlock`

## RELATED LINKS

- None
