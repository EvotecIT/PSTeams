function Add-TeamsMessageButtons {
    param(
        $buttons
    )
    $PotentialAction = @()
    foreach ($button in $buttons) {
        $PotentialAction += @{
            '@context' = 'http://schema.org'
            '@type'    = 'ViewAction'
            name       = $($button.Name)
            target     = @("$($button.Value)")
        }
    }
    return $PotentialAction
}