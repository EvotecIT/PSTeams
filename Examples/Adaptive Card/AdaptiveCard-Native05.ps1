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
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 🤦‍♂️' -Color Good
        }
        New-AdaptiveColumn {
            #New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 <at>Name</at>' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 @Przemysław Kłys <at>Przemysław Kłys</at>' #-Color Warning
        }
    }
    New-AdaptiveMention -Text 'Przemysław Kłys' -UserPrincipalName 'przemyslaw.klys@euvic.pl'
    #New-AdaptiveMention -Text 'Name' -UserPrincipalName 'przemyslaw.klys@evotec.test'
} -Verbose -FullWidth #-SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz'