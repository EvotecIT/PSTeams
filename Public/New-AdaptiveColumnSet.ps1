function New-AdaptiveColumnSet {
    [cmdletBinding()]
    param(
        [scriptblock] $Columns,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet("Accent", 'Default', 'Emphasis', 'Good', 'Warning', 'Attention')][string] $Style,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [int] $MinimumHeight,
        [switch] $Bleed,
        [switch] $Separator
    )
    if ($Columns) {
        $TeamObject = [ordered] @{
            "type"                = "ColumnSet"
            "columns"             = @(
                & $Columns
            )
            "horizontalAlignment" = $HorizontalAlignment
            "style"               = $Style
            "height"              = $Height
            "spacing"             = $Spacing
        }
        if ($Bleed) {
            $TeamObject['bleed'] = $true
        }
        if ($MinimumHeight) {
            $TeamObject['minHeight'] = "$($MinimumHeight)px"
        }
        if ($Separator) {
            $TeamObject['separator'] = $Separator.IsPresent
        }
        Remove-EmptyValue -Hashtable $TeamObject
        $TeamObject
    }
}