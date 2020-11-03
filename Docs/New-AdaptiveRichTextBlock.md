---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveRichTextBlock

## SYNOPSIS
Defines an array of inlines, allowing for inline text formatting.

## SYNTAX

```
New-AdaptiveRichTextBlock [[-Text] <String[]>] [[-Color] <String[]>] [[-Subtle] <Boolean[]>]
 [[-Size] <String[]>] [[-Weight] <String[]>] [[-Highlight] <Boolean[]>] [[-Italic] <Boolean[]>]
 [[-StrikeThrough] <Boolean[]>] [[-FontType] <String[]>] [[-Spacing] <String>] [-Separator]
 [[-HorizontalAlignment] <String>] [[-Height] <String>] [[-Id] <String>] [-Hidden] [<CommonParameters>]
```

## DESCRIPTION
Defines an array of inlines, allowing for inline text formatting.

## EXAMPLES

### EXAMPLE 1
```
New-AdaptiveRichTextBlock -Text 'This is the first inline.', 'We support colors,', 'both regular and subtle. ', 'Monospace too!' -Color Attention, Default, Warning -StrikeThrough $false, $true, $false -Size ExtraLarge, Default, Medium -Italic $false, $false, $true -Subtle $false, $true, $true
```

## PARAMETERS

### -Text
Text to display.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Controls the color of text elements.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Subtle
Displays text slightly toned down to appear less prominent.

```yaml
Type: Boolean[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Controls size of text.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: FontSize

Required: False
Position: 4
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Weight
Controls the weight of text elements.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: FontWeight

Required: False
Position: 5
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Highlight
Controls the hightlight of text elements

```yaml
Type: Boolean[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Italic
Controls italic of text elements

```yaml
Type: Boolean[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 7
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -StrikeThrough
Controls strikethrough of text elements

```yaml
Type: Boolean[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontType
Type of font to use for rendering

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 9
Default value: @()
Accept pipeline input: False
Accept wildcard characters: False
```

### -Spacing
Controls the amount of spacing between this element and the preceding element.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 10
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Separator
Draw a separating line at the top of the element.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -HorizontalAlignment
Controls the horizontal text alignment.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 11
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Specifies the height of the element.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 12
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
A unique identifier associated with the item.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 13
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Hidden
If used this item will be removed from the visual tree.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
General notes

## RELATED LINKS
