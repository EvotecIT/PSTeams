Import-Module .\PSTeams.psd1 -Force

$Objects = @(
    [PSCustomObject] @{
        Test  = 123
        Test2 = "Tes1t"
    }
    [PSCustomObject] @{
        Test  = 456
        Test2 = "Test2"
    }
    [PSCustomObject] @{
        Test  = 789
        Test2 = "Test3"
    }
)
$ObjectsHashes = @(
    [ordered] @{
        Test  = 123
        Test2 = "Tes1t"
    }
    [ordered] @{
        Test  = 456
        Test2 = "Test2"
    }
    [ordered] @{
        Test  = 789
        Test2 = "Test3"
    }
)

$Card = New-AdaptiveCard {
    # So here we should tables and stuff
    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Table usage with PSCustomObject 🔥' -Wrap

    New-AdaptiveTable -DataTable $Objects

    New-AdaptiveLineBreak

    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Table usage with OrderedDictionary 🤷‍♂️' -Wrap

    New-AdaptiveTable -DataTable $ObjectsHashes

    New-AdaptiveLineBreak

    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Table usage with display as PSCustomObject ❤️' -Wrap

    New-AdaptiveTable -DataTable $ObjectsHashes -DictionaryAsCustomObject

    New-AdaptiveTextBlock -Text 'Different example' -Size Large -Subtle -Spacing ExtraLarge

    New-AdaptiveLineBreak

    # and here we mix it with some sample from Adaptive cards
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

} -Uri $Env:TEAMSPESTERID -FullWidth -ReturnJson

$Card | ConvertFrom-Json | ConvertTo-Json -Depth 20