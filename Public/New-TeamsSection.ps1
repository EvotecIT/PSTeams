function New-TeamsSection {
    [CmdletBinding()]
    param (
        [string] $Title,
        [string] $ActivityTitle,
        [string] $ActivitySubtitle ,
        [string] $ActivityImageLink,
        [ImageType] $ActivityImage = [ImageType]::None,
        [string] $ActivityText,
        [hashtable[]]$ActivityDetails,
        [hashtable[]]$Buttons
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
        facts            = @(
            $ActivityDetails
        )
        potentialAction  = @(
            $Buttons
        )
    }
    return $Section
}