Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID {
    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Test'
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1'
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1'
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1'
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1'
        } -Width stretch
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size Medium -Text "Test Card Title 5"
            New-AdaptiveTextBlock -Size ExtraLarge -Text "Poll Request" -Color Attention -Weight Bolder -Spacing Medium -HorizontalAlignment Center -Subtle
            New-AdaptiveTextBlock -Size ExtraLarge -Text "Poll Request" -Color Warning -Weight Bolder -Spacing Medium -HorizontalAlignment Center
            New-AdaptiveTextBlock -Size ExtraLarge -Text "Poll Request" -Color Accent -Weight Bolder -Spacing Medium -HorizontalAlignment Center
            @{
                "type"        = "Input.ChoiceSet"
                "placeholder" = "Select from these choices"
                "choices"     = @(
                    @{
                        "title" = "Choice 1"
                        "value" = "Choice 1"
                    }
                    @{
                        "title" = "Choice 2"
                        "value" = "Choice 2"
                    }
                    @{
                        "title" = "Choice 3"
                        "value" = "Choice 3"
                    }
                )
                "id"          = "acPollChoices"
                "style"       = "expanded"
            }
            New-AdaptiveTextBlock -Text 'Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.' -wrap
        } -Width stretch -Style 'Attention'
    } -Style Warning -Separator
    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.' -Separator -Wrap
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
            } -Spacing ExtraLarge
        } -Style Attention
        New-AdaptiveColumn {
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
                New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Cards'
            } -Spacing ExtraLarge
        } -Style Emphasis
    }
}