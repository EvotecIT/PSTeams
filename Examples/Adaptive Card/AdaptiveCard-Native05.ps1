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
    }
} -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz' -Verbose