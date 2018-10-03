function New-TeamsButton {
    [CmdletBinding()]
    param (
        [string] $Name,
        [string] $Link
    )
    $Button = [ordered] @{
        '@context' = 'http://schema.org'
        '@type'    = 'ViewAction'
        name       = "$Name"
        target     = @("$Link")
    }
    return $Button
}