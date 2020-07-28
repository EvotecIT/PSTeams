function ConvertTo-TeamsSection {
    <#
    .SYNOPSIS
    Convert an array of PSCustomObject or a Hashtable to separate Teams sections.

    .DESCRIPTION
    Teams sections are chunks of information that appear within a Teams message. This function helps convert an array of PSObject or an array of Hashtables to Teams sections (only one level deep).

    .PARAMETER InputObject
    The Hashtable or PSObject that is output by another cmdlet.

    .EXAMPLE
    Get-ChildItem -Directory | ConvertTo-TeamsSection -SectionTitleProperty Name

    .NOTES
    Ram Iyer (https://ramiyer.me)
    #>
    param (
        # The input object
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        $InputObject,

        # The property to use for title
        [Parameter(Mandatory = $false, Position = 1)]
        [string]
        $SectionTitleProperty
    )

    process {
        #$TotalCount = $InputObject.Count
        #$CurrentCount = 1

        foreach ($Item in $InputObject) {
            $SectionParams = @{
                ActivityDetails = $Item | ConvertTo-TeamsFact
            }
            if ($SectionTitleProperty) {
                $SectionParams.ActivityTitle = "$(($SectionTitleProperty -creplace '([A-Z])', ' $1').Trim()) $($Item.$SectionTitleProperty)"
            }
            New-TeamsSection @SectionParams
        }
    }
}