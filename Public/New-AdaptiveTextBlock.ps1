function New-AdaptiveTextBlock {
    [cmdletBinding()]
    param(
        [string] $Text,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet("Small", 'Default', "Medium", "Large", "ExtraLarge")][string] $Size,
        [ValidateSet("Lighter", 'Default', "Bolder")][string] $Weight,
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string] $Color,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [ValidateSet('Default', 'Monospace')][string] $FontType,
        [switch] $Subtle,
        [switch] $Hidden,
        [int] $MaximumLines,
        [switch] $Separator,
        [switch] $Wrap
    )
    $TeamObject = [ordered]@{
        type                = "TextBlock"
        text                = $Text
        #id"                  = "Title"
        spacing             = $Spacing
        horizontalAlignment = $HorizontalAlignment
        size                = $Size
        weight              = $Weight
        color               = $Color
        height              = $Height
        fontType            = $FontType
    }
    if ($MaximumLines) {
        $TeamObject['maxLines'] = $MaximumLines
    }
    if ($Separator) {
        $TeamObject['separator'] = $Separator.IsPresent
    }
    if ($Wrap) {
        $TeamObject['wrap'] = $Wrap.IsPresent
    }
    if ($Subtle) {
        $TeamObject['isSubtle'] = $true
    }
    if ($Hidden) {
        $TeamObject['isVisible'] = $false
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}

