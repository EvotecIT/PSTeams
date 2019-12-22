function New-TeamsButton {
    [alias('TeamsButton')]
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
        type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
    }
    return $Button
}