---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveTextBlock

## SYNOPSIS
Displays text, allowing control over font sizes, weight, and color.

## SYNTAX

```
New-AdaptiveTextBlock [[-Text] <String>] [[-Color] <String>] [[-FontType] <String>]
 [[-HorizontalAlignment] <String>] [-Subtle] [[-MaximumLines] <Int32>] [[-Size] <String>] [[-Weight] <String>]
 [-Wrap] [[-Height] <String>] [-Separator] [[-Spacing] <String>] [[-Id] <String>] [-Hidden]
 [<CommonParameters>]
```

## DESCRIPTION
Displays text, allowing control over font sizes, weight, and color.

## EXAMPLES

### EXAMPLE 1
```
New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
```

New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Dark
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Light
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Good
        }
    }
} -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz' -Verbose

## PARAMETERS

### -Text
Text to display.
A subset of markdown is supported (https://aka.ms/ACTextFeatures)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Controls the color of TextBlock elements.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontType
Type of font to use for rendering

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
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
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Subtle
Displays text slightly toned down to appear less prominent.

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

### -MaximumLines
Specifies the maximum number of lines to display.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Controls size of text.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FontSize

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Weight
Controls the weight of TextBlock elements.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FontWeight

Required: False
Position: 7
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Wrap
Allow text to wrap.
Otherwise, text is clipped.

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

### -Height
Specifies the height of the element.

```yaml
Type: String
Parameter Sets: (All)
Aliases: BlockElementHeight

Required: False
Position: 8
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

### -Spacing
Controls the amount of spacing between this element and the preceding element.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 9
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
Position: 10
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
