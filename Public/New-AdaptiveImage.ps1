function New-AdaptiveImage {
    <#
    .SYNOPSIS
    Displays an image. Acceptable formats are PNG, JPEG, and GIF

    .DESCRIPTION
    Displays an image. Acceptable formats are PNG, JPEG, and GIF

    .PARAMETER Url
    The URL to the image.

    .PARAMETER Style
    Controls how this Image is displayed.

    .PARAMETER AlternateText
    Alternate text describing the image.

    .PARAMETER Size
    Controls the approximate size of the image. The physical dimensions will vary per host.

    .PARAMETER Spacing
    Controls the amount of spacing between this element and the preceding element.

    .PARAMETER Separator
    Draw a separating line at the top of the element.

    .PARAMETER HorizontalAlignment
    Controls how this element is horizontally positioned within its parent.

    .PARAMETER Height
    The desired height of the image.

    .PARAMETER HeightInPixels
    The desired height of the image. Will be specified in pixel value. The image will distort to fit that exact height. This overrides the size property.

    .PARAMETER WidthInPixels
    The desired on-screen width of the image. This overrides the size property.

    .PARAMETER Id
    A unique identifier associated with the item.

    .PARAMETER Hidden
    If used this item will be removed from the visual tree.

    .PARAMETER BackgroundColor
    Applies a background to a transparent image. This property will respect the image style.

    .PARAMETER SelectAction
    An Action that will be invoked when the card is tapped or selected.

    .PARAMETER SelectActionId
    Provide ID for Select Action

    .PARAMETER SelectActionUrl
    Provide URL to open when using SelectAction with Action.OpenUrl

    .PARAMETER SelectActionTitle
    Provide Title for Select Action

    .EXAMPLE
    New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'

    .EXAMPLE
    New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person

    .EXAMPLE
    New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz'

    .EXAMPLE
    New-HeroImage -Url 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg'

    .EXAMPLE
    New-ThumbnailImage -Url 'https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png' -AltText "Bender Rodríguez"

    .NOTES
    Adaptive Image supports most if not all of those options. However HeroImage and ThumbnailImage most likely support only some if not just what is shown in Examples.
    I didn't want to create additional functions just for the sake of having more of them, as I expect most people using Adaptive Cards, and occasionally other types.

    #>
    [alias('New-HeroImage', 'New-ThumbnailImage')]
    [cmdletBinding()]
    param(
        [alias('Link')][string] $Url,
        [ValidateSet('person', 'default')][string] $Style,
        [alias('Alt', 'AltText')][string] $AlternateText,
        [ValidateSet('Auto', 'Stretch', 'Small', 'Medium', 'Large')][string] $Size,
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [int] $HeightInPixels,
        [int] $WidthInPixels,
        # Layout End
        [string] $Id,
        [switch] $Hidden,
        [string] $BackgroundColor,
        # SelectAction
        [ValidateSet('Action.Submit', 'Action.OpenUrl', 'Action.ToggleVisibility')][string] $SelectAction,
        [string] $SelectActionId,
        [string] $SelectActionUrl,
        [string] $SelectActionTitle,
        [string[]] $SelectActionTargetElement
    )
    $TeamObject = [ordered] @{
        type                = 'Image'
        id                  = $Id
        url                 = $Url
        size                = $Size
        alt                 = $AlternateText
        style               = $Style
        # Start Layout
        horizontalAlignment = $HorizontalAlignment
        height              = $Height
        spacing             = $Spacing
        # End Layout
        backgroundColor     = ConvertFrom-Color -Color $BackgroundColor
    }
    # Start Layout
    if ($Separator) {
        $TeamObject['separator'] = $Separator.IsPresent
    }
    # End Layout
    if ($Hidden) {
        $TeamObject['isVisible'] = $false
    }
    if ($WidthInPixels) {
        $TeamObject['width'] = "$($WidthInPixels)px"
    }
    if ($HeightInPixels) {
        $TeamObject['height'] = "$($HeightInPixels)px"
    } else {
        $TeamObject['height'] = $Height
    }
    if ($SelectActionUrl) {
        # We help user so the actioon choses itself
        $SelectAction = 'Action.OpenUrl'
    }
    $TeamObject['selectAction'] = [ordered] @{
        type  = $SelectAction
        id    = $SelectActionId
        title = $SelectActionTitle
        url   = $SelectActionUrl
    }
    if ($SelectActionTargetElement) {
        # We help user so the actioon choses itself
        $TeamObject['selectAction']['type'] = 'Action.ToggleVisibility'
        # We add missing data
        $TeamObject['selectAction']['targetElements'] = @(
            $SelectActionTargetElement
        )
    }
    Remove-EmptyValue -Hashtable $TeamObject -Recursive -Rerun 1
    $TeamObject
}

$Script:ScriptBlockColors = {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
    $Script:RGBColors.Keys | Where-Object { $_ -like "*$wordToComplete*" }
}

Register-ArgumentCompleter -CommandName New-AdaptiveImage -ParameterName BackgroundColor -ScriptBlock $Script:ScriptBlockColors