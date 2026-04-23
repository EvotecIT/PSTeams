Describe 'Legacy adaptive leaf migration cmdlets' {
    BeforeEach {
        Get-Module PSTeams, TeamsX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force
    }

    It 'creates typed adaptive leaf elements from migrated cmdlets' {
        $textBlock = New-AdaptiveTextBlock -Text '' -Color Attention -Wrap -Subtle -HorizontalAlignment Center -Separator -Spacing None -Id 'heading'
        $image = New-AdaptiveImage -Url 'https://example.test/avatar.png' -AlternateText 'Avatar' -Size Small -Style person -HorizontalAlignment Right -HeightInPixels 40 -WidthInPixels 32 -Id 'avatar' -Hidden -BackgroundColor DodgerBlue -SelectActionUrl 'https://example.test/profile' -SelectActionTitle 'Open profile'
        $mention = New-AdaptiveMention -Text 'Ops Team' -UserPrincipalName 'ops@example.test' -Name 'Ops Team'
        $submitAction = New-AdaptiveAction -Title 'Approve' -Type Action.Submit
        $openUrlAction = New-AdaptiveAction -Title 'Open build' -ActionUrl 'https://example.test/build'
        $showCardAction = New-AdaptiveAction -Title 'Details' -Body {
            New-AdaptiveTextBlock -Text 'Nested details'
        } -Actions {
            New-AdaptiveAction -Title 'Nested approve' -Type Action.Submit
        }
        $container = New-AdaptiveContainer -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Style Emphasis -MinimumHeight 120 -Bleed -VerticalContentAlignment center -Id 'panel' -Hidden -BackgroundUrl 'https://example.test/background.png' -BackgroundFillMode Cover -BackgroundHorizontalAlignment left -BackgroundVerticalAlignment top -SelectActionUrl 'https://example.test/panel' -SelectActionTitle 'Open panel' {
            New-AdaptiveTextBlock -Text 'Container body'
        }
        $weightedColumn = New-AdaptiveColumn -WidthInWeight 2 -Spacing Small -Height Stretch -MinimumHeight 90 -HorizontalAlignment Right -VerticalContentAlignment Bottom -Style Attention -Hidden -Separator -SelectAction Action.ToggleVisibility -SelectActionId 'toggle-column' -SelectActionTitle 'Toggle column' -SelectActionTargetElement 'detailsBlock' {
            New-AdaptiveTextBlock -Text 'Weighted column'
        }
        $pixelColumn = New-AdaptiveColumn -WidthInPixels 48 {
            New-AdaptiveImage -Url 'https://example.test/status.png' -AlternateText 'Status'
        }
        $columnSet = New-AdaptiveColumnSet -Style Good -MinimumHeight 80 -Bleed -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch {
            $weightedColumn
            $pixelColumn
        }
        $actionSet = New-AdaptiveActionSet {
            New-AdaptiveAction -Title 'View' -ActionUrl 'https://example.test/view'
            New-AdaptiveAction -Title 'Approve' -Type Action.Submit
        }
        $mediaSource = New-AdaptiveMediaSource -Type 'video/mp4' -Url 'https://example.test/video.mp4'
        $media = New-AdaptiveMedia -PosterUrl 'https://example.test/poster.png' -AlternateText 'Walkthrough' -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'demo' -Hidden {
            New-AdaptiveMediaSource -Type 'video/mp4' -Url 'https://example.test/video.mp4'
        }
        $fact = New-AdaptiveFact -Title 'Status' -Value 'Failed'
        $factSet = New-AdaptiveFactSet -Spacing Medium -Height Stretch -Separator {
            New-AdaptiveFact -Title 'Status' -Value 'Failed'
            New-AdaptiveFact -Title 'Build' -Value '42'
        }
        $richText = New-AdaptiveRichTextBlock -Text 'Build ', 'failed' -Color Default, Attention -Weight Default, Bolder -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'summary' -Hidden
        $lineBreak = New-AdaptiveLineBreak
        $imageSet = New-AdaptiveImageSet -Size Small -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'gallery' -Hidden {
            New-AdaptiveImage -Url 'https://example.test/one.png' -AlternateText 'One'
            New-AdaptiveImage -Url 'https://example.test/two.png' -AlternateText 'Two'
        }

        $textBlock.GetType().Name | Should -Be 'TeamsAdaptiveTextBlock'
        $textBlock.Text | Should -Be "$([char]0x200F)"
        $textBlock.Wrap | Should -BeTrue
        $textBlock.Subtle | Should -BeTrue
        $image.GetType().Name | Should -Be 'TeamsAdaptiveImage'
        $image.BackgroundColor | Should -Be '#1E90FF'
        $image.SelectAction.GetType().Name | Should -Be 'TeamsAdaptiveOpenUrlAction'
        $mention.GetType().Name | Should -Be 'TeamsAdaptiveMention'
        $mention.Text | Should -Be '<at>Ops Team</at>'
        $submitAction.GetType().Name | Should -Be 'TeamsAdaptiveSubmitAction'
        $openUrlAction.GetType().Name | Should -Be 'TeamsAdaptiveOpenUrlAction'
        (New-AdaptiveAction -Title 'Open later' -Type Action.OpenUrl).GetType().Name | Should -Be 'TeamsAdaptiveOpenUrlAction'
        $showCardAction.GetType().Name | Should -Be 'TeamsAdaptiveShowCardAction'
        $showCardAction.Card.type | Should -Be 'AdaptiveCard'
        $container.GetType().Name | Should -Be 'TeamsAdaptiveContainer'
        $container.MinimumHeight | Should -Be '120px'
        $container.Bleed | Should -BeTrue
        $container.IsVisible | Should -BeFalse
        $container.SelectAction.GetType().Name | Should -Be 'TeamsAdaptiveOpenUrlAction'
        $columnSet.GetType().Name | Should -Be 'TeamsAdaptiveColumnSet'
        $columnSet.Columns.Count | Should -Be 2
        $columnSet.Bleed | Should -BeTrue
        $weightedColumn.GetType().Name | Should -Be 'TeamsAdaptiveColumn'
        $weightedColumn.Width | Should -Be '2'
        $weightedColumn.SelectAction.GetType().Name | Should -Be 'TeamsAdaptiveToggleVisibilityAction'
        $pixelColumn.Width | Should -Be '48px'
        $actionSet.GetType().Name | Should -Be 'TeamsAdaptiveActionSet'
        $actionSet.Actions.Count | Should -Be 2
        $mediaSource.GetType().Name | Should -Be 'TeamsAdaptiveMediaSource'
        $mediaSource.MimeType | Should -Be 'video/mp4'
        $media.GetType().Name | Should -Be 'TeamsAdaptiveMedia'
        $media.Separator | Should -BeTrue
        $media.IsVisible | Should -BeFalse
        $media.Sources.Count | Should -Be 1
        $fact.GetType().Name | Should -Be 'TeamsAdaptiveFact'
        $fact.Title | Should -Be 'Status'
        $factSet.GetType().Name | Should -Be 'TeamsAdaptiveFactSet'
        $factSet.Separator | Should -BeTrue
        $factSet.Facts.Count | Should -Be 2
        $richText.GetType().Name | Should -Be 'TeamsAdaptiveRichTextBlock'
        $richText.Separator | Should -BeTrue
        $richText.IsVisible | Should -BeFalse
        $richText.Inlines.Count | Should -Be 2
        $lineBreak.GetType().Name | Should -Be 'TeamsAdaptiveTextBlock'
        $lineBreak.Text | Should -Be "`n"
        $imageSet.GetType().Name | Should -Be 'TeamsAdaptiveImageSet'
        $imageSet.Separator | Should -BeTrue
        $imageSet.IsVisible | Should -BeFalse
        $imageSet.Images.Count | Should -Be 2
    }

    It 'renders New-AdaptiveCard JSON when body contains migrated adaptive cmdlets' {
        $json = New-AdaptiveCard -ReturnJson {
            New-AdaptiveTextBlock -Text '' -Color Attention -Wrap -Subtle -HorizontalAlignment Center -Separator -Spacing None -Id 'heading'
            New-AdaptiveContainer -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Style Emphasis -MinimumHeight 120 -Bleed -VerticalContentAlignment center -Id 'panel' -Hidden -BackgroundUrl 'https://example.test/background.png' -BackgroundFillMode Cover -BackgroundHorizontalAlignment left -BackgroundVerticalAlignment top -SelectActionUrl 'https://example.test/panel' -SelectActionTitle 'Open panel' {
                New-AdaptiveTextBlock -Text 'Container body'
                New-AdaptiveColumnSet -Style Good -MinimumHeight 80 -Bleed -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch {
                    New-AdaptiveColumn -WidthInWeight 2 -Spacing Small -Height Stretch -MinimumHeight 90 -HorizontalAlignment Right -VerticalContentAlignment Bottom -Style Attention -Hidden -Separator -SelectAction Action.ToggleVisibility -SelectActionId 'toggle-column' -SelectActionTitle 'Toggle column' -SelectActionTargetElement 'detailsBlock' {
                        New-AdaptiveTextBlock -Text 'Weighted column'
                    }
                    New-AdaptiveColumn -Width Auto {
                        New-AdaptiveImage -Url 'https://example.test/status.png' -AlternateText 'Status'
                    }
                }
            }
            New-AdaptiveImage -Url 'https://example.test/avatar.png' -AlternateText 'Avatar' -Size Small -Style person -HorizontalAlignment Right -HeightInPixels 40 -WidthInPixels 32 -Id 'avatar' -Hidden -BackgroundColor DodgerBlue -SelectActionUrl 'https://example.test/profile' -SelectActionTitle 'Open profile'
            New-AdaptiveFactSet -Spacing Medium -Height Stretch -Separator {
                New-AdaptiveFact -Title 'Status' -Value 'Failed'
                New-AdaptiveFact -Title 'Build' -Value '42'
            }
            New-AdaptiveRichTextBlock -Text 'Build ', 'failed' -Color Default, Attention -Weight Default, Bolder -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'summary' -Hidden
            New-AdaptiveLineBreak
            New-AdaptiveImageGallery -Size Small -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'gallery' -Hidden {
                New-AdaptiveImage -Url 'https://example.test/one.png' -AlternateText 'One'
                New-AdaptiveImage -Url 'https://example.test/two.png' -AlternateText 'Two'
            }
            New-AdaptiveActionSet {
                New-AdaptiveAction -Title 'View in body' -ActionUrl 'https://example.test/body'
                New-AdaptiveAction -Title 'Submit in body' -Type Action.Submit
            }
            New-AdaptiveMedia -PosterUrl 'https://example.test/poster.png' -AlternateText 'Walkthrough' -Spacing Medium -Separator -HorizontalAlignment Center -Height Stretch -Id 'demo' -Hidden {
                New-AdaptiveMediaSource -Type 'video/mp4' -Url 'https://example.test/video.mp4'
            }
            New-AdaptiveMention -Text 'Ops Team' -UserPrincipalName 'ops@example.test' -Name 'Ops Team'
        } -Action {
            New-AdaptiveAction -Title 'Approve' -Type Action.Submit
            New-AdaptiveAction -Title 'Open build' -ActionUrl 'https://example.test/build'
            New-AdaptiveAction -Title 'Show details' -Body {
                New-AdaptiveTextBlock -Text 'Nested details'
            } -Actions {
                New-AdaptiveAction -Title 'Nested approve' -Type Action.Submit
            }
        }

        $json | Should -Match '"type"\s*:\s*"ActionSet"'
        $json | Should -Match '"type"\s*:\s*"TextBlock"'
        $json | Should -Match '"wrap"\s*:\s*true'
        $json | Should -Match '"isSubtle"\s*:\s*true'
        $json | Should -Match '"type"\s*:\s*"Container"'
        $json | Should -Match '"minHeight"\s*:\s*"120px"'
        $json | Should -Match '"backgroundImage"\s*:\s*\{'
        $json | Should -Match '"type"\s*:\s*"ColumnSet"'
        $json | Should -Match '"type"\s*:\s*"Column"'
        $json | Should -Match '"width"\s*:\s*"2"'
        $json | Should -Match '"targetElements"\s*:\s*\[\s*"detailsBlock"\s*\]'
        $json | Should -Match '"type"\s*:\s*"FactSet"'
        $json | Should -Match '"separator"\s*:\s*true'
        $json | Should -Match '"title"\s*:\s*"Status"'
        $json | Should -Match '"value"\s*:\s*"Failed"'
        $json | Should -Match '"type"\s*:\s*"Image"'
        $json | Should -Match '"backgroundColor"\s*:\s*"#1E90FF"'
        $json | Should -Match '"type"\s*:\s*"RichTextBlock"'
        $json | Should -Match '"id"\s*:\s*"summary"'
        $json | Should -Match '"type"\s*:\s*"ImageSet"'
        $json | Should -Match '"imageSize"\s*:\s*"Small"'
        $json | Should -Match '"type"\s*:\s*"Media"'
        $json | Should -Match '"mimeType"\s*:\s*"video/mp4"'
        $json | Should -Match '"type"\s*:\s*"Action.Submit"'
        $json | Should -Match '"type"\s*:\s*"Action.ShowCard"'
        $json | Should -Match '"card"\s*:\s*\{'
        $json | Should -Match '"type"\s*:\s*"Action.OpenUrl"'
        $json | Should -Match '"type"\s*:\s*"mention"'
        $json | Should -Match '"entities"\s*:\s*\['
    }

    It 'returns adaptive card JSON when Uri and ReturnJson are used together' {
        $json = New-AdaptiveCard -Uri 'https://example.test/webhook' -ReturnJson -WhatIf -FallBackText 'Fallback text' -MinimumHeight 140 -Speak 'Build failed' -Language 'en' -VerticalContentAlignment center -BackgroundUrl 'https://example.test/background.png' -BackgroundFillMode Cover -BackgroundHorizontalAlignment left -BackgroundVerticalAlignment top -SelectActionUrl 'https://example.test/card' -SelectActionTitle 'Open card' -AllowImageExpand -FullWidth {
            New-AdaptiveTextBlock -Text 'Build failed'
            New-AdaptiveMention -Text 'Ops Team' -UserPrincipalName 'ops@example.test' -Name 'Ops Team'
        } -Action {
            New-AdaptiveAction -Title 'Open build' -ActionUrl 'https://example.test/build'
        }

        $json | Should -Match '"type"\s*:\s*"message"'
        $json | Should -Match '"fallbackText"\s*:\s*"Fallback text"'
        $json | Should -Match '"minHeight"\s*:\s*"140px"'
        $json | Should -Match '"speak"\s*:\s*"Build failed"'
        $json | Should -Match '"lang"\s*:\s*"en"'
        $json | Should -Match '"allowExpand"\s*:\s*true'
        $json | Should -Match '"width"\s*:\s*"Full"'
        $json | Should -Match '"selectAction"\s*:\s*\{'
        $json | Should -Match '"url"\s*:\s*"https://example.test/card"'
        $json | Should -Match '"entities"\s*:\s*\['
    }

    It 'creates adaptive table rows from objects and dictionaries' {
        $objectTable = @(New-AdaptiveTable -DataTable @(
            [pscustomobject]@{ Name = 'Server01'; Status = 'Failed' }
            [pscustomobject]@{ Name = 'Server02'; Status = 'Passed' }
        ))
        $dictionaryTable = @(New-AdaptiveTable -DataTable @(
            [ordered]@{ Name = 'Server01'; Status = 'Failed' }
            [ordered]@{ Name = 'Server02'; Status = 'Passed' }
        ) -DictionaryAsCustomObject)

        $objectTable.Count | Should -Be 3
        $objectTable[0].GetType().Name | Should -Be 'TeamsAdaptiveColumnSet'
        $objectTable[0].Columns.Count | Should -Be 2
        $objectTable[1].Columns[0].Items[0].Text | Should -Be 'Server01'
        $objectTable[1].Columns[1].Items[0].Text | Should -Be 'Failed'
        $dictionaryTable.Count | Should -Be 3
        $dictionaryTable[0].Columns[0].Items[0].Text | Should -Be 'Name'
        $dictionaryTable[1].Columns[1].Items[0].Text | Should -Be 'Failed'
    }
}
