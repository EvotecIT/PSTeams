### PSTeams Release Notes

#### 2.4.0
  - Fixes `New-AdaptiveTable` when using null values or empty strings [#55](https://github.com/EvotecIT/PSTeams/issues/55)
  - Fixes `New-AdaptiveTable` when using longer data [#50](https://github.com/EvotecIT/PSTeams/issues/50)
  - Add couple of options for `New-AdaptiveTable` (disable separators)

#### 2.3.0 / 2023.06.18
  - Improves `New-AdaptiveTable` to support `Width` (Auto/Stretch), default being 'Stretch' [#51](https://github.com/EvotecIT/PSTeams/issues/51)

#### 2.2.1 / 2022.08.21
  - Improves `New-AdaptiveTable` with more parameters to better support user needs.
  - Improves `New-AdaptiveTextBlock` with more parameters to better support user needs.
  - Improves `New-AdaptiveTable` when it comes to Ordered Dictionary support.
  - Added `New-AdaptiveLineBreak` to create a line break easily.

#### 2.2.0 / 2022.08.19
  - Added support for `New-AdaptiveTable` that simplifies the creation of tables from arrays.

#### 2.1.2 / 2022.07.13
  - ☑ Added support for `Proxy` parameter in `Send-TeamsMessage` and `Send-TeamsMessageBody`, for environments where connections to Office 365 go via a proxy.

#### 2.1.1 / 2021.12.05
  - 🐛 Fixes broken `FullWidth` and `AllowImageExpand` in last version of PSTeams for `New-AdaptiveCard`
  - 📃 Updated docs for `New-AdaptiveCard`

#### 2.1.0 / 2021.12.04
  - ☑ Added `New-AdaptiveMention` that allows to define mentions for Adaptive Cards. Requires MS to enable notifications. For details: [#17](https://github.com/EvotecIT/PSTeams/issues/17)

#### 2.0.1 / 2021.04.21
  - ☑ Added `FullWidth` switch to `New-AdaptiveCard`. This allows for full width adaptive card.
  - ☑ Added `AllowImageExpand` switch to `New-AdaptiveCard`. This allows for expanding on images
#### 2.0.0 / 2020.11.03
  - ☑ Added support for [AdaptiveCard](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card) using `New-AdaptiveCard`
    - ☑ Added `New-AdaptiveActionSet`
      - ☑ Added `New-AdaptiveAction`
    - ☑ Added `New-AdaptiveColumnSet`
      - ☑ Added `New-AdaptiveColumn`
    - ☑ Added `New-AdaptiveImageSet` -> alias `New-AdaptiveImageGallery`
      - ☑ Added `New-AdaptiveImage`
    - ☑ Added `New-AdaptiveMedia`
      - ☑ Added `New-AdaptiveMediaSource`
    - ☑ Added `New-AdaptiveFactSet`
      - ☑ Added `New-AdaptiveFact`
    - ☑ Added `New-AdaptiveContainer`
    - ☑ Added `New-AdaptiveTextBlock`
    - ☑ Added `New-AdaptiveRichTextBlock`
  - ☑ Added support for [List Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#list-card) using `New-CardList`
    - ☑ Added `New-CardListItem`
    - ☑ Added `New-CardListButton` (Maximum 6 buttons)
    - ☑ Please notice this isn't really supported in Connectors and is added mostly for fun or basic usage as most of the features do not work
  - ☑ Added support for [Hero Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#hero-card) using `New-HeroCard`
    - ☑ Added `New-HeroImage` -> actually an alias `New-AdaptiveImage`
    - ☑ Added `New-HeroButton` -> actually an alias `New-CardListButton`
  - ☑ Added support for [Thumbnail Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#thumbnail-card) using `New-ThumbnailCard`
    - ☑ Added `New-ThumbnailImage` -> actually an alias `New-AdaptiveImage`
    - ☑ Added `New-ThumbnailButton` -> actually an alias `New-CardListButton`
#### 1.0.7 / 2020.10.31
  - ☑ Improved `Send-TeamsMessageBody`
  - ☑ Removed `ShowErrors` from `Send-TeamsMessage`
    - ☑ Replaced it with `ErrorAction Stop`
  - ☑ Add support for custom activity image file system path [#24](https://github.com/EvotecIT/PSTeams/pull/24) by R3ality
#### 1.0.5 / 2020.08.10
  - ☑ Fix for ActivityImage images not working correctly [#22](https://github.com/EvotecIT/PSTeams/issues/22)
#### 1.0.4 / 2020.07.29
  - ☑ Small update to error handling
#### 1.0.3 / 2020.07.29
  - ☑ Small update to `Send-TeamsMessage` to potentially display warning if cmdlet fails
  - ☑ Renamed `Supress` to `Suppress` in `Send-TeamsMessage` - this will hunt me forever all over my modules (left alias in place)
#### 1.0.2 / 2020.07.28
  - ☑ Added missing images `New-TeamsActivityImage`
#### 1.0.1 / 2020.07.28
  - ☑ Module is now signed
  - ☑ Added `ConvertTo-TeamsFact` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - ☑ Added `ConvertTo-TeamsSection` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - ☑ Fixed problem with missing image
  - ☑ Added missing image choice
#### 1.0.0 / 2019.12.22 - [full blog post](https://evotec.xyz/sending-to-microsoft-teams-from-powershell-just-got-easier-and-better/)
  - ☑ New way of sending to Teams, old way still works
  - ☑ Added new aliases
  - ☑ Reworked lists/list items creation
  - ☑ Removed Enums in favour of ArgumentCompleters with strings.
  - ☑ Changed 149 colors into 800 (+-) colors (same as [PSWriteHTML](https://github.com/EvotecIT/PSWriteHTML))
#### 0.6.0 / 2019.04.12
  - ☑ Stability issues
#### 0.4.0 / 2019.04.03
  - ☑ fix for UTF-8 charset - (provided by hjorslev)
  - ☑ emoji support added - (provided by hjorslev) - to use it you may need UTF-8 with BOM file encoding
#### 0.3.0 / 2019.02.21
  - ☑ added summary for message that is visible in Activity pane
#### 0.2.0 / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
  - ☑ added cross-platform (works on linux, mac os, windows)
  - ☑ added azure pipelines
  - ☑ added some pester tests
#### 0.1.0 / 2018.07.12
  ☑ first release