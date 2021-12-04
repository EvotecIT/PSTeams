---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveImage

## SYNOPSIS
Displays an image.
Acceptable formats are PNG, JPEG, and GIF

## SYNTAX

```
New-AdaptiveImage [[-Url] <String>] [[-Style] <String>] [[-AlternateText] <String>] [[-Size] <String>]
 [[-Spacing] <String>] [-Separator] [[-HorizontalAlignment] <String>] [[-Height] <String>]
 [[-HeightInPixels] <Int32>] [[-WidthInPixels] <Int32>] [[-Id] <String>] [-Hidden]
 [[-BackgroundColor] <String>] [[-SelectAction] <String>] [[-SelectActionId] <String>]
 [[-SelectActionUrl] <String>] [[-SelectActionTitle] <String>] [[-SelectActionTargetElement] <String[]>]
 [<CommonParameters>]
```

## DESCRIPTION
Displays an image.
Acceptable formats are PNG, JPEG, and GIF

## EXAMPLES

### EXAMPLE 1
```
New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
```

### EXAMPLE 2
```
New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
```

### EXAMPLE 3
```
New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz'
```

### EXAMPLE 4
```
New-HeroImage -Url 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg'
```

### EXAMPLE 5
```
New-ThumbnailImage -Url 'https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png' -AltText "Bender Rodríguez"
```

## PARAMETERS

### -Url
The URL to the image.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Link

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style
Controls how this Image is displayed.

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

### -AlternateText
Alternate text describing the image.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Alt, AltText

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Controls the approximate size of the image.
The physical dimensions will vary per host.

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

### -Spacing
Controls the amount of spacing between this element and the preceding element.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
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
Controls how this element is horizontally positioned within its parent.

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

### -Height
The desired height of the image.

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

### -HeightInPixels
The desired height of the image.
Will be specified in pixel value.
The image will distort to fit that exact height.
This overrides the size property.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -WidthInPixels
The desired on-screen width of the image.
This overrides the size property.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 9
Default value: 0
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

### -BackgroundColor
Applies a background to a transparent image.
This property will respect the image style.

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

### -SelectAction
An Action that will be invoked when the card is tapped or selected.

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

### -SelectActionId
Provide ID for Select Action

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

### -SelectActionUrl
Provide URL to open when using SelectAction with Action.OpenUrl

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

### -SelectActionTitle
Provide Title for Select Action

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

### -SelectActionTargetElement
{{ Fill SelectActionTargetElement Description }}

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 16
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
Adaptive Image supports most if not all of those options.
However HeroImage and ThumbnailImage most likely support only some if not just what is shown in Examples.
I didn't want to create additional functions just for the sake of having more of them, as I expect most people using Adaptive Cards, and occasionally other types.

## RELATED LINKS
