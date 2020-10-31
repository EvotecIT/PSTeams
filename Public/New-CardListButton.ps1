function New-CardListButton {
    [alias('New-HeroButton')]
    [cmdletBinding()]
    param(
        [ValidateSet('imBack', 'openUrl', 'file')][string] $Type,
        [string] $Title,
        [string] $Value
    )

    $TeamsObject = [ordered] @{
        "type"  = $Type
        "title" = $Title
        "value" = $Value
    }
    Remove-EmptyValue -Hashtable $TeamsObject
    $TeamsObject
}