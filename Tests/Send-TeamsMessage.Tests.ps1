param(
    $TeamsID = $Env:TEAMSPESTERID
)

Describe 'Send-TeamsMessage - Should send messages properly' {
    It 'Given 1 button, 3 facts, 1 section should not throw' {
        $Button1 = New-TeamsButton -Name 'Visit English Evotec Website' -Link "https://evotec.xyz"

        $Fact1 = New-TeamsFact -Name 'PS Version' -Value "**$($PSVersionTable.PSVersion)**"
        $Fact2 = New-TeamsFact -Name 'PS Edition' -Value "**$($PSVersionTable.PSEdition)**"
        $Fact3 = New-TeamsFact -Name 'OS' -Value "**$($PSVersionTable.OS)**"

        $CurrentDate = Get-Date

        $Section = New-TeamsSection `
            -ActivityTitle "**PSTeams**" `
            -ActivitySubtitle "@PSTeams - $CurrentDate" `
            -ActivityImage Add `
            -ActivityText "This message proves PSTeams Pester test passed properly." `
            -Buttons $Button1 `
            -ActivityDetails $Fact1, $Fact2, $Fact3

        Send-TeamsMessage `
            -Uri $TeamsID `
            -MessageTitle 'PSTeams - Pester Test' `
            -MessageText "This text won't show up" `
            -Color DodgerBlue `
            -Sections $Section -ErrorAction Stop
    } -TestCases @{ TeamsID = $TeamsID }
    It 'Given 3 facts, 1 section should not throw' {
        $Fact1 = New-TeamsFact -Name 'PS Version' -Value "**$($PSVersionTable.PSVersion)**"
        $Fact2 = New-TeamsFact -Name 'PS Edition' -Value "**$($PSVersionTable.PSEdition)**"
        $Fact3 = New-TeamsFact -Name 'OS' -Value "**$($PSVersionTable.OS)**"

        $CurrentDate = Get-Date

        $Section = New-TeamsSection `
            -ActivityTitle "**PSTeams**" `
            -ActivitySubtitle "@PSTeams - $CurrentDate" `
            -ActivityImage Add `
            -ActivityText "This message proves PSTeams Pester test passed properly." `
            -ActivityDetails $Fact1, $Fact2, $Fact3

        Send-TeamsMessage `
            -Uri $TeamsID `
            -MessageTitle 'PSTeams - Pester Test' `
            -MessageText "This text won't show up" `
            -Color DodgerBlue `
            -Sections $Section -ErrorAction Stop
    } -TestCases @{ TeamsID = $TeamsID }
}