function New-AdaptiveRichTextBlock {
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
        [ValidateSet('Stretch', 'Automatic')][string] $Height
        # Layout End
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
    if ($Separator) {
        $TeamObject['separator'] = $Separator.IsPresent
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}