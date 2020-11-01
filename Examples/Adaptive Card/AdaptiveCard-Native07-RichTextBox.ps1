Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveContainer {
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
    }
    New-AdaptiveContainer {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
            }
            New-AdaptiveColumn {
                New-AdaptiveRichTextBlock -Text 'This is the first inline.', 'We support colors,', 'both regular and subtle. ', 'Monospace too!' -Color Attention, Default, Warning -StrikeThrough $false, $true, $false -Size ExtraLarge, Default, Medium -Italic $false, $false, $true -Subtle $false, $true, $true
                New-AdaptiveRichTextBlock -Text 'This is the first inline.', 'We support colors,', 'both regular and subtle. ', 'Monospace too!' -Color Attention, Default, Warning -StrikeThrough $false, $true, $false -Size ExtraLarge, Default, Medium -Italic $false, $false, $true -Subtle $false, $true, $true
                New-AdaptiveRichTextBlock -Text 'This is the first inline.', 'We support colors,', 'both regular and subtle. ', 'Monospace too!' -Color Attention, Default, Warning -StrikeThrough $false, $true, $false -Size ExtraLarge, Default, Medium -Italic $false, $false, $true -Subtle $false, $true, $true
            }
        }
    }
} -Verbose