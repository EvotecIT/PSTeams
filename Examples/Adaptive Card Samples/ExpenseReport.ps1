Import-Module .\PSTeams.psd1 -Force

# Based on: https://adaptivecards.io/samples/ExpenseReport.html

New-AdaptiveCard -Uri $Env:TEAMSPESTERID {
    New-AdaptiveContainer -Style Emphasis -Bleed {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn -Width Stretch {
                New-AdaptiveTextBlock -Text '**EXPENSE APPROVAL**' -Weight Bolder -Size Large
            }
            New-AdaptiveColumn -Width Auto {
                New-AdaptiveImage -Url "https://adaptivecards.io/content/pending.png" -AlternateText 'Pending' -HeightInPixels 30 #-HorizontalAlignment Right
            }
        }
    }

    New-AdaptiveContainer {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn -Width Stretch {
                New-AdaptiveTextBlock -Text 'Trip to UAE' -Wrap -Size ExtraLarge
            }
            New-AdaptiveColumn -Width Auto {
                New-AdaptiveActionSet {
                    New-AdaptiveAction -Title 'LINK TO CLICK' -ActionUrl 'https://adaptivecards.io'
                }
            }
        }
    }
    New-AdaptiveTextBlock -Text "[ER-13052](https://adaptivecards.io)" -Spacing Small -Size Small -Weight Bolder -Color Accent

    New-AdaptiveFactSet -Spacing Large {
        New-AdaptiveFact -Title "Submitted By" -Value "**Matt Hidinger**  matt@contoso.com"
        New-AdaptiveFact -Title "Duration" -Value "2019-06-19 - 2019-06-21"
        New-AdaptiveFact -Title "Submitted On" -Value "2019-04-14"
        New-AdaptiveFact -Title "Reimbursable Amount" -Value '$ 400.00'
        New-AdaptiveFact -Title "Awaiting approval from" -Value "**Thomas**  thomas@contoso.com"
        New-AdaptiveFact -Title "Submitted to" -Value "**David**  david@contoso.com"
    }

    New-AdaptiveContainer -Style Emphasis -Spacing Large {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text 'DATE' -Weight Bolder
            } -Width Auto
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text 'CATEGORY' -Weight Bolder
            } -Width Stretch
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text 'AMOUNT' -Weight Bolder
            } -Width Auto
        }
    } -Bleed

    New-AdaptiveColumnSet {
        New-AdaptiveColumn -Width Auto -Spacing Medium {
            New-AdaptiveTextBlock -Text '06-19' -Wrap
        }
        New-AdaptiveColumn -Width Stretch {
            New-AdaptiveTextBlock -Text 'Air Travel Expense' -Wrap
        }
        New-AdaptiveColumn -Width Auto {
            New-AdaptiveTextBlock -Text '$300.00' -Wrap
        }
        # Special column with Action and hidden items
        # Notice ActionTargetElement which triggers automatically ToggleVisibility for those mentioned
        New-AdaptiveColumn -Spacing Small -VerticalContentAlignment Center -Width Auto {
            New-AdaptiveImage -Id 'chevronDown1' -Url "https://adaptivecards.io/content/down.png" -WidthInPixels 20 -AlternateText "Details collapsed"
            New-AdaptiveImage -Id 'chevronUp1' -Url "https://adaptivecards.io/content/up.png" -WidthInPixels 20 -AlternateText "Details collapsed" -Hidden
        } -SelectActionTargetElement 'cardContent1', 'chevronDown1', 'chevronUp1'
    }

    # Notice this will be hidden initially and shown with the action from above
    New-AdaptiveContainer -Hidden -Id 'cardContent1' {
        New-AdaptiveTextBlock -Text '* Leg 1 on Tue, Jun 19th, 2019 at 6:00 AM.' -Subtle -Wrap
        New-AdaptiveTextBlock -Text '* Leg 2 on Tue, Jun 19th, 2019 at 7:15 PM.' -Subtle -Wrap
        New-AdaptiveContainer -Style Good {
            # This should be an input type, but not yet added - not sure if it makes sense, as inputs are not working for webhooks
            New-AdaptiveTextBlock -Text 'Some more data in good color' -Subtle -Wrap
        }
    }

    New-AdaptiveColumnSet {
        New-AdaptiveColumn -Width Auto -Spacing Medium {
            New-AdaptiveTextBlock -Text '06-19' -Wrap
        }
        New-AdaptiveColumn -Width Stretch {
            New-AdaptiveTextBlock -Text 'Auto Mobile Expense' -Wrap
        }
        New-AdaptiveColumn -Width Auto {
            New-AdaptiveTextBlock -Text '$100.00' -Wrap
        }
        # Special column with Action and hidden items
        # Notice ActionTargetElement which triggers automatically ToggleVisibility for those mentioned
        New-AdaptiveColumn -Spacing Small -VerticalContentAlignment Center -Width Auto {
            New-AdaptiveImage -Id 'chevronDown2' -Url "https://adaptivecards.io/content/down.png" -WidthInPixels 20 -AlternateText "Details collapsed"
            New-AdaptiveImage -Id 'chevronUp2' -Url "https://adaptivecards.io/content/up.png" -WidthInPixels 20 -AlternateText "Details collapsed" -Hidden
        } -SelectActionTargetElement 'cardContent2', 'chevronDown2', 'chevronUp2'
    }

    # Notice this will be hidden initially and shown with the action from above
    New-AdaptiveContainer -Hidden -Id 'cardContent2' {
        New-AdaptiveTextBlock -Text '* Contoso Car Rentrals, Tues 6/19 at 7:00 AM' -Subtle -Wrap
        New-AdaptiveContainer -Style Warning {
            # This should be an input type, but not yet added - not sure if it makes sense, as inputs are not working for webhooks
            New-AdaptiveTextBlock -Text 'Some more data in warning color' -Subtle -Wrap
        }
    }

    New-AdaptiveColumnSet {
        New-AdaptiveColumn -Width Auto -Spacing Medium {
            New-AdaptiveTextBlock -Text '06-21' -Wrap
        }
        New-AdaptiveColumn -Width Stretch {
            New-AdaptiveTextBlock -Text 'Excess Baggage Cost' -Wrap
        }
        New-AdaptiveColumn -Width Auto {
            New-AdaptiveTextBlock -Text '$50.38' -Wrap
        }
        # Special column with Action and hidden items
        # Notice ActionTargetElement which triggers automatically ToggleVisibility for those mentioned
        New-AdaptiveColumn -Spacing Small -VerticalContentAlignment Center -Width Auto {
            New-AdaptiveImage -Id 'chevronDown3' -Url "https://adaptivecards.io/content/down.png" -WidthInPixels 20 -AlternateText "Details collapsed"
            New-AdaptiveImage -Id 'chevronUp3' -Url "https://adaptivecards.io/content/up.png" -WidthInPixels 20 -AlternateText "Details collapsed" -Hidden
        } -SelectActionTargetElement 'cardContent3', 'chevronDown3', 'chevronUp3'
    }

    # Notice this will be hidden initially and shown with the action from above
    New-AdaptiveContainer -Hidden -Id 'cardContent3' {
        New-AdaptiveTextBlock -Text 'More data' -Subtle -Wrap
        New-AdaptiveContainer -Style Attention {
            # This should be an input type, but not yet added - not sure if it makes sense, as inputs are not working for webhooks
            New-AdaptiveTextBlock -Text 'Some more data in warning color' -Subtle -Wrap
        }
    }

    New-AdaptiveColumnSet -Spacing Large -Separator {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Text "Total Expense Amount" -Wrap -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text 'Non-reimbursable Amount' -Wrap -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text 'Advance Amount' -Wrap -HorizontalAlignment Right
        } -Width Stretch
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Text '$450.38' -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text '(-) 50.38' -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text '(-) 0.00' -HorizontalAlignment Right
        } -Width Auto
    }

    New-AdaptiveContainer -Style Emphasis {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text 'Amount to be Reimbursed' -Wrap -HorizontalAlignment Right
            } -Width Stretch
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text '$ 400.00' -Weight Bolder
            } -Width Auto
        }
    } -Bleed

    New-AdaptiveColumnSet {
        New-AdaptiveColumn -VerticalContentAlignment Center -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'Show history' -Wrap -HorizontalAlignment Right -Id 'showHistory' -Color Accent
            New-AdaptiveTextBlock -Text 'Hide history' -Wrap -HorizontalAlignment Right -Id 'hideHistory' -Color Accent -Hidden
        } -SelectActionTargetElement 'cardContent4', 'showHistory', 'hideHistory'
    }

    New-AdaptiveContainer -id 'cardContent4' -Hidden {
        New-AdaptiveTextBlock -Text '* Expense submitted by **Matt Hidinger** on Mon, Jul 15, 2019' -Subtle -Wrap
        New-AdaptiveTextBlock -Text '* Expense approved by **Thomas** on Mon, Jul 15, 2019' -Subtle -Wrap
    }
} -Action {
    # This won't really work as submit doesn't work in
    New-AdaptiveAction -Type Action.Submit -Title 'Approve'
    New-AdaptiveAction -Type Action.Submit -Title 'Reject'
} -Verbose
