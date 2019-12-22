function New-TeamsImage {
    [alias('TeamsImage')]
    [CmdletBinding()]
    param(
        [alias('Url', 'Uri')] $Link
    )
    if ($Link) {
        [ordered] @{
            image = $Link
            type  = 'image'
        }
    }
}