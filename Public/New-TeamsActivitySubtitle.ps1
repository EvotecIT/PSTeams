function New-TeamsActivitySubtitle {
    [CmdletBinding()]
    [alias('ActivitySubtitle', 'TeamsActivitySubtitle')]
    param(
        [string] $Subtitle
    )
    @{
        ActivitySubtitle = $Subtitle
        Type             = 'ActivitySubtitle'
    }
}