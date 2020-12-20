Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card _Title_ 1' -Color Warning # this will trigger markdown
            New-AdaptiveTextBlock -Size 'Medium' -Text "Test Card \_Title_ 1" -Color Warning # This works
            New-AdaptiveTextBlock -Size 'Medium' -Text "Test Card \_Title\_ 1" -Color Warning # This works
            $Text = "Test Card _Title_ 1" -replace '_','\_'
            New-AdaptiveTextBlock -Size 'Medium' -Text $Text -Color Warning # This works
        }
    }
} -Verbose