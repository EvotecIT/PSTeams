---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveRichTextBlock
## SYNOPSIS
New-TeamsAdaptiveRichTextBlock -Text <string[]> [-Color <string[]>] [-Subtle <bool[]>] [-Size <string[]>] [-Weight <string[]>] [-Highlight <bool[]>] [-Italic <bool[]>] [-StrikeThrough <bool[]>] [-FontType <string[]>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

New-TeamsAdaptiveRichTextBlock -Inlines <TeamsAdaptiveTextRun[]> [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

## SYNTAX
### Text
```powershell
New-TeamsAdaptiveRichTextBlock -Text <string[]> [-Color <string[]>] [-Subtle <bool[]>] [-Size <string[]>] [-Weight <string[]>] [-Highlight <bool[]>] [-Italic <bool[]>] [-StrikeThrough <bool[]>] [-FontType <string[]>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]
```

### Inline
```powershell
New-TeamsAdaptiveRichTextBlock -Inlines <TeamsAdaptiveTextRun[]> [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveRichTextBlock -Text <string[]> [-Color <string[]>] [-Subtle <bool[]>] [-Size <string[]>] [-Weight <string[]>] [-Highlight <bool[]>] [-Italic <bool[]>] [-StrikeThrough <bool[]>] [-FontType <string[]>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

New-TeamsAdaptiveRichTextBlock -Inlines <TeamsAdaptiveTextRun[]> [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveRichTextBlock -Inlines @('Value')
```


### EXAMPLE 2
```powershell
New-TeamsAdaptiveRichTextBlock -Text @('Value')
```


## PARAMETERS

### -Color
Specifies one or more values for color.

```yaml
Type: String[]
Parameter Sets: Text
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
Parameter Sets: Text
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
Parameter Sets: Text, Inline
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
Parameter Sets: Text, Inline
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Highlight
Specifies one or more values for highlight.

```yaml
Type: Boolean[]
Parameter Sets: Text
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
Parameter Sets: Text, Inline
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
Parameter Sets: Text, Inline
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Inlines
Specifies one or more values for inlines.

```yaml
Type: TeamsAdaptiveTextRun[]
Parameter Sets: Inline
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Italic
Specifies one or more values for italic.

```yaml
Type: Boolean[]
Parameter Sets: Text
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
Parameter Sets: Text, Inline
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Specifies one or more values for size.

```yaml
Type: String[]
Parameter Sets: Text
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
Parameter Sets: Text, Inline
Aliases: None
Possible values: None, Small, Default, Medium, Large, ExtraLarge, Padding

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StrikeThrough
Specifies one or more values for strike through.

```yaml
Type: Boolean[]
Parameter Sets: Text
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Subtle
Specifies one or more values for subtle.

```yaml
Type: Boolean[]
Parameter Sets: Text
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Specifies one or more values for text.

```yaml
Type: String[]
Parameter Sets: Text
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Weight
Specifies one or more values for weight.

```yaml
Type: String[]
Parameter Sets: Text
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

- `TeamsX.TeamsAdaptiveRichTextBlock`

## RELATED LINKS

- None
