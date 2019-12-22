Send-TeamsMessage -Verbose -Color DimGray {
    New-TeamsSection -Title 'This is 2nd section within 1 message' {
        New-TeamsList -Name 'Testing List' {
            New-TeamsListItem -Text 'First ordered list item' -Level 0
            New-TeamsListItem -Text 'Another item' -Level 0  -Numbered
        }
    }
} -Uri $TeamsID -MessageSummary 'Test1' #-MessageText 'Test2' -MessageTitle 'Test1' #