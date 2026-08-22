---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveTable
## SYNOPSIS
Creates a legacy-named adaptive table by projecting objects into column sets.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveTable [[-DataTable] <Object[]>] [-Width <string>] [-HeaderColor <string>] [-HeaderWeight <string>] [-HeaderSize <string>] [-HeaderHighlight] [-HeaderItalic] [-HeaderStrikeThrough] [-HeaderFontType <string>] [-HeaderSpacing <string>] [-HeaderHorizontalAlignment <string>] [-HeaderHeight <string>] [-HeaderSubtle] [-HeaderMaximumLines <int>] [-Weight <string>] [-Size <string>] [-Color <string>] [-Highlight <bool>] [-Italic <bool>] [-StrikeThrough <bool>] [-FontType <string[]>] [-Spacing <string>] [-HorizontalAlignment <string>] [-Wrap] [-Height <string>] [-Subtle] [-MaximumLines <int>] [-DictionaryAsCustomObject] [-DisableHeaderColumnSeparators] [-DisableRowSeparators] [-DisableColumnSeparators] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive table by projecting objects into column sets.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveTable -Color 'Value'
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

### -DataTable
Specifies one or more values for data table.

```yaml
Type: Object[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DictionaryAsCustomObject
Specifies the dictionary as custom object switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: HashTableAsCustomObject
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisableColumnSeparators
Specifies the disable column separators switch.

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

### -DisableHeaderColumnSeparators
Specifies the disable header column separators switch.

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

### -DisableRowSeparators
Specifies the disable row separators switch.

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

### -FontType
Specifies one or more values for font type.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Default, Monospace

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeaderColor
Specifies a value for header color.

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

### -HeaderFontType
Specifies a value for header font type.

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

### -HeaderHeight
Specifies a value for header height.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: HeaderBlockElementHeight
Possible values: Stretch, Automatic

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeaderHighlight
Specifies the header highlight switch.

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

### -HeaderHorizontalAlignment
Specifies a value for header horizontal alignment.

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

### -HeaderItalic
Specifies the header italic switch.

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

### -HeaderMaximumLines
Specifies a value for header maximum lines.

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

### -HeaderSize
Specifies a value for header size.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: HeaderFontSize
Possible values: Small, Default, Medium, Large, ExtraLarge

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeaderSpacing
Specifies a value for header spacing.

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

### -HeaderStrikeThrough
Specifies the header strike through switch.

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

### -HeaderSubtle
Specifies the header subtle switch.

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

### -HeaderWeight
Specifies a value for header weight.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: HeaderFontWeight
Possible values: Lighter, Default, Bolder

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
Aliases: BlockElementHeight
Possible values: Stretch, Automatic

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Highlight
Specifies a Boolean value for highlight.

```yaml
Type: Boolean
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

### -Italic
Specifies a Boolean value for italic.

```yaml
Type: Boolean
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
Specifies a Boolean value for strike through.

```yaml
Type: Boolean
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

### -Width
Specifies a value for width.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, Stretch

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

- `MessageX.Teams.TeamsAdaptiveColumnSet`

## RELATED LINKS

- None
