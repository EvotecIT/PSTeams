Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Dark
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Light
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Good
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 <at>Name</at>' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 <at>Zenon Jaskuła</at>' -Color Warning
        }
    }
    New-AdaptiveMention -Text 'Zenon Jaskuła' -UserPrincipalName 'przemyslaw.klys@evotec.test'
    New-AdaptiveMention -Text 'Name' -UserPrincipalName 'przemyslaw.klys@evotec.test'
} -Verbose #-SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz'