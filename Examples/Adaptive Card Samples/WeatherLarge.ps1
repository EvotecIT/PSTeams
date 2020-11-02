Import-Module .\PSTeams.psd1 -Force

# Based on: https://adaptivecards.io/samples/WeatherLarge.html

New-AdaptiveCard -Uri $Env:TEAMSPESTERID -BackgroundUrl 'https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Background.jpg' {
    New-AdaptiveColumnSet {
        New-AdaptiveColumn -WidthInWeight 35 {
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png" -Size Stretch -AlternateText "Mostly cloudy weather"
        }
        New-AdaptiveColumn -WidthInWeight 65 {
            New-AdaptiveTextBlock -Text 'Tue, Nov 5, 2019' -Weight Bolder -Size Large
            New-AdaptiveTextBlock -Text '32 / 50' -Size Medium -Spacing None
            New-AdaptiveTextBlock -Text '31% chance of rain' -Spacing None
            New-AdaptiveTextBlock -Text 'Winds 4.4 mph SSE' -Spacing None
        }
    }
    New-AdaptiveColumnSet {
        New-AdaptiveColumn -WidthInWeight 20 {
            New-AdaptiveTextBlock -Text 'Wednesday' -HorizontalAlignment Center
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Drizzle-Square.png" -Size Auto -AlternateText "Drizzly weather"
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'High' -Value '50'
                New-AdaptiveFact -Title 'Low' -Value '32'
            }
        } -SelectActionUrl 'https://www.evotec.xyz' -SelectActionTitle 'View Wednesday'
        New-AdaptiveColumn -WidthInWeight 20 {
            New-AdaptiveTextBlock -Text 'Thursday' -HorizontalAlignment Center
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png" -Size Auto -AlternateText "Mostly cloudy weather"
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'High' -Value '50'
                New-AdaptiveFact -Title 'Low' -Value '32'
            }
        } -SelectActionUrl 'https://www.evotec.xyz' -SelectActionTitle 'View Thursday'
        New-AdaptiveColumn -WidthInWeight 20 {
            New-AdaptiveTextBlock -Text 'Friday' -HorizontalAlignment Center
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png" -Size Auto -AlternateText "Mostly cloudy weather"
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'High' -Value '59'
                New-AdaptiveFact -Title 'Low' -Value '32'
            }
        } -SelectActionUrl 'https://www.evotec.xyz' -SelectActionTitle 'View Friday'
        New-AdaptiveColumn -WidthInWeight 20 {
            New-AdaptiveTextBlock -Text 'Saturday' -HorizontalAlignment Center
            New-AdaptiveImage -Url "https://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png" -Size Auto -AlternateText "Mostly cloudy weather"
            New-AdaptiveFactSet {
                New-AdaptiveFact -Title 'High' -Value '50'
                New-AdaptiveFact -Title 'Low' -Value '32'
            }
        } -SelectActionUrl 'https://www.evotec.xyz' -SelectActionTitle 'View Saturday'
    }
} -Speak "Weather forecast for Monday is high of 62 and low of 42 degrees with a 20% chance of rainWinds will be 5 mph from the northeast" -Verbose