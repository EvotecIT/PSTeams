function New-AdaptiveMedia {
    <#
    .SYNOPSIS
    Displays a media player for audio or video content.

    .DESCRIPTION
    Displays a media player for audio or video content.

    .PARAMETER Sources
    One or more sources of media to attempt to play.

    .PARAMETER PosterUrl
    URL of an image to display before playing

    .PARAMETER AlternateText
    Alternate text describing the audio or video.

    .PARAMETER Spacing
    Controls the amount of spacing between this element and the preceding element.

    .PARAMETER Separator
    Draw a separating line at the top of the element.

    .PARAMETER HorizontalAlignment
    Controls the horizontal text alignment.

    .PARAMETER Height
    Specifies the height of the element.

    .PARAMETER Id
    A unique identifier associated with the item.

    .PARAMETER Hidden
    If used this item will be removed from the visual tree.

    .EXAMPLE
    New-AdaptiveMedia -PosterUrl 'https://adaptivecards.io/content/poster-video.png' {
        New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
        New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"
    }

    .NOTES
    Media playback is currently not supported in Adaptive Cards in Teams. Adding it for sake of having.
    May need to improve how it's handled.

    #>
    [cmdletBinding()]
    param(
        [parameter(Mandatory)][scriptblock] $Sources,
        [string] $PosterUrl,
        [string] $AlternateText,
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        # Layout End,
        [string] $Id,
        [switch] $Hidden
    )
    if ($Sources) {
        $TeamObject = [ordered] @{
            type                = 'Media'
            poster              = $PosterUrl
            id                  = $Id
            altText             = $AlternateText
            # Start Layout
            horizontalAlignment = $HorizontalAlignment
            height              = $Height
            spacing             = $Spacing
            # End Layout
        }
        # Start Layout
        if ($Separator) {
            $TeamObject['separator'] = $Separator.IsPresent
        }
        # End Layout
        if ($Hidden) {
            $TeamObject['isVisible'] = $false
        }
        $TeamObject['sources'] = @(
            & $Sources
        )
        Remove-EmptyValue -Hashtable $TeamObject
        $TeamObject
    }
}
