function New-CardListItem {
    [cmdletBinding()]
    param(
        [parameter(Mandatory)][ValidateSet('file', 'resultItem', 'section', 'person')][string] $Type,
        [string] $Icon,
        [string] $Title,
        [string] $SubTitle,
        [ValidateSet('whois', 'editOnline')][string] $TapAction,
        [ValidateSet('imBack', 'openUrl', 'file')][string] $TapType,
        [string] $TapValue
    )
    $TeamsObject = [ordered] @{
        type     = $Type
        id       = if ($TapAction) { $TapValue } else { '' }
        title    = $Title
        subtitle = $SubTitle
        icon     = $Icon
        tap      = @{
            type  = $TapType
            value = "$TapAction $TapValue".Trim()
        }
    }
    Remove-EmptyValue -Hashtable $TeamsObject -Recursive -Rerun 2
    $TeamsObject
}