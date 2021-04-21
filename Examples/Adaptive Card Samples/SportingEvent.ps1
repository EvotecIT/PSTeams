Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveContainer {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveImage -Url "https://adaptivecards.io/content/cats/3.png" -Size Medium -AlternateText "Shades cat team emblem" -HorizontalAlignment Center
                New-AdaptiveTextBlock -Weight Bolder -Text 'SHADES' -HorizontalAlignment Center
            } -Width Auto
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text "Sat, Aug 31, 2019" -HorizontalAlignment Center -Wrap
                New-AdaptiveTextBlock -Text "Final" -Spacing None -HorizontalAlignment Center
                New-AdaptiveTextBlock -Text "45 - 7" -HorizontalAlignment Center -Size ExtraLarge
            } -Width Stretch -Separator -Spacing Medium
            New-AdaptiveColumn {
                New-AdaptiveImage -Url "https://adaptivecards.io/content/cats/2.png" -Size Medium -HorizontalAlignment Center -AlternateText "Skins cat team emblem"
                New-AdaptiveTextBlock -Weight Bolder -Text 'SKINS' -HorizontalAlignment Center
            } -Width Auto -Separator -Spacing Medium
        }
    }
} -Speak 'The Seattle Seahawks beat the Carolina Panthers 40-7' -Verbose -Width Full