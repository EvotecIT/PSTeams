function New-AdaptiveActionSet {
    [cmdletBinding()]
    param(
        [scriptblock] $Action
    )

    if ($Action) {
        $OutputAction = & $Action
        if ($OutputAction) {
            $TeamObject = [ordered] @{
                type    = 'ActionSet'
                actions = @(
                    $OutputAction
                )
            }
            Remove-EmptyValue -Hashtable $TeamObject
            $TeamObject
        }
    }
}