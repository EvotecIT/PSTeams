Describe 'Wrapper-card migration cmdlets' {
    BeforeEach {
        Get-Module PSTeams, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force
    }

    It 'renders HeroCard payloads from migrated cmdlets' {
        $body = New-HeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text 'Monorail text' {
            New-HeroImage -Url 'https://example.test/monorail.jpg'
            New-HeroButton -Type OpenUrl -Title 'Official website' -Value 'https://example.test'
        }

        $body | Should -Match '"contentType":"application/vnd.microsoft.card.hero"'
        $body | Should -Match '"title":"Seattle Center Monorail"'
        $body | Should -Match '"subTitle":"Seattle Center Monorail"'
        $body | Should -Match '"url":"https://example.test/monorail.jpg"'
        $body | Should -Match '"type":"openUrl"'
    }

    It 'accepts legacy adaptive-image aliases inside wrapper cards' {
        $body = New-ThumbnailCard -Title 'Bender' -SubTitle 'robot' -Text 'Futurama' {
            New-ThumbnailImage -Url 'https://example.test/bender.png' -AltText 'Bender'
            New-ThumbnailButton -Type ImBack -Title 'Thumbs Up' -Value 'I like it'
        }

        $body | Should -Match '"contentType":"application/vnd.microsoft.card.thumbnail"'
        $body | Should -Match '"url":"https://example.test/bender.png"'
        $body | Should -Match '"alt":"Bender"'
    }

    It 'supports HeroCard sending in WhatIf mode' {
        $result = New-HeroCard -Title 'Seattle Center Monorail' -Uri 'https://example.test/webhook' -WhatIf {
            New-HeroImage -Url 'https://example.test/monorail.jpg'
            New-HeroButton -Type OpenUrl -Title 'Official website' -Value 'https://example.test'
        }

        $result | Should -BeNullOrEmpty
    }

    It 'renders ThumbnailCard payloads from migrated cmdlets' {
        $body = New-ThumbnailCard -Title 'Bender' -SubTitle 'robot' -Text 'Futurama' {
            New-ThumbnailImage -Url 'https://example.test/bender.png' -AltText 'Bender'
            New-ThumbnailButton -Type ImBack -Title 'Thumbs Up' -Value 'I like it'
        }

        $body | Should -Match '"contentType":"application/vnd.microsoft.card.thumbnail"'
        $body | Should -Match '"title":"Bender"'
        $body | Should -Match '"alt":"Bender"'
        $body | Should -Match '"type":"imBack"'
        $body | Should -Match '"value":"I like it"'
    }

    It 'supports ThumbnailCard sending in WhatIf mode' {
        $result = New-ThumbnailCard -Title 'Bender' -Uri 'https://example.test/webhook' -WhatIf {
            New-ThumbnailImage -Url 'https://example.test/bender.png' -AltText 'Bender'
            New-ThumbnailButton -Type ImBack -Title 'Thumbs Up' -Value 'I like it'
        }

        $result | Should -BeNullOrEmpty
    }

    It 'renders ListCard payloads from migrated cmdlets' {
        $body = New-CardList -Title 'Card Title' {
            New-CardListItem -Type File -Title 'Report' -SubTitle 'teams > new > design' -TapType OpenUrl -TapValue 'https://contoso.example/report.xlsx' -TapAction editOnline
            New-CardListItem -Type Person -Title 'John Doe' -SubTitle 'Manager' -TapType ImBack -TapValue 'JohnDoe@contoso.com' -TapAction whois
            New-CardListButton -Type OpenUrl -Title 'Show' -Value 'https://evotec.xyz'
        }

        $body | Should -Match '"contentType":"application/vnd.microsoft.teams.card.list"'
        $body | Should -Match '"type":"file"'
        $body | Should -Match '"value":"editOnline https://contoso.example/report.xlsx"'
        $body | Should -Match '"type":"person"'
        $body | Should -Match '"value":"whois JohnDoe@contoso.com"'
        $body | Should -Match '"title":"Show"'
    }

    It 'prefers legacy list-item dictionaries over generic button fallback' {
        $body = New-CardList -Title 'Card Title' {
            [ordered]@{
                type     = 'file'
                title    = 'Report'
                subtitle = 'teams > new > design'
                tap      = [ordered]@{
                    type  = 'openUrl'
                    value = 'editOnline https://contoso.example/report.xlsx'
                }
            }
        }

        $body | Should -Match '"items":\['
        $body | Should -Match '"type":"file"'
        $body | Should -Match '"value":"editOnline https://contoso.example/report.xlsx"'
        $body | Should -Not -Match '"buttons":\[\{"type":"file"'
    }

    It 'supports ListCard sending in WhatIf mode' {
        $result = New-CardList -Title 'Card Title' -Uri 'https://example.test/webhook' -WhatIf {
            New-CardListItem -Type File -Title 'Report' -SubTitle 'teams > new > design' -TapType OpenUrl -TapValue 'https://contoso.example/report.xlsx' -TapAction editOnline
            New-CardListButton -Type OpenUrl -Title 'Show' -Value 'https://evotec.xyz'
        }

        $result | Should -BeNullOrEmpty
    }
}
