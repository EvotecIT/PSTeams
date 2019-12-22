Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

$TeamsID = 'https://outlook.office.com/webhook/a5c7c'

Send-TeamsMessage -Verbose -MessageText 'Test' {
    New-TeamsSection -ActivityTitle "**Elon Musk**"
    New-TeamsSection -ActivityTitle "Elon Musk"
} -Uri $TeamsID -Color DarkSeaGreen -MessageSummary 'Tweet'