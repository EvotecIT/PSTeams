function New-AdaptiveImage {
    [alias('New-HeroImage', 'New-ThumbnailImage')]
    [cmdletBinding()]
    param(
        [alias('Link')][string] $Url,
        [ValidateSet('Auto', 'Stretch', 'Small', 'Medium', 'Large')][string] $Size,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [alias('Alt')][string] $AltText
    )
    $TeamObject = [ordered] @{
        type   = 'Image'
        url    = $Url
        size   = $Size
        height = $Height
        alt    = $AltText
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}