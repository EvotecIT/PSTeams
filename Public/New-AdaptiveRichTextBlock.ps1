function New-AdaptiveRichTextBlock {
    [cmdletBinding()]
    param(
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [switch] $Separator
    )
    $TeamObject = [ordered]@{
        type                = "RichTextBlock"
        inlines             = @(
            @{
                type = 'TextRun'
                text = 'New RichTextBlock'
            }
        )
        horizontalAlignment = $HorizontalAlignment
        height              = $Height
        fontType            = $FontType
    }
    if ($Separator) {
        $TeamObject['separator'] = $Separator.IsPresent
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}