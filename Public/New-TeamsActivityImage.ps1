function New-TeamsActivityImage {
    <#
    .SYNOPSIS
    Adds ability to add an image to an activity within Office 365 Connector Card

    .DESCRIPTION
    Adds ability to add an image to an activity within Office 365 Connector Card

    .PARAMETER Image
    Choose one of built-in images to display. Options are: 'Add', 'Alert', 'Cancel', 'Check', 'Disable', 'Download', 'Info', 'Minus', 'Question', 'Reload', 'None'

    .PARAMETER Link
    Provide a link to image to display in the card.

    .PARAMETER Path
    Provide a path to image to display in the card. JPG and PNG files are supported.

    .EXAMPLE
    Send-TeamsMessage -URI $TeamsID -MessageTitle 'PSTeams - Pester Test' -MessageText "This text will show up" -Color DodgerBlue {
        New-TeamsSection {
            New-TeamsActivityTitle -Title "**PSTeams**"
            New-TeamsActivitySubtitle -Subtitle "@PSTeams - $CurrentDate"
            New-TeamsActivityImage -Image Add
            New-TeamsActivityText -Text "This message proves PSTeams Pester test passed properly."
            New-TeamsFact -Name 'PS Version' -Value "**$($PSVersionTable.PSVersion)**"
            New-TeamsFact -Name 'PS Edition' -Value "**$($PSVersionTable.PSEdition)**"
            New-TeamsFact -Name 'OS' -Value "**$($PSVersionTable.OS)**"
            New-TeamsButton -Name 'Visit English Evotec Website' -Link "https://evotec.xyz"
        }
    } -Verbose

    .NOTES
    General notes
    #>
    [CmdletBinding(DefaultParameterSetName = 'Link')]
    [alias('ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage')]
    param(
        [Parameter(ParameterSetName = 'Image')][string][ValidateSet('Add', 'Alert', 'Cancel', 'Check', 'Disable', 'Download', 'Info', 'Minus', 'Question', 'Reload', 'None')] $Image,
        [Parameter(ParameterSetName = 'Link')][string] $Link,

        [Parameter(ParameterSetName = 'Path')]
        [ValidateScript({
                if (-not ($_ | Test-Path)) {
                    throw "Path is inaccessible or does not exist"
                }
                if (-not ($_ | Test-Path -PathType Leaf) -or ($_ -notmatch "(\.jpg|\.png)")) {
                    throw "Path is not a file or file extension is not supported"
                }
                return $true
            })]
        [System.IO.FileInfo] $Path
    )
    if ($Path) {
        $FilePath = [System.IO.Path]::GetDirectoryName($Path)
        $FileBaseName = [System.IO.Path]::GetFileNameWithoutExtension($Path)
        $FileExtension = [System.IO.Path]::GetExtension($Path)
        @{
            ActivityImageLink = Get-Image -PathToImages $FilePath -FileName $FileBaseName -FileExtension $FileExtension -Verbose
            type              = 'ActivityImage'
        }
    } elseif ($Image) {
        if ($Image -ne 'None') {
            $StoredImages = [IO.Path]::Combine($PSScriptRoot, '..', 'Images')
            @{
                ActivityImageLink = Get-Image -PathToImages $StoredImages -FileName $Image -FileExtension '.jpg' # -Verbose
                type              = 'ActivityImage'
            }
        }
    } else {
        @{
            ActivityImageLink = $Link
            Type              = 'ActivityImageLink'
        }
    }
}
