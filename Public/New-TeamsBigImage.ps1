function New-TeamsBigImage {
    [alias('TeamsBigImage')]
    [CmdletBinding()]
    param(
        [alias('Url', 'Uri')] $Link,
        [string] $AlternativeText = 'Alternative Text'
    )
    if ($Link) {
        [ordered] @{
            image = "![$AlternativeText]($Link)"
            type  = 'HeroImageWorkaround'
        }
    }
}