function New-AdaptiveAction {
    [cmdletBinding()]
    param(
        [scriptblock] $Body,
        [scriptblock] $Actions,
        [ValidateSet('Action.ShowCard')][string] $Type = 'Action.ShowCard',
        [string] $Title
    )
    $TeamObject = [ordered] @{
        type  = $Type
        title = $Title
        card  = [ordered]@{}
    }
    if ($Body) {
        $TeamObject['card']['body'] = & $Body
    }
    if ($Actions) {
        $TeamObject['card']['actions'] = & $Actions
    }
    Remove-EmptyValue -Hashtable $TeamObject
    $TeamObject
}