function New-AdaptiveImage {
    [alias('New-HeroImage', 'New-ThumbnailImage')]
    [cmdletBinding()]
    param(
        [alias('Link')][string] $Url,
        [ValidateSet('person', 'default')][string] $Style,
        [alias('Alt')][string] $AlternateText,
        [ValidateSet('Auto', 'Stretch', 'Small', 'Medium', 'Large')][string] $Size,
        # Layout Start
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [switch] $Separator,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [int] $HeightInPixels,
        [int] $WidthInPixels,
        # Layout End
        [string] $Id,
        [switch] $Hidden,
        [string] $BackgroundColor
    )
    $TeamObject = [ordered] @{
        type                = 'Image'
        id                  = $Id
        url                 = $Url
        size                = $Size
        alt                 = $AlternateText
        style               = $Style
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
    if ($WidthInPixels) {
        $TeamObject['width'] = "$($WidthInPixels)px"
    }
    if ($HeightInPixels) {
        $TeamObject['height'] = "$($HeightInPixels)px"
    } else {
        $TeamObject['height'] = $Height
    }
    if ($BackgroundColor) {
        $TeamObject['backgroundColor'] = ConvertFrom-Color -Color $BackgroundColor
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}

$Script:ScriptBlockColors = {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
    $Script:RGBColors.Keys | Where-Object { $_ -like "*$wordToComplete*" }
}

Register-ArgumentCompleter -CommandName New-AdaptiveImage -ParameterName BackgroundColor -ScriptBlock $Script:ScriptBlockColors