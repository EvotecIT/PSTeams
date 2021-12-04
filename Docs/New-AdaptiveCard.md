---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveCard

## SYNOPSIS
An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

## SYNTAX

```
New-AdaptiveCard [[-Body] <ScriptBlock>] [[-Action] <ScriptBlock>] [[-Uri] <String>] [[-FallBackText] <String>]
 [[-MinimumHeight] <Int32>] [[-Speak] <String>] [[-Language] <String>] [[-VerticalContentAlignment] <String>]
 [[-BackgroundUrl] <String>] [[-BackgroundFillMode] <String>] [[-BackgroundHorizontalAlignment] <String>]
 [[-BackgroundVerticalAlignment] <String>] [[-SelectAction] <String>] [[-SelectActionId] <String>]
 [[-SelectActionUrl] <String>] [[-SelectActionTitle] <String>] [-FullWidth] [-AllowImageExpand]
 [<CommonParameters>]
```

## DESCRIPTION
An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

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

### -Body
The card elements to show in the primary card region.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Action
The Actions to show in the card's action bar.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Uri
WebHook Uri to send Adaptive Card to.
When provided sends Adaptive Card.
When not provided JSON is returned.

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

### -FallBackText
Text shown when the client doesn't support the version specified (may contain markdown).

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

### -MinimumHeight
Specifies the minimum height of the card.

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

### -Speak
Specifies what should be spoken for this entire card.
This is simple text or SSML fragment.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Language
The 2-letter ISO-639-1 language used in the card.
Used to localize any date/time functions.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 7
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerticalContentAlignment
Defines how the content should be aligned vertically within the container.
Only relevant for fixed-height cards, or cards with a minHeight specified.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundUrl
Specifies a background image.
Acceptable formats are PNG, JPEG, and GIF

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

### -BackgroundFillMode
Controls how background is displayed

"cover": The background image covers the entire width of the container.
Its aspect ratio is preserved.
Content may be clipped if the aspect ratio of the image doesn't match the aspect ratio of the container.
verticalAlignment is respected (horizontalAlignment is meaningless since it's stretched width).
This is the default mode and is the equivalent to the current model.
"repeatHorizontally": The background image isn't stretched.
It is repeated in the x axis as many times as necessary to cover the container's width.
verticalAlignment is honored (default is top), horizontalAlignment is ignored.
"repeatVertically": The background image isn't stretched.
It is repeated in the y axis as many times as necessary to cover the container's height.
verticalAlignment is ignored, horizontalAlignment is honored (default is left).
"repeat": The background image isn't stretched.
It is repeated first in the x axis then in the y axis as many times as necessary to cover the entire container.
Both horizontalAlignment and verticalAlignment are honored (defaults are left and top).

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

### -BackgroundHorizontalAlignment
Controls how background is aligned horizontally

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

### -BackgroundVerticalAlignment
Controls how background is aligned vertically

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

### -SelectAction
An Action that will be invoked when the card is tapped or selected.

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

### -SelectActionId
Provide ID for Select Action

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 14
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionUrl
Provide URL to open when using SelectAction with Action.OpenUrl

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 15
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionTitle
Provide Title for Select Action

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 16
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FullWidth
{{ Fill FullWidth Description }}

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

### -AllowImageExpand
{{ Fill AllowImageExpand Description }}

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
