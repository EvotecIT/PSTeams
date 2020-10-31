function New-AdaptiveFact {
    [cmdletBinding()]
    param(
        [string] $Title,
        [string] $Value
    )

    $Fact = [ordered] @{
        title = "$Title"
        value = "$Value"
        #type  = 'fact' # this is only needed for module to process this correctly. JSON doesn't care
    }
    $Fact
}