function New-CardListButton {
    [alias('New-HeroButton', 'New-ThumbnailButton')]
    [cmdletBinding()]
    param(
        [ValidateSet('imBack', 'openUrl', 'file')][string] $Type,
        [string] $Title,
        [string] $Value,
        [string] $Image
    )
    if ($Image) {
        Write-Warning "Using Image for Buttons while technically supported by Teams, it's not supported by Teams Connectors. Leaving this in place just in case it starts working"
    }
    $TeamsObject = [ordered] @{
        "type"  = $Type
        "title" = $Title
        "value" = $Value
        "image" = $Image
    }
    Remove-EmptyValue -Hashtable $TeamsObject
    $TeamsObject
}