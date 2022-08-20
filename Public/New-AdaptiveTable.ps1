function New-AdaptiveTable {
    <#
    .SYNOPSIS
    Simplifies process of creating an adaptive table by working with PowerShell objects

    .DESCRIPTION
    Simplifies process of creating an adaptive table by working with PowerShell objects

    .PARAMETER DataTable
    Provide a data table to be converted to an adaptive table

    .PARAMETER HeaderColor
    Provide a color to be used for the header row of the table. By default, the header row is set to 'Accent'

    .PARAMETER HeaderWeight
    Provide a weight to be used for the header row of the table. By default, the header row is set to 'Bolder'

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
        [ValidateSet("Accent", 'Default', 'Dark', 'Light', 'Good', 'Warning', 'Attention')][string] $HeaderColor = 'Accent',
        [alias('FontWeight')][ValidateSet("Lighter", 'Default', "Bolder")][string] $HeaderWeight = 'Bolder',
        [alias('HashTableAsCustomObject')][switch] $DictionaryAsCustomObject
    )

    if ($DataTable[0] -is [System.Collections.IDictionary]) {
        if ($DictionaryAsCustomObject) {
            New-AdaptiveColumnSet {
                for ($i = 0; $i -lt $DataTable[0].Keys.Count; $i++) {
                    New-AdaptiveColumn {
                        $HeaderText = @($DataTable[0].Keys)[$i]
                        New-AdaptiveTextBlock -Text $HeaderText -Weight $HeaderWeight -Color $HeaderColor
                        for ($j = 0; $j -lt $DataTable.Count; $j++) {
                            $Test = @($DataTable[$j].Values)[$i]
                            New-AdaptiveTextBlock -Text $Test -Separator
                        }
                    } -Width Stretch
                }
            }
        } else {
            New-AdaptiveColumnSet {
                New-AdaptiveColumn {
                    New-AdaptiveTextBlock -Text 'Name' -Weight $HeaderWeight -Color $HeaderColor
                    foreach ($Data in $DataTable) {
                        foreach ($Key in $Data.Keys) {
                            New-AdaptiveTextBlock -Text $Key -Separator
                        }
                    }
                } -Width Stretch

                New-AdaptiveColumn {
                    New-AdaptiveTextBlock -Text 'Value' -Weight $HeaderWeight -Color $HeaderColor
                    foreach ($Data in $DataTable) {
                        foreach ($Key in $Data.Keys) {
                            New-AdaptiveTextBlock -Text $Data[$Key] -Separator
                        }
                    }
                } -Width Stretch
            }
        }
    } else {
        New-AdaptiveColumnSet {
            for ($i = 0; $i -lt $DataTable[0].PSObject.Properties.Name.Count; $i++) {
                New-AdaptiveColumn {
                    $HeaderText = $DataTable[0].PSObject.Properties.Name[$i]
                    New-AdaptiveTextBlock -Text $HeaderText -Weight $HeaderWeight -Color $HeaderColor
                    for ($j = 0; $j -lt $DataTable.Count; $j++) {
                        $Test = $DataTable[$j].PSObject.Properties.Value[$i]
                        New-AdaptiveTextBlock -Text $Test -Separator
                    }
                } -Width Stretch
            }
        }
    }
}