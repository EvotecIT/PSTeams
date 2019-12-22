function New-TeamsActivityTitle {
    [CmdletBinding()]
    [alias('ActivityTitle', 'TeamsActivityTitle')]
    param(
        [string] $Title
    )
    @{
        ActivityTitle = $Title
        Type          = 'ActivityTitle'
    }

}