function New-TeamsActivityText {
    [CmdletBinding()]
    [alias('ActivityText','TeamsActivityText')]
    param(
        [string] $Text
    )
    @{
        ActivityText = $Text
        Type         = 'ActivityText'
    }
}