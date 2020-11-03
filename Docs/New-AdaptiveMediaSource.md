---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveMediaSource

## SYNOPSIS
Defines a source for a Media element

## SYNTAX

```
New-AdaptiveMediaSource [[-Type] <String>] [[-Url] <String>] [<CommonParameters>]
```

## DESCRIPTION
Defines a source for a Media element

## EXAMPLES

### EXAMPLE 1
```
New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
```

### EXAMPLE 2
```
New-AdaptiveMedia -PosterUrl 'https://adaptivecards.io/content/poster-video.png' {
```

New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
    New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
}

## PARAMETERS

### -Type
Mime type of associated media (e.g.
"video/mp4").

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

### -Url
URL to media.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
Media playback is currently not supported in Adaptive Cards in Teams.
Adding it for sake of having.
May need to improve how it's handled.

## RELATED LINKS
