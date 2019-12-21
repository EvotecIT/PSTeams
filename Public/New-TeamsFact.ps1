function New-TeamsFact {
    [CmdletBinding()]
    param (
        [string] $Name,
        [string] $Value
    )
    $Fact = [ordered] @{
        name  = "$Name"
        value = "$Value"
        type  = 'fact' # this is only needed for module to process this correctly. JSON doesn't care
        #wrap = $false
    }
    return $Fact
}