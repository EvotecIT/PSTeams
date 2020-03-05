function ConvertTo-TeamsFact {
    <#
    .SYNOPSIS
    Convert a PSCustomObject to Teams facts.
    
    .DESCRIPTION
    Teams facts are a name-value pair. This module helps convert a PSObject to Teams facts (only string values, one level deep).
    
    .PARAMETER InputObject
    The PSObject that is output by another cmdlet.
    
    .EXAMPLE
    Get-ChildItem | Select-Object -First 1 | ConvertTo-TeamsFact
    
    .NOTES
    Ram Iyer (ram.iyer@merckgroup.com)
    #>
    
    [CmdletBinding(DefaultParameterSetName = 'InputObject')]
    param (
        # The input object
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0, ParameterSetName = 'InputObject')]
        [PSObject]
        $InputObject,

        # A dictionary input
        [Parameter(Mandatory = $true, ParameterSetName = 'InputHash')]
        [hashtable]
        $InputHash
    )

    begin { }
    process {
        if ($InputObject) {
            $Facts = foreach ($Property in $InputObject.PsObject.Properties) {
                New-TeamsFact -Name $Property.Name -Value $Property.Value
            }
        }
        elseif ($InputDictionary) {
            $Facts = foreach ($Key in $InputDictionary.Keys) {
                New-TeamsFact -Name $Key -Value $InputDictionary.$Key
            }
        }
        $Facts
    }
}