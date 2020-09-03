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

        [ValidateScript({
            if (-not ($_ | Test-Path)) {
                throw "ActivityImagePath is inaccessible or does not exist"
            }
            if (-not ($_ | Test-Path -PathType Leaf) -or ($_ -notmatch "(\.jpg|\.png)")) {
                throw "ActivityImagePath is not a file or file extension is not supported"
            }
            return $true
        })]
        [System.IO.FileInfo] $ActivityImagePath,

        [string] $ActivityText,
        [string] $Text,
        [System.Collections.IDictionary[]]$ActivityDetails,
        [System.Collections.IDictionary[]]$Buttons,
        [switch] $StartGroup
    )

    if ($ActivityImagePath) { # ActivityImagePath takes precedence over ActivityImage
        $FilePath = [System.IO.Path]::GetDirectoryName($ActivityImagePath)
        $FileBaseName = [System.IO.Path]::GetFileNameWithoutExtension($ActivityImagePath)
        $FileExtension = [System.IO.Path]::GetExtension($ActivityImagePath)
        $ActivityImageLink = Get-Image -PathToImages $FilePath -FileName $FileBaseName -FileExtension $FileExtension # -Verbose
    }
    elseif ($ActivityImage -ne 'None') {
        $StoredImages = [IO.Path]::Combine($PSScriptRoot, '..', 'Images')
        $ActivityImageLink = Get-Image -PathToImages $StoredImages -FileName $ActivityImage -FileExtension '.jpg' # -Verbose
    }

    $ButtonsList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()
    $FactList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()
    $ImagesList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()
    $ImageHeroList = [System.Collections.Generic.List[System.Collections.IDictionary]]::new()

    if ($SectionInput) {
        $SectionOutput = & $SectionInput
        foreach ($_ in $SectionOutput) {
            if ($_.Type -eq 'button') {
                $_.Remove('Type')
                $ButtonsList.Add($_)
            } elseif ($_.Type -eq 'fact') {
                $_.Remove('Type')
                $FactList.Add($_)
            } elseif ($_.Type -eq 'image') {
                $_.Remove('Type')
                $ImagesList.Add($_)
            } elseif ($_.Type -eq 'HeroImageWorkaround') {
                $ImageHeroList.Add($_)
            } elseif ($_.Type -eq 'ActivityTitle') {
                $ActivityTitle = $_.ActivityTitle
            } elseif ($_.Type -eq 'ActivitySubtitle') {
                $ActivitySubtitle = $_.ActivitySubtitle
            } elseif ($_.Type -eq 'ActivityImageLink') {
                $ActivityImageLink = $_.ActivityImageLink
            } elseif ($_.Type -eq 'ActivityText') {
                $ActivityText = $_.ActivityText
            } elseif ($_.Type -eq 'ActivityImage') {
                $ActivityImageLink = $_.ActivityImageLink
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


    # $section.heroImage = @{ image = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg" }

    if ($Text -or $ImageHeroList.Count -gt 0) {
        if ($ImageHeroList.Count -gt 0) {
            [string] $TextBundle = @(
                foreach ($_ in $ImageHeroList) {
                    $_.Image
                }
                if ($Text) {
                    $Text
                }
            )
        } else {
            [string] $TextBundle = $Text
        }
        $section.text = $TextBundle
    }
    if ($ImagesList.Count -gt 0) {
        $section.images = @( $ImagesList )
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
