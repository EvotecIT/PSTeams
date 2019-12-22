function New-TeamsListItem {
    [alias('TeamsListItem')]
    [CmdletBinding()]
    param(
        [string] $Text,
        [int] $Level,
        [switch] $Numbered
    )
    [ordered] @{
        Text     = $Text
        Level    = $Level
        Numbered = $Numbered.IsPresent
        Type     = 'ListItem'
    }
}