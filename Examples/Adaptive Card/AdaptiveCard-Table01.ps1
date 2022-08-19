Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard {
    #New-TeamsText -Size 'Medium' -Weight Bolder -Text 'Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.' -Separator -Wrap
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'Board:1' -Value 'Adaptive Cards1'
                New-AdaptiveFact -Title 'Board:2' -Value 'Adaptive Cards2'
                New-AdaptiveFact -Title 'Board:4' -Value 'Adaptive Cards3'
                New-AdaptiveFact -Title 'Board:5' -Value 'Adaptive Cards4'
            }
        } -Width stretch
        New-AdaptiveColumn {
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'Board:1' -Value 'Adaptive Cards1'
                New-AdaptiveFact -Title 'Board:2' -Value 'Adaptive Cards2'
                New-AdaptiveFact -Title 'Board:4' -Value 'Adaptive Cards3'
                New-AdaptiveFact -Title 'Board:5' -Value 'Adaptive Cards4'
            }
        } -Width stretch
    }
    New-AdaptiveRichTextBlock -HorizontalAlignment left
    New-AdaptiveTextBlock -Text 'Dear <at>przemyslawklys</at>' -Subtle -Spacing None
    #New-AdaptiveTextBlock -Text '<at>Przemysław</at>'
} -Uri $Env:TEAMSPESTERID