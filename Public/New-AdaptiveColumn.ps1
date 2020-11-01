function New-AdaptiveColumn {
    [cmdletBinding()]
    param(
        [scriptblock] $Items,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [ValidateSet('stretch', 'auto', 'weighted')][string] $Width,
        [int] $WidthSize,
        [int] $MinimumHeight,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Top', 'Center', 'Bottom')][string] $VerticalContentAlignment,
        [ValidateSet("Accent", 'Default', 'Emphasis', 'Good', 'Warning', 'Attention')][string] $Style,
        [switch] $Hidden,
        [switch] $Separator
    )
    if ($WidthSize) {
        $WidthValue = "$($WidthSize)px"
    } else {
        # Width value pixels is not displayed
        $WidthValue = $Width
    }

    if ($Items) {
        $OutputItems = & $Items
        if ($OutputItems) {
            $TeamObject = [ordered] @{
                type                     = 'Column'
                width                    = $WidthValue
                height                   = $Height
                items                    = @(
                    $OutputItems
                )
                horizontalAlignment      = $HorizontalAlignment
                verticalContentAlignment = $VerticalContentAlignment
                spacing                  = $Spacing
                style                    = $Style
            }
            if ($MinimumHeight) {
                $TeamObject['minHeight'] = "$($MinimumHeight)px"
            }
            if ($Hidden) {
                $TeamObject['isVisible'] = $false
            }
            if ($Separator) {
                $TeamObject['separator'] = $Separator.IsPresent
            }
            Remove-EmptyValue -Hashtable $TeamObject
            $TeamObject
        }
    }
}