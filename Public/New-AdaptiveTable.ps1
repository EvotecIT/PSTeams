function New-AdaptiveTable {
    <#
    .SYNOPSIS
    Simplifies process of creating an adaptive table by working with PowerShell objects

    .DESCRIPTION
    Simplifies process of creating an adaptive table by working with PowerShell objects

    .PARAMETER DataTable
    Provide a data table to be converted to an adaptive table

    .PARAMETER Width
    Controls the horizontal size of the element.

    .PARAMETER HeaderColor
    Provide a color to be used for the header row of the table. By default, the header row is set to 'Accent'

    .PARAMETER HeaderWeight
    Provide a weight to be used for the header row of the table. By default, the header row is set to 'Bolder'

    .PARAMETER HeaderSubtle
    Displays text slightly toned down to appear less prominent.

    .PARAMETER HeaderMaximumLines
    Specifies the maximum number of lines to display.

    .PARAMETER HeaderFontType
    Type of font to use for rendering

    .PARAMETER HeaderHorizontalAlignment
    Controls the horizontal text alignment.

    .PARAMETER HeaderSubtle
    Displays text slightly toned down to appear less prominent.

    .PARAMETER HeaderMaximumLines
    Specifies the maximum number of lines to display.

    .PARAMETER HeaderSize
    Controls size of text.

    .PARAMETER HeaderHighlight
    Controls the hightlight of text elements

    .PARAMETER HeaderStrikeThrough
    Controls strikethrough of text elements

    .PARAMETER HeaderItalic
    Controls italic of text elements

    .PARAMETER HeaderWrap
    Allow text to wrap. Otherwise, text is clipped.

    .PARAMETER HeaderHeight
    Specifies the height of the element.

    .PARAMETER HeaderSpacing
    Controls the amount of spacing between this element and the preceding element.

    .PARAMETER Subtle
    Displays text slightly toned down to appear less prominent.

    .PARAMETER MaximumLines
    Specifies the maximum number of lines to display.

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

    .PARAMETER Spacing
    Controls the amount of spacing between this element and the preceding element.

    .PARAMETER DictionaryAsCustomObject
    Forces display of Dictionary the same way as a custom object. By default, the Dictionary is displayed the way you see with Format-Table

    .EXAMPLE
    $Card = New-AdaptiveCard {
        New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Table usage with PSCustomObject' -Separator -Wrap

        New-AdaptiveTable -DataTable $Objects

        New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Table usage with OrderedDictionary' -Separator -Wrap

        New-AdaptiveTable -DataTable $ObjectsHashes
    } -Uri $Env:TEAMSPESTERID

    .NOTES
    General notes
    #>
    [CmdletBinding()]
    param(
        [Array] $DataTable,
        [ValidateSet('Auto', 'Stretch')][string] $Width = 'Stretch',
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string] $HeaderColor = 'Accent',
        [alias('HeaderFontWeight')][ValidateSet("Lighter", 'Default', "Bolder")][string] $HeaderWeight = 'Bolder',
        [alias('HeaderFontSize')][ValidateSet("Small", 'Default', "Medium", "Large", "ExtraLarge")][string] $HeaderSize,
        [switch] $HeaderHighlight,
        [switch] $HeaderItalic,
        [switch] $HeaderStrikeThrough,
        [ValidateSet('Default', 'Monospace')][string] $HeaderFontType,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $HeaderSpacing,
        [ValidateSet("Left", "Center", 'Right')][string] $HeaderHorizontalAlignment,
        [alias('HeaderBlockElementHeight')][ValidateSet('Stretch', 'Automatic')][string] $HeaderHeight,
        [switch] $HeaderSubtle,
        [int] $HeaderMaximumLines,

        [alias('FontWeight')][ValidateSet("Lighter", 'Default', "Bolder")][string] $Weight,
        [alias('FontSize')][ValidateSet("Small", 'Default', "Medium", "Large", "ExtraLarge")][string] $Size,
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string] $Color,
        [bool] $Highlight,
        [bool] $Italic,
        [bool] $StrikeThrough,
        [ValidateSet('Default', 'Monospace')][string[]] $FontType,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet("Left", "Center", 'Right')][string] $HorizontalAlignment,
        [switch] $Wrap,
        [alias('BlockElementHeight')][ValidateSet('Stretch', 'Automatic')][string] $Height,
        [switch] $Subtle,
        [int] $MaximumLines,

        [alias('HashTableAsCustomObject')][switch] $DictionaryAsCustomObject,
        [switch] $DisableHeaderColumnSeparators,
        [switch] $DisableRowSeparators,
        [switch] $DisableColumnSeparators
    )
    # We cleanup things before we start
    # This is also required because it seems to be working badly
    # in terms of availability of properties such as HorizontalAlignment once it runs few of variables
    # it starts bleeding thru the rest of the code overwritting values
    $ContentAdaptiveTextBlockSplat = @{
        Weight              = $Weight
        Color               = $Color
        Wrap                = $Wrap.IsPresent
        Size                = $Size
        Highlight           = $Highlight.IsPresent
        Italic              = $Italic.IsPresent
        StrikeThrough       = $StrikeThrough.IsPresent
        FontType            = $FontType
        Spacing             = $Spacing
        HorizontalAlignment = $HorizontalAlignment
        Height              = $Height
        MaximumLines        = $MaximumLines
    }
    Remove-EmptyValue -Hashtable $ContentAdaptiveTextBlockSplat

    $HeaderAdaptiveTextBlockSplat = @{
        Weight              = $HeaderWeight
        Color               = $HeaderColor
        Wrap                = $HeaderWrap.IsPresent
        Size                = $HeaderSize
        Highlight           = $HeaderHighlight.IsPresent
        Italic              = $HeaderItalic.IsPresent
        StrikeThrough       = $HeaderStrikeThrough.IsPresent
        FontType            = $HeaderFontType
        Spacing             = $HeaderSpacing
        HorizontalAlignment = $HeaderHorizontalAlignment
        MaximumLines        = $HeaderMaximumLines
        Height              = $HeaderHeight
    }
    Remove-EmptyValue -Hashtable $HeaderAdaptiveTextBlockSplat

    if ($DataTable[0] -is [System.Collections.IDictionary]) {
        # Process hashtables and dictionaries
        if ($DictionaryAsCustomObject) {
            # process it the same way as a custom object
            #Header
            New-AdaptiveColumnSet {
                for ($i = 0; $i -lt $DataTable[0].Keys.Count; $i++) {
                    New-AdaptiveColumn {
                        $HeaderText = @($DataTable[0].Keys)[$i]
                        New-AdaptiveTextBlock @HeaderAdaptiveTextBlockSplat -Text $HeaderText
                    } -Width $Width -Separator:(-not $DisableHeaderColumnSeparators.IsPresent)
                }
            }
            #Data
            for ($j = 0; $j -lt $DataTable.Count; $j++) {

                New-AdaptiveColumnSet {
                    for ($i = 0; $i -lt $DataTable[0].Keys.Count; $i++) {
                        New-AdaptiveColumn {
                            $Value = @($DataTable[$j].Values)[$i]
                            New-AdaptiveTextBlock @ContentAdaptiveTextBlockSplat -Text $Value
                        } -Width $Width -Separator:(-not $DisableColumnSeparators.IsPresent)
                    }
                } -Separator:(-not $DisableRowSeparators.IsPresent)
            }

        } else {
            # Process as standard hashtable
            # Header
            New-AdaptiveColumnSet {
                New-AdaptiveColumn {
                    New-AdaptiveTextBlock @HeaderAdaptiveTextBlockSplat -Text 'Name'
                } -Width $Width -Separator:(-not $DisableHeaderColumnSeparators.IsPresent)
                New-AdaptiveColumn {
                    New-AdaptiveTextBlock @HeaderAdaptiveTextBlockSplat -Text 'Value'
                } -Width $Width -Separator:(-not $DisableHeaderColumnSeparators.IsPresent)
            }
            # Data
            foreach ($Data in $DataTable) {
                foreach ($Key in $Data.Keys) {
                    New-AdaptiveColumnSet {
                        New-AdaptiveColumn {
                            New-AdaptiveTextBlock @ContentAdaptiveTextBlockSplat -Text $Key -Separator
                        } -Width $Width -Separator:(-not $DisableColumnSeparators.IsPresent)
                        New-AdaptiveColumn {
                            New-AdaptiveTextBlock @ContentAdaptiveTextBlockSplat -Text $Data.$Key -Separator
                        } -Width $Width -Separator:(-not $DisableColumnSeparators.IsPresent)
                    } -Separator:(-not $DisableRowSeparators.IsPresent)
                }
            }
        }
    } else {
        #Header
        New-AdaptiveColumnSet {
            for ($Column = 0; $Column -lt $DataTable[0].PSObject.Properties.Name.Count; $Column++) {
                New-AdaptiveColumn {
                    $HeaderText = $DataTable[0].PSObject.Properties.Name[$Column]
                    New-AdaptiveTextBlock @HeaderAdaptiveTextBlockSplat -Text $HeaderText
                } -Width $Width -Separator:(-not $DisableHeaderColumnSeparators.IsPresent)
            }
        }
        #Data
        for ($Row = 0; $Row -lt $DataTable.Count; $Row++) {
            New-AdaptiveColumnSet {
                for ($Column = 0; $Column -lt $DataTable[$Row].PSObject.Properties.Name.Count; $Column++) {
                    New-AdaptiveColumn {
                        $Value = $DataTable[$Row].PSObject.Properties.Value[$Column]
                        New-AdaptiveTextBlock @ContentAdaptiveTextBlockSplat -Text $Value
                    } -Width $Width -Separator:(-not $DisableColumnSeparators.IsPresent)
                }
            } -Separator:(-not $DisableRowSeparators.IsPresent)
        }
    }
}