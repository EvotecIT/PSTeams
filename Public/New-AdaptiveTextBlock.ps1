function New-AdaptiveTextBlock {
    <#
    .SYNOPSIS
    Displays text, allowing control over font sizes, weight, and color.

    .DESCRIPTION
    Displays text, allowing control over font sizes, weight, and color.

    .PARAMETER Text
    Text to display. A subset of markdown is supported (https://aka.ms/ACTextFeatures)

    .PARAMETER Color
    Controls the color of TextBlock elements.

    .PARAMETER FontType
    Type of font to use for rendering

    .PARAMETER HorizontalAlignment
    Controls the horizontal text alignment.

    .PARAMETER Subtle
    Displays text slightly toned down to appear less prominent.

    .PARAMETER MaximumLines
    Specifies the maximum number of lines to display.

    .PARAMETER Size
    Controls size of text.

    .PARAMETER Weight
    Controls the weight of TextBlock elements.

    .PARAMETER Highlight
    Controls the hightlight of text elements

    .PARAMETER StrikeThrough
    Controls strikethrough of text elements

    .PARAMETER Italic
    Controls italic of text elements

    .PARAMETER Wrap
    Allow text to wrap. Otherwise, text is clipped.

    .PARAMETER Height
    Specifies the height of the element.

    .PARAMETER Separator
    Draw a separating line at the top of the element.

    .PARAMETER Spacing
    Controls the amount of spacing between this element and the preceding element.

    .PARAMETER Id
    A unique identifier associated with the item.

    .PARAMETER Hidden
    If used this item will be removed from the visual tree.

    .EXAMPLE
    New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
        New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Dark
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Light
            }
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Good
            }
        }
    } -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz' -Verbose

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [string] $Text,
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string] $Color,
        [ValidateSet('Default', 'Monospace')][string] $FontType,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [switch] $Subtle,
        [int] $MaximumLines,
        [alias('FontSize')][ValidateSet("Small", 'Default', "Medium", "Large", "ExtraLarge")][string] $Size,
        [alias('FontWeight')][ValidateSet("Lighter", 'Default', "Bolder")][string] $Weight,
        [switch] $Highlight,
        [switch] $Italic,
        [switch] $StrikeThrough,
        [switch] $Wrap,
        [alias('BlockElementHeight')][ValidateSet('Stretch', 'Automatic')][string] $Height,
        [switch] $Separator,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [string] $Id,
        [switch] $Hidden
    )
    $TeamObject = [ordered]@{
        type                = "TextBlock"
        # the intent behind this is to allow for empty textblocks to be created
        # if there is no text, the block is never added which causes all sort of issues
        # this is a workaround for that
        text                = if ($Text -eq '') { "$([char]0x200F)" } else { $Text }
        id                  = $Id
        spacing             = $Spacing
        horizontalAlignment = $HorizontalAlignment
        size                = $Size
        weight              = $Weight
        color               = $Color
        height              = $Height
        fontType            = $FontType
        highlight           = $Highlight
        italic              = $Italic
        strikeThrough       = $StrikeThrough
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
    Remove-EmptyValue -Hashtable $TeamObject -ExcludeParameter 'text'
    $TeamObject
}

