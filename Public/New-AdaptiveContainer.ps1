function New-AdaptiveContainer {
    [cmdletBinding()]
    param(
        [scriptblock] $Items,
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        # Layout End
        [ValidateSet("Accent", 'Default', 'Emphasis', 'Good', 'Warning', 'Attention')][string] $Style,
        [int] $MinimumHeight,
        [switch] $Bleed,
        [ValidateSet('top', 'center', 'bottom')][string] $VerticalContentAlignment,

        [string] $BackgroundUrl,
        [ValidateSet('Cover', 'RepeatHorizontally', 'RepeatVertically', 'Repeat')][string] $BackgroundFillMode,
        [ValidateSet('left', 'center', 'right')][string] $BackgroundHorizontalAlignment,
        [ValidateSet('top', 'center', 'bottom')][string] $BackgroundVerticalAlignment,

        [ValidateSet('Action.Submit', 'Action.OpenUrl', 'Action.ToggleVisibility')][string] $SelectAction,
        [string] $SelectActionId,
        [string] $SelectActionUrl,
        [string] $SelectActionTitle
    )
    if ($Items) {
        $OutputItems = & $Items
        if ($OutputItems) {
            $TeamObject = [ordered] @{
                type                     = "Container"
                items                    = @(
                    $OutputItems
                )
                style                    = $Style
                verticalContentAlignment = $verticalContentAlignment
                # Layout
                horizontalAlignment      = $HorizontalAlignment
                height                   = $Height
                spacing                  = $Spacing
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
            $TeamObject['backgroundImage'] = [ordered] @{
                "fillMode"            = $BackgroundFillMode
                "horizontalAlignment" = $BackgroundHorizontalAlignment
                "verticalAlignment"   = $BackgroundVerticalAlignment
                "url"                 = $BackgroundUrl
            }
            $TeamObject['selectAction'] = [ordered] @{
                type  = $SelectAction
                id    = $SelectActionId
                title = $SelectActionTitle
                url   = $SelectActionUrl
            }
            Remove-EmptyValue -Hashtable $TeamObject -Recursive -Rerun 1
            $TeamObject
        }
    }
}