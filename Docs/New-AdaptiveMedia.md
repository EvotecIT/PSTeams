---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveMedia

## SYNOPSIS
Displays a media player for audio or video content.

## SYNTAX

```
New-AdaptiveMedia [-Sources] <ScriptBlock> [[-PosterUrl] <String>] [[-AlternateText] <String>]
 [[-Spacing] <String>] [-Separator] [[-HorizontalAlignment] <String>] [[-Height] <String>] [[-Id] <String>]
 [-Hidden] [<CommonParameters>]
```

## DESCRIPTION
Displays a media player for audio or video content.

## EXAMPLES

### EXAMPLE 1
```
New-AdaptiveMedia -PosterUrl 'https://adaptivecards.io/content/poster-video.png' {
```

New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
    New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
}

## PARAMETERS

### -Sources
One or more sources of media to attempt to play.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PosterUrl
URL of an image to display before playing

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
Alternate text describing the audio or video.

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

### -Spacing
Controls the amount of spacing between this element and the preceding element.

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
Position: 5
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
Position: 6
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
Position: 7
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
Media playback is currently not supported in Adaptive Cards in Teams.
Adding it for sake of having.
May need to improve how it's handled.

## RELATED LINKS
