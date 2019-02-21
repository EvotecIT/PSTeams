function New-TeamsSection {
    [CmdletBinding()]
    param (
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

    $Section = [ordered] @{
        title            = $Title
        activityTitle    = "$($ActivityTitle)"
        activitySubtitle = "$($ActivitySubtitle)"
        activityImage    = "$($ActivityImageLink)"
        activityText     = "$($ActivityText)"
    }
    if ($null -ne $ActivityDetails) {
        $Section.facts = @(
            $ActivityDetails
        )
    }
    if ($null -ne $Buttons) {
        $Section.potentialAction = @(
            $Buttons
        )
    }
    return $Section
}