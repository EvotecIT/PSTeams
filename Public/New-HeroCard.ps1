function New-HeroCard {
    <#
    .SYNOPSIS
    Provides a quick and easy way to build a Hero Card and send it thru incoming webhook to Microsoft Teams.

    .DESCRIPTION
    Provides a quick and easy way to build a Hero Card and send it thru incoming webhook to Microsoft Teams.

    .PARAMETER Content
    ScriptBlock to define the content of HeroCard

    .PARAMETER Title
    Title of the hero card

    .PARAMETER SubTitle
    Subtitle of the hero card

    .PARAMETER Text
    Text to display within hero card

    .PARAMETER Uri
    URI/URL of Incoming Webhook generated in Microsoft Teams

    .EXAMPLE
    New-HeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text "The Seattle Center Monorail is an elevated train line between Seattle Center (near the Space Needle) and downtown Seattle. It was built for the 1962 World's Fair. Its original two trains, completed in 1961, are still in service." {
        New-HeroImage -Url 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg'
        New-HeroButton -Type openUrl -Title 'Official website' -Value 'https://www.seattlemonorail.com'
        New-HeroButton -Type openUrl -Title 'Wikipeda page' -Value 'https://www.seattlemonorail.com'
        New-HeroButton -Type imBack -Title 'Evotec page' -Value 'https://www.evotec.xyz'
    } -Uri $URI

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][scriptblock] $Content,
        [string] $Title,
        [string] $SubTitle,
        [string] $Text,
        [string] $Uri
    )
    if ($Content) {
        $Buttons = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $Images = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $ExecutedContent = & $Content
        foreach ($E in $ExecutedContent) {
            if ($E.Value) {
                if ($Buttons.Count -lt 3) {
                    $Buttons.Add($E)
                } else {
                    Write-Warning "New-HeroCard - Herd Card support only up to 3 buttons."
                }
            } else {
                if ($Images.Count -lt 2) {
                    $Images.Add($E)
                } else {
                    Write-Warning "New-HeroCard - Herd Card support only 1 image."
                }
            }
        }

        $Wrapper = @{
            contentType = "application/vnd.microsoft.card.hero"
            content     = @{
                title    = $Title
                subTitle = $SubTitle
                text     = $Text
                images   = @(
                    $Images
                )
                buttons  = @(
                    $Buttons
                )
            }
        }
    }
    $Body = $Wrapper | ConvertTo-Json -Depth 20
    if ($Uri) {
        Send-TeamsMessageBody -Uri $URI -Body $Body -Wrap
    } else {
        $Body
    }
}