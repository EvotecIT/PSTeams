function ConvertTo-TeamsFact {
    <#
    .SYNOPSIS
    Convert a PSCustomObject or a Hashtable to Teams facts.

    .DESCRIPTION
    Teams facts are name-value pairs. This function helps convert a PSObject or a Hashtable to Teams facts (only one level deep).

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
    foreach ($Object in $InputObject) {
        if ($Object -is [System.Collections.IDictionary]) {
            $Facts = foreach ($Key in $Object.Keys) {
                New-TeamsFact -Name $Key -Value $Object.$Key
            }
            #} elseif (($Object -is [int]) -or ($Object -is [long]) -or ($Object -is [string]) -or ($Object -is [char]) -or ($Object -is [bool]) -or ($Object -is [byte]) -or ($Object -is [double]) -or ($Object -is [decimal]) -or ($Object -is [single]) -or ($Object -is [array]) -or ($Object -is [xml])) {
        } elseif ($Object.GetType().Name -match 'bool|byte|char|datetime|decimal|double|xml|float|int|long|sbyte|short|string|timespan|uint|ulong|URI|ushort') {
            # Because PowerShell implicitly converts datatypes to PSObject
            Write-Error -Message 'The input is neither a PSObject nor a Hashtable. Operation aborted.' -Category InvalidData -ErrorAction Stop
        } else {
            # Assumes that the input is a PSObject; anyway there would be an implicit conversion if not caught in the previous block
            $Facts = foreach ($Property in $Object.PsObject.Properties) {
                New-TeamsFact -Name $Property.Name -Value $Property.Value
            }
        }
        $Facts
    }
}