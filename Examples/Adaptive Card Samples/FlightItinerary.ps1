Import-Module .\PSTeams.psd1 -Force

# Based on: https://adaptivecards.io/samples/FlightItinerary.html

New-AdaptiveCard -Uri $Env:TEAMSPESTERID {
    New-AdaptiveTextBlock -Text 'Passengers' -Weight Bolder -Subtle
    New-AdaptiveTextBlock -Text 'Sarah Hum' -Separator
    New-AdaptiveTextBlock -Text 'Jeremy Goldberg' -Spacing None
    New-AdaptiveTextBlock -Text 'Evan Litvak' -Spacing None
    New-AdaptiveTextBlock -Text '2 Stops' -Spacing Medium -Weight Bolder
    New-AdaptiveTextBlock -Text 'Tue, May 30, 2017 12:25 PM' -Spacing None -Weight Bolder

    New-AdaptiveColumnSet {
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'San Francisco' -Subtle
            New-AdaptiveTextBlock -Text 'SFO' -Size ExtraLarge -Color Accent -Spacing None
        }
        New-AdaptiveColumn -Width Auto {
            New-AdaptiveTextBlock -Text ' '
            New-AdaptiveImage -Url 'https://adaptivecards.io/content/airplane.png' -Size Small -Spacing Large -AlternateText 'Flight to'
        }
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'Amsterdam' -Subtle -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text 'AMS' -Size ExtraLarge -Color Accent -Spacing None -HorizontalAlignment Right
        }
    }

    New-AdaptiveTextBlock -Text 'Non-Stop' -Weight Bolder -Spacing Medium
    New-AdaptiveTextBlock -Text 'Fri, Jun 2, 2017 1:55 PM' -Weight Bolder -Spacing None

    New-AdaptiveColumnSet -Separator {
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'Amsterdam' -Subtle
            New-AdaptiveTextBlock -Text 'AMS' -Size ExtraLarge -Color Accent -Spacing None
        }
        New-AdaptiveColumn -Width Auto {
            New-AdaptiveTextBlock -Text ' '
            New-AdaptiveImage -Url 'https://adaptivecards.io/content/airplane.png' -Size Small -Spacing Large -AlternateText 'Flight to'
        }
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'San Francisco' -Subtle -HorizontalAlignment Right
            New-AdaptiveTextBlock -Text 'SFO' -Size ExtraLarge -Color Accent -Spacing None -HorizontalAlignment Right
        }
    }

    New-AdaptiveColumnSet -Spacing Medium {
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text 'Total' -Subtle -Size Medium
        }
        New-AdaptiveColumn -WidthInWeight 1 {
            New-AdaptiveTextBlock -Text '$4,032.54' -Size Medium -Weight Bolder -HorizontalAlignment Right
        }
    }

} -Speak "Your flight is confirmed for you and 3 other passengers from San Francisco to Amsterdam on Friday, October 10 8:30 AM" -Verbose