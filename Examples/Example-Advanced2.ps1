Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

# This is fake TeamsID - you need to use yours
$TeamsID = 'https://outlook.office.ad05-32e40'

Send-TeamsMessage -Verbose -Color DimGray {
    New-TeamsSection -Title 'This is 1st section within 1 message' {
        New-TeamsFact -Name 'Bold' -Value '**Special GPO**'
        New-TeamsFact -Name 'Italic and Bold' -Value '***Other values***'
        New-TeamsFact -Name 'Italic' -Value '*2010-10-10*'
    } #-ActivityTitle "**Przemyslaw Klys**" #-ActivitySubtitle "@przemyslawklys - 9/12/2016 at 5:33pm" -ActivityImageLink "https://github.com/EvotecIT/PSTeams/blob/master/Images/10.png?raw=true" -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732" `
    New-TeamsSection -Title 'This is 2nd section within 1 message' {
        New-TeamsFact -Name 'Bold' -Value '**Special GPO**'
        New-TeamsFact -Name 'Italic and Bold' -Value '***Other values***'
        New-TeamsFact -Name 'Italic' -Value '*2010-10-10*'
        New-TeamsFact -Name 'Link example' -Value "[Microsoft](https://www.microsoft.com)"
        New-TeamsFact -Name 'Other link example' -Value "[Evotec](https://evotec.xyz) and some **bold** text"
        New-TeamsList -Name 'Testing List' {
            New-TeamsListItem -Text 'Test 1' -Level 0 -Numbered
            New-TeamsListItem -Text 'Test 2' -Level 1 -Numbered
            New-TeamsListItem -Text 'Test 3' -Level 1 -Numbered
            New-TeamsListItem -Text 'Test 4' -Level 2 -Numbered
            New-TeamsListItem -Text 'Test 5' -Level 2 -Numbered
            New-TeamsListItem -Text 'Test 6' -Level 2 -Numbered
            New-TeamsListItem -Text 'Test 7' -Level 0 -Numbered
            New-TeamsListItem -Text 'Test 8' -Level 0 -Numbered
        }
        New-TeamsList -Name 'Testing List' {
            New-TeamsListItem -Text 'Test 1' -Level 0
            New-TeamsListItem -Text 'Test 2' -Level 1
            New-TeamsListItem -Text 'Test 3' -Level 1
            New-TeamsListItem -Text 'Test 4' -Level 2
            New-TeamsListItem -Text 'Test 5' -Level 2
            New-TeamsListItem -Text 'Test 6' -Level 2
            New-TeamsListItem -Text 'Test 7' -Level 0
            New-TeamsListItem -Text 'Test 8' -Level 0
        }
        New-TeamsList -Name 'Testing List' {
            New-TeamsListItem -Text 'First ordered list item' -Level 0
            New-TeamsListItem -Text 'Another item' -Level 1  -Numbered
            New-TeamsListItem -Text 'First ordered list item' -Level 2
            New-TeamsListItem -Text 'First ordered list item' -Level 3
            New-TeamsListItem -Text 'Unordered sub-list' -Level 0
            New-TeamsListItem -Text "Actual numbers don't matter, just that it's a number" -Level 0 -Numbered
            New-TeamsListItem -Text 'Ordered sub-list' -Level 1 -Numbered
            New-TeamsListItem -Text 'Another entry' -Level 1 -Numbered
            New-TeamsListItem -Text 'Another entry' -Level 0 -Numbered
            New-TeamsListItem -Text 'Very very long line that I want to show, hellow helow. Very very long line that I want to show, hellow helow. ' -Level 0 -Numbered
        }
    } -Uri $TeamsID -MessageSummary 'Test1' #-MessageText 'Test2' -MessageTitle 'Test1' #
}