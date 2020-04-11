function ConvertTo-TeamsFact {
    <#
    .SYNOPSIS
    Convert a PSCustomObject or a Hashtable to Teams facts.
    
    .DESCRIPTION
    Teams facts are name-value pairs. This module helps convert a PSObject or a Hashtable to Teams facts (only one level deep).
    
    .PARAMETER InputObject
    The Hashtable or PSObject that is output by another cmdlet.
    
    .EXAMPLE
    Get-ChildItem | Select-Object -First 1 | ConvertTo-TeamsFact

    .EXAMPLE
    @{ Product = 'Microsoft Teams'; Developer = 'Microsoft Corporation'; ReleaseYear = '2018' } | ConvertTo-TeamsFact
    
    .NOTES
    Ram Iyer (https://ramiyer.me)
    #>
    
    [CmdletBinding()]
    param (
        # The input object
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        $InputObject
    )

    begin { }
    process {
        if ($InputObject -is [hashtable]) {
            $Facts = foreach ($Key in $InputObject.Keys) {
                New-TeamsFact -Name $Key -Value $InputObject.$Key
            }
        }
        elseif (($InputObject -is [int]) -or ($InputObject -is [long]) -or ($InputObject -is [string]) -or ($InputObject -is [char]) -or ($InputObject -is [bool]) -or ($InputObject -is [byte]) -or ($InputObject -is [double]) -or ($InputObject -is [decimal]) -or ($InputObject -is [single]) -or ($InputObject -is [array]) -or ($InputObject -is [xml])) {
            Write-Error -Message 'The input is neither a PSObject nor a Hashtable. Operation aborted.' -Category InvalidData
            break
        }
        elseif ($InputObject -is [psobject]) {
            $Facts = foreach ($Property in $InputObject.PsObject.Properties) {
                New-TeamsFact -Name $Property.Name -Value $Property.Value
            }
        }
        $Facts
    }
}
Get-Help ConvertTo-TeamsFact