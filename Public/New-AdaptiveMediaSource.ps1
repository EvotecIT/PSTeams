function New-AdaptiveMediaSource {
    <#
    .SYNOPSIS
    Defines a source for a Media element

    .DESCRIPTION
    Defines a source for a Media element

    .PARAMETER Type
    Mime type of associated media (e.g. "video/mp4").

    .PARAMETER Url
    URL to media.

    .EXAMPLE
    New-AdaptiveMediaSource -Type "video/mp4" -Url "https://adaptivecardsblob.blob.core.windows.net/assets/AdaptiveCardsOverviewVideo.mp4"

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
        [string] $Type,
        [string] $Url
    )
    $TeamObject = [ordered] @{
        mimeType = $Type
        url      = $Url
    }
    $TeamObject
}