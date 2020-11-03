Import-Module .\PSTeams.psd1 -Force

# Based on: https://adaptivecards.io/samples/WeatherCompact.html

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
    New-AdaptiveTextBlock -Text 'Redmond, WA' -Size Large -Subtle
    New-AdaptiveTextBlock -Text 'Mon, Nov 4, 2019 6:21 PM' -Spacing None

    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png" -Size Small -AlternateText "Mostly cloudy weather"
        } -Width Auto
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Text '46' -Size ExtraLarge -Spacing None
        } -Width Auto
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Text "°F" -Weight Bolder -Spacing Small
        } -Width Stretch
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Text 'Hi 50' -HorizontalAlignment Left
            New-AdaptiveTextBlock -Text 'Lo 46' -HorizontalAlignment Left -Spacing None
        } -Width Stretch
    }
} -Speak "The forecast for Seattle January 20 is mostly clear with a High of 51 degrees and Low of 40 degrees" # -Verbose