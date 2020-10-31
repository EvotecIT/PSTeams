function New-AdaptiveImage {
    [alias('New-HeroImage')]
    [cmdletBinding()]
    param(
        [alias('Link')][string] $Url,
        [ValidateSet('Auto', 'Stretch', 'Small', 'Medium', 'Large')][string] $Size,
        [ValidateSet('Stretch', 'Automatic')][string] $Height
    )
    $TeamObject = [ordered] @{
        type   = 'Image'
        url    = $Url
        size   = $Size
        height = $Height
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}