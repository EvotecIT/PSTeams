function New-TeamsSection {
    [CmdletBinding()]
    param (
        [scriptblock] $SectionInput,
        [string] $Title,
        [string] $ActivityTitle,
        [string] $ActivitySubtitle ,
        [string] $ActivityImageLink,
        [ImageType] $ActivityImage = [ImageType]::None,
        [string] $ActivityText,
        [System.Collections.IDictionary[]]$ActivityDetails,
        [System.Collections.IDictionary[]]$Buttons
    )
    if ($ActivityImage -ne [ImageType]::None) {
        $StoredImages = [IO.Path]::Combine("$(Split-Path -Path $PSScriptRoot -Parent)", "Images")
        $ActivityImageLink = Get-Image -PathToImages $StoredImages -FileName $ActivityImage -FileExtension '.jpg' # -Verbose
    }

    $ButtonsList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()
    $FactList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()

    if ($SectionInput) {
        $SectionOutput = & $SectionInput
        foreach ($_ in $SectionOutput) {
            if ($_.Type -eq 'button') {
                $ButtonsList.Add($_)
            } elseif ($_.Type -eq 'fact') {
                $FactList.Add($_)
            }
        }
    }

    $Section = [ordered] @{
        title            = $Title
        activityTitle    = "$($ActivityTitle)"
        activitySubtitle = "$($ActivitySubtitle)"
        activityImage    = "$($ActivityImageLink)"
        activityText     = "$($ActivityText)"
    }
    if ($null -ne $ActivityDetails -or $FactList.Count -gt 0) {
        $Section.facts = @(
            if ($SectionInput) {
                $FactList
            } else {
                $ActivityDetails
            }
        )
    }
    if ($null -ne $Buttons -or $ButtonsList.Count -gt 0) {
        $Section.potentialAction = @(
            if ($SectionInput) {
                $ButtonsList
            } else {
                $Buttons
            }
        )
    }
    return $Section
}