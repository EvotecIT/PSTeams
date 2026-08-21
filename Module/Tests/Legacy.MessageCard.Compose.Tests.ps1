Describe 'Legacy connector-card migration cmdlets' {
    BeforeEach {
        Get-Module PSTeams, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force
    }

    It 'creates typed connector-card building blocks from migrated cmdlets' {
        $fact = New-TeamsFact -Name 'Status' -Value 'Failed'
        $button = New-TeamsButton -Name 'Open build' -Link 'https://example.test/build/42'
        $section = New-TeamsSection -Title 'Build summary' -ActivityText 'Pipeline failed' -ActivityDetails $fact -Buttons $button

        $fact.GetType().Name | Should -Be 'TeamsMessageFact'
        $button.GetType().Name | Should -Be 'TeamsMessageButton'
        $section.GetType().Name | Should -Be 'TeamsMessageSection'
        $section.Facts.Count | Should -Be 1
        $section.Buttons.Count | Should -Be 1
    }

    It 'supports section helper cmdlets inside the legacy composition scriptblock' {
        $section = New-TeamsSection {
            New-TeamsActivityTitle -Title 'Build title'
            New-TeamsActivitySubtitle -Subtitle 'Build subtitle'
            New-TeamsActivityText -Text 'Build text'
            New-TeamsActivityImage -Link 'https://example.test/activity.png'
            New-TeamsImage -Link 'https://example.test/image.png'
            New-TeamsBigImage -Link 'https://example.test/hero.png' -AlternativeText 'Hero'
            New-TeamsFact -Name 'Status' -Value 'Failed'
        }

        $section.ActivityTitle | Should -Be 'Build title'
        $section.ActivitySubtitle | Should -Be 'Build subtitle'
        $section.ActivityText | Should -Be 'Build text'
        $section.ActivityImage | Should -Be 'https://example.test/activity.png'
        $section.Images.Count | Should -Be 1
        $section.HeroImages.Count | Should -Be 1
        $section.Facts.Count | Should -Be 1
    }

    It 'preserves action-card link targets and date-input subtypes from legacy dictionaries' {
        $section = New-TeamsSection {
            [ordered]@{
                type    = 'button'
                name    = 'Add comment'
                '@type' = 'ActionCard'
                Inputs  = @(
                    [ordered]@{
                        '@type' = 'TextInput'
                    }
                )
                actions = @(
                    [ordered]@{
                        '@type' = 'HttpPOST'
                        target  = 'https://example.test/comment'
                    }
                )
            }
            [ordered]@{
                type    = 'button'
                name    = 'Choose date'
                '@type' = 'ActionCard'
                Inputs  = @(
                    [ordered]@{
                        '@type' = 'DateInput'
                    }
                )
                actions = @(
                    [ordered]@{
                        '@type' = 'HttpPOST'
                        target  = 'https://example.test/date'
                    }
                )
            }
        }

        $section.Buttons.Count | Should -Be 2
        $section.Buttons[0].ButtonType.ToString() | Should -Be 'TextInput'
        $section.Buttons[0].Link | Should -Be 'https://example.test/comment'
        $section.Buttons[1].ButtonType.ToString() | Should -Be 'DateInput'
        $section.Buttons[1].Link | Should -Be 'https://example.test/date'
    }

    It 'creates legacy list facts from migrated cmdlets' {
        $fact = New-TeamsList -Name 'Checklist' {
            New-TeamsListItem -Text 'Top level' -Level 0
            New-TeamsListItem -Text 'Nested ordered' -Level 1 -Numbered
        }

        $fact.GetType().Name | Should -Be 'TeamsMessageFact'
        $fact.Name | Should -Be 'Checklist'
        $fact.Value | Should -Be "- Top level`r`t1. Nested ordered"
    }

    It 'converts objects into facts and sections using migrated cmdlets' {
        $facts = [pscustomobject]@{
            BuildStatus = 'Failed'
            BuildId     = 42
        } | ConvertTo-TeamsFact

        $sections = @(
            [pscustomobject]@{
                Name   = 'Pipeline'
                Status = 'Failed'
            }
        ) | ConvertTo-TeamsSection -SectionTitleProperty Name

        $facts.Count | Should -Be 2
        $facts[0].GetType().Name | Should -Be 'TeamsMessageFact'
        $facts.Name | Should -Contain 'BuildStatus'
        $sections.Count | Should -Be 1
        $sections[0].GetType().Name | Should -Be 'TeamsMessageSection'
        $sections[0].ActivityTitle | Should -Be 'Name Pipeline'
        $sections[0].Facts.Count | Should -Be 2
    }

    It 'renders legacy Send-TeamsMessage payloads without sending when using WhatIf' {
        $body = Send-TeamsMessage -Uri 'https://example.test/webhook' -MessageTitle 'Build failed' -MessageText 'Pipeline 42' -Color DodgerBlue -Sections @(
            New-TeamsSection -Title 'Section' -ActivityDetails @(
                New-TeamsFact -Name 'Status' -Value 'Failed'
            ) -Buttons @(
                New-TeamsButton -Name 'Open build' -Link 'https://example.test/build/42' -Type OpenUri
            )
        ) -Suppress:$false -WhatIf

        $body | Should -Match '"themeColor":"#1E90FF"'
        $body | Should -Match '"title":"Build failed"'
        $body | Should -Match '"name":"Status"'
        $body | Should -Match '"@type":"OpenURI"'
    }

    It 'renders helper-based section content in the legacy Send-TeamsMessage scriptblock path' {
        $body = Send-TeamsMessage -Uri 'https://example.test/webhook' -MessageTitle 'Build failed' -Suppress:$false -WhatIf {
            New-TeamsSection {
                New-TeamsActivityTitle -Title 'Build title'
                New-TeamsActivitySubtitle -Subtitle 'Build subtitle'
                New-TeamsActivityText -Text 'Build text'
                New-TeamsActivityImage -Link 'https://example.test/activity.png'
                New-TeamsImage -Link 'https://example.test/image.png'
                New-TeamsBigImage -Link 'https://example.test/hero.png' -AlternativeText 'Hero'
            }
        }

        $body | Should -Match '"activityTitle":"Build title"'
        $body | Should -Match '"activitySubtitle":"Build subtitle"'
        $body | Should -Match '"activityText":"Build text"'
        $body | Should -Match '"activityImage":"https://example.test/activity.png"'
        $body | Should -Match '"images":\['
        $body | Should -Match '!\[Hero\]\(https://example.test/hero.png\)'
    }

    It 'preserves raw dictionary sections in the legacy Send-TeamsMessage scriptblock path' {
        $body = Send-TeamsMessage -Uri 'https://example.test/webhook' -MessageTitle 'Build failed' -Suppress:$false -WhatIf {
            [ordered]@{
                title           = 'Build summary'
                text            = 'Pipeline failed'
                startGroup      = $true
                facts           = @(
                    [ordered]@{
                        name  = 'Status'
                        value = 'Failed'
                    }
                )
                images          = @(
                    [ordered]@{
                        image = 'https://example.test/image.png'
                    }
                )
                potentialAction = @(
                    [ordered]@{
                        name    = 'Open build'
                        '@type' = 'OpenURI'
                        Targets = @(
                            [ordered]@{
                                os  = 'default'
                                uri = 'https://example.test/build/42'
                            }
                        )
                    }
                )
            }
        }

        $body | Should -Match '"title":"Build summary"'
        $body | Should -Match '"text":"Pipeline failed"'
        $body | Should -Match '"startGroup":true'
        $body | Should -Match '"name":"Status"'
        $body | Should -Match '"image":"https://example.test/image.png"'
        $body | Should -Match '"@type":"OpenURI"'
    }

    It 'wraps raw attachment bodies without sending when using WhatIf' {
        $body = Send-TeamsMessageBody -Uri 'https://example.test/webhook' -Body '{"contentType":"application/vnd.microsoft.card.hero"}' -Wrap -Supress:$false -WhatIf

        $body | Should -Match '"type":"message"'
        $body | Should -Match '"attachments":\['
        $body | Should -Match '"contentType":"application/vnd.microsoft.card.hero"'
    }

    It 'does not expose message bodies in verbose output' {
        $messageMarker = 'MESSAGE_BODY_MUST_NOT_APPEAR'
        $rawMarker = 'RAW_BODY_MUST_NOT_APPEAR'

        $messageVerbose = & {
            Send-TeamsMessage -Uri 'https://example.test/webhook' -MessageText $messageMarker -WhatIf -Verbose
        } 4>&1 | Out-String
        $rawVerbose = & {
            Send-TeamsMessageBody -Uri 'https://example.test/webhook' -Body "{`"text`":`"$rawMarker`"}" -WhatIf -Verbose
        } 4>&1 | Out-String

        $messageVerbose | Should -Match 'Prepared \d+ characters for example.test'
        $messageVerbose | Should -Not -Match $messageMarker
        $rawVerbose | Should -Match 'Prepared \d+ characters for example.test'
        $rawVerbose | Should -Not -Match $rawMarker
    }
}
