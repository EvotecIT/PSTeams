Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveImage -BackgroundColor AlbescentWhite -Url 'https://devblogs.microsoft.com/powershell/wp-content/uploads/sites/30/2018/09/Powershell_256.png'
        }
        New-AdaptiveColumn {
            New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
        }
    }
} -Action {
    New-AdaptiveAction -Title 'Set due date' -Type Action.Submit
    New-AdaptiveAction -Title 'Comment' -Type Action.OpenUrl -ActionUrl 'https://evotec.xyz'
}