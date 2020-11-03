function New-AdaptiveColumn {
    [cmdletBinding()]
    param(
        [scriptblock] $Items,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [ValidateSet('Stretch', 'Auto', 'Weighted')][string] $Width,
        [int] $WidthInWeight,
        [int] $WidthInPixels,
        [int] $MinimumHeight,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Top', 'Center', 'Bottom')][string] $VerticalContentAlignment,
        [ValidateSet("Accent", 'Default', 'Emphasis', 'Good', 'Warning', 'Attention')][string] $Style,
        [switch] $Hidden,
        [switch] $Separator,

        [ValidateSet('Action.Submit', 'Action.OpenUrl', 'Action.ToggleVisibility')][string] $SelectAction,
        [string] $SelectActionId,
        [string] $SelectActionUrl,
        [string] $SelectActionTitle,
        [string[]] $SelectActionTargetElement
    )
    if ($WidthInWeight) {
        $WidthValue = "$($WidthInWeight)"
        # it actually forces $Width = Weighted but it's not in JSON
    } elseif ($WidthInPixels) {
        $WidthValue = "$($WidthInPixels)px"
    } else {
        # Width value pixels is not displayed
        # it seems width requires lowerCase values which is weird for Microsoft
        $WidthValue = $Width.ToLower()
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
    }
}