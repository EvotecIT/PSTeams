function New-AdaptiveImageSet {
    <#
    .SYNOPSIS
    The ImageSet displays a collection of Images similar to a gallery. Acceptable formats are PNG, JPEG, and GIF

    .DESCRIPTION
    The ImageSet displays a collection of Images similar to a gallery. Acceptable formats are PNG, JPEG, and GIF

    .PARAMETER Images
    List of images

    .PARAMETER Size
    Controls size of all images in gallery

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
    New-AdaptiveImageGallery {
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Style person
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Style person
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Style person
    } -HorizontalAlignment Right -Size Large

    .EXAMPLE
    New-AdaptiveImageGallery {
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
    }

    .EXAMPLE
    New-AdaptiveImageGallery {
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
    } -Size Small

    .NOTES
    General notes
    #>
    [alias('New-AdaptiveImageGallery')]
    [cmdletBinding()]
    param(
        [scriptblock] $Images,
        [ValidateSet('Small', 'Medium', 'Large')][string] $Size,
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        # Layout End
        [string] $Id,
        [switch] $Hidden
    )
    if ($Images) {
        $OutputImages = & $Images
        if ($OutputImages) {
            $TeamObject = [ordered] @{
                type                = 'ImageSet'
                images              = @($OutputImages)
                id                  = $Id
                imageSize           = $Size
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
            Remove-EmptyValue -Hashtable $TeamObject -Recursive -Rerun 1
            $TeamObject
        }
    }
}