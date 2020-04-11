param (
    $TeamsID = $Env:TEAMSPESTERID
)
#Requires -Modules Pester
Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

Describe 'Send-TeamsMessage - Should send messages properly with new syntax' {
    It 'Given 3 facts, 1 section should not throw' {
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
        }
    }
}

Describe "ConvertTo-TeamsFact - Should convert hash tables and PSObjects to Teams facts" {
    It "Given two values, 1 section should not throw" {
        $Files = Get-ChildItem $HOME | Sort-Object CreationTime -Descending | Select-Object Name, CreationTime -First 1
        Send-TeamsMessage -Uri $TeamsID  -MessageTitle 'PSTeams - Pester Test for ConvertTo-TeamsFact' -Sections (New-TeamsSection -Title 'Latest file' -Text 'Here are the details about the latest file created in the specified directory' -ActivityDetails ($Files | ConvertTo-TeamsFact))
    }
}