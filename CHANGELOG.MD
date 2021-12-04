﻿### PSTeams Release Notes

#### 2.1.0 / 2021.12.04
  - ☑ Added `New-AdaptiveMention` that allows to define mentions for Adaptive Cards. Requires MS to enable notifications. For details: [#17](https://github.com/EvotecIT/PSTeams/issues/17)

#### 2.0.1 / 2021.04.21
  - ☑ Added `FullWidth` switch to `New-AdaptiveCard`. This allows for full width adaptive card.
  - ☑ Added `AllowImageExpand` switch to `New-AdaptiveCard`. This allows for expanding on images
#### 2.0.0 / 2020.11.03
  - [x] Added support for [AdaptiveCard](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card) using `New-AdaptiveCard`
    - [x] Added `New-AdaptiveActionSet`
      - [x] Added `New-AdaptiveAction`
    - [x] Added `New-AdaptiveColumnSet`
      - [x] Added `New-AdaptiveColumn`
    - [x] Added `New-AdaptiveImageSet` -> alias `New-AdaptiveImageGallery`
      - [x] Added `New-AdaptiveImage`
    - [x] Added `New-AdaptiveMedia`
      - [x] Added `New-AdaptiveMediaSource`
    - [x] Added `New-AdaptiveFactSet`
      - [x] Added `New-AdaptiveFact`
    - [x] Added `New-AdaptiveContainer`
    - [x] Added `New-AdaptiveTextBlock`
    - [x] Added `New-AdaptiveRichTextBlock`
  - [x] Added support for [List Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#list-card) using `New-CardList`
    - [x] Added `New-CardListItem`
    - [x] Added `New-CardListButton` (Maximum 6 buttons)
    - [x] Please notice this isn't really supported in Connectors and is added mostly for fun or basic usage as most of the features do not work
  - [x] Added support for [Hero Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#hero-card) using `New-HeroCard`
    - [x] Added `New-HeroImage` -> actually an alias `New-AdaptiveImage`
    - [x] Added `New-HeroButton` -> actually an alias `New-CardListButton`
  - [x] Added support for [Thumbnail Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#thumbnail-card) using `New-ThumbnailCard`
    - [x] Added `New-ThumbnailImage` -> actually an alias `New-AdaptiveImage`
    - [x] Added `New-ThumbnailButton` -> actually an alias `New-CardListButton`
#### 1.0.7 / 2020.10.31
  - [x] Improved `Send-TeamsMessageBody`
  - [x] Removed `ShowErrors` from `Send-TeamsMessage`
    - [x] Replaced it with `ErrorAction Stop`
  - [x] Add support for custom activity image file system path [#24](https://github.com/EvotecIT/PSTeams/pull/24) by R3ality
#### 1.0.5 / 2020.08.10
  - [x] Fix for ActivityImage images not working correctly [#22](https://github.com/EvotecIT/PSTeams/issues/22)
#### 1.0.4 / 2020.07.29
  - [x] Small update to error handling
#### 1.0.3 / 2020.07.29
  - [x] Small update to `Send-TeamsMessage` to potentially display warning if cmdlet fails
  - [x] Renamed `Supress` to `Suppress` in `Send-TeamsMessage` - this will hunt me forever all over my modules (left alias in place)
#### 1.0.2 / 2020.07.28
  - [x] Added missing images `New-TeamsActivityImage`
#### 1.0.1 / 2020.07.28
  - [x] Module is now signed
  - [x] Added `ConvertTo-TeamsFact` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - [x] Added `ConvertTo-TeamsSection` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - [x] Fixed problem with missing image
  - [x] Added missing image choice
#### 1.0.0 / 2019.12.22 - [full blog post](https://evotec.xyz/sending-to-microsoft-teams-from-powershell-just-got-easier-and-better/)
  - [x] New way of sending to Teams, old way still works
  - [x] Added new aliases
  - [x] Reworked lists/list items creation
  - [x] Removed Enums in favour of ArgumentCompleters with strings.
  - [x] Changed 149 colors into 800 (+-) colors (same as [PSWriteHTML](https://github.com/EvotecIT/PSWriteHTML))
#### 0.6.0 / 2019.04.12
  - [x] Stability issues
#### 0.4.0 / 2019.04.03
  - [x] fix for UTF-8 charset - (provided by hjorslev)
  - [x] emoji support added - (provided by hjorslev) - to use it you may need UTF-8 with BOM file encoding
#### 0.3.0 / 2019.02.21
  - [x] added summary for message that is visible in Activity pane
#### 0.2.0 / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
  - [x] added cross-platform (works on linux, mac os, windows)
  - [x] added azure pipelines
  - [x] added some pester tests
#### 0.1.0 / 2018.07.12
  [x] first release