function New-TeamsSection {
    [alias('TeamsSection')]
    [CmdletBinding()]
    param (
        [scriptblock] $SectionInput,
        [string] $Title,
        [string] $ActivityTitle,
        [string] $ActivitySubtitle ,
        [string] $ActivityImageLink,
        [string][ValidateSet('Alert', 'Cancel', 'Disable', 'Download', 'Minus', 'Check', 'Add', 'None')] $ActivityImage = 'None',
        [string] $ActivityText,
        [System.Collections.IDictionary[]]$ActivityDetails,
        [System.Collections.IDictionary[]]$Buttons,
        [switch] $StartGroup
    )
    if ($ActivityImage -ne 'None') {
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

    $Section = [ordered] @{ }
    if ($Title) {
        $Section.title = $Title
    }
    if ($ActivityTitle) {
        $Section.activityTitle = "$($ActivityTitle)"
    }
    if ($ActivitySubtitle) {
        $Section.activitySubtitle = "$($ActivitySubtitle)"
    }
    if ($ActivityImageLink) {
        $Section.activityImage = "$($ActivityImageLink)"
    }
    if ($ActivityText) {
        $Section.activityText = "$($ActivityText)"
    }

    if ($StartGroup) {
        $Section.startGroup = $startGroup.IsPresent
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