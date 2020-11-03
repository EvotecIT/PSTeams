function New-AdaptiveRichTextBlock {
    <#
    .SYNOPSIS
    Defines an array of inlines, allowing for inline text formatting.

    .DESCRIPTION
    Defines an array of inlines, allowing for inline text formatting.

    .PARAMETER Text
    Text to display.

    .PARAMETER Color
    Controls the color of text elements.

    .PARAMETER Subtle
    Displays text slightly toned down to appear less prominent.

    .PARAMETER Size
    Controls size of text.

    .PARAMETER Weight
    Controls the weight of text elements.

    .PARAMETER Highlight
    Controls the hightlight of text elements

    .PARAMETER Italic
    Controls italic of text elements

    .PARAMETER StrikeThrough
    Controls strikethrough of text elements

    .PARAMETER FontType
    Type of font to use for rendering

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
    New-AdaptiveRichTextBlock -Text 'This is the first inline.', 'We support colors,', 'both regular and subtle. ', 'Monospace too!' -Color Attention, Default, Warning -StrikeThrough $false, $true, $false -Size ExtraLarge, Default, Medium -Italic $false, $false, $true -Subtle $false, $true, $true

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [string[]] $Text,
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string[]] $Color = @(),
        [bool[]] $Subtle = @(),
        [alias('FontSize')][ValidateSet("Small", 'Default', "Medium", "Large", "ExtraLarge")][string[]] $Size = @(),
        [alias('FontWeight')][ValidateSet("Lighter", 'Default', "Bolder")][string[]] $Weight = @(),
        [bool[]] $Highlight = @(),
        [bool[]] $Italic = @(),
        [bool[]] $StrikeThrough = @(),
        [ValidateSet('Default', 'Monospace')][string[]] $FontType = @(),
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        # Layout End
        [string] $Id,
        [switch] $Hidden
    )

    [Array] $Inlines = for ($a = 0; $a -lt $Text.Count; $a++) {
        $TextRun = [ordered] @{
            type = 'TextRun'
            text = $Text[$a]
        }
        if ($Color[$a]) {
            $TextRun['color'] = $Color[$a]
        }
        if ($Subtle[$a]) {
            $TextRun['subtle'] = $Subtle[$a]
        }
        if ($Size[$a]) {
            $TextRun['size'] = $Size[$a]
        }
        if ($Weight[$a]) {
            $TextRun['weight'] = $Weight[$a]
        }
        if ($Highlight[$a]) {
            $TextRun['highlight'] = $Highlight[$a]
        }
        if ($Italic[$a]) {
            $TextRun['italic'] = $Italic[$a]
        }
        if ($StrikeThrough[$a]) {
            $TextRun['strikethrough'] = $StrikeThrough[$a]
        }
        if ($FontType[$a]) {
            $TextRun['fontType'] = $FontType[$a]
        }
        $TextRun
    }
    $TeamObject = [ordered]@{
        type                = "RichTextBlock"
        id                  = $Id
        inlines             = $Inlines
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
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}