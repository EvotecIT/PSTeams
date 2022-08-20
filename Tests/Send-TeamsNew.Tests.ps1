param(
    $TeamsID = $Env:TEAMSPESTERID
)

Describe 'Send-TeamsMessage - Should send messages properly with new syntax' {
    It 'Given 3 facts, 1 section should not throw' {
        Send-TeamsMessage -Uri $TeamsID -MessageTitle 'PSTeams - Pester Test' -MessageText "This text will show up" -Color DodgerBlue {
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
        } -ErrorAction Stop
    } -TestCases @{ TeamsID = $TeamsID }
}