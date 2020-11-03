Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveContainer {
        New-AdaptiveTextBlock -Text "Microsoft Corporation" -Size Medium -Wrap
        New-AdaptiveTextBlock -Text "Nasdaq Global Select: MSFT" -Subtle -Spacing None -Wrap
        New-AdaptiveTextBlock -Text "Fri, May 3, 2019 1:00 PM"
    }
    New-AdaptiveContainer {
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Text "128.90" -Size ExtraLarge
                New-AdaptiveTextBlock -Text "▲ 2.69 USD (2.13%)" -Color Good -Spacing None
            } -Width Stretch
            New-AdaptiveColumn {
                New-AdaptiveFactSet {
                    New-AdaptiveFact -Title 'Open' -Value '127.42'
                    New-AdaptiveFact -Title 'High' -Value '129.43'
                    New-AdaptiveFact -Title 'Low' -Value '127.25'
                }
            } -Width Auto
        }
    } -Spacing None
} -Speak 'Microsoft stock is trading at $62.30 a share, which is down .32%' -Verbose