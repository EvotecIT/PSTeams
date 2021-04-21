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
} -BackgroundUrl 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png' -MinimumHeight 450 -Verbose -FullWidth