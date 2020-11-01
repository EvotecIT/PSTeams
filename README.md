<p align="center">
  <a href="https://dev.azure.com/evotecpl/PSTeams/_build/latest?definitionId=3"><img src="https://dev.azure.com/evotecpl/PSTeams/_apis/build/status/EvotecIT.PSTeams"></a>
  <a href="https://www.powershellgallery.com/packages/PSTeams"><img src="https://img.shields.io/powershellgallery/v/PSTeams.svg"></a>
  <a href="https://www.powershellgallery.com/packages/PSTeams"><img src="https://img.shields.io/powershellgallery/vpre/PSTeams.svg?label=powershell%20gallery%20preview&colorB=yellow"></a>
  <a href="https://github.com/EvotecIT/PSTeams"><img src="https://img.shields.io/github/license/EvotecIT/PSTeams.svg"></a>
</p>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/PSTeams"><img src="https://img.shields.io/powershellgallery/p/PSTeams.svg"></a>
  <a href="https://github.com/EvotecIT/PSTeams"><img src="https://img.shields.io/github/languages/top/evotecit/PSTeams.svg"></a>
  <a href="https://github.com/EvotecIT/PSTeams"><img src="https://img.shields.io/github/languages/code-size/evotecit/PSTeams.svg"></a>
  <a href="https://www.powershellgallery.com/packages/PSTeams"><img src="https://img.shields.io/powershellgallery/dt/PSTeams.svg"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=social"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn"></a>
</p>

# PSTeams - PowerShell Module

[PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) is a **PowerShell Module** working on **Windows** / **Linux** and **Mac**. It allows sending notifications to _Microsoft Teams_ via WebHook Notifications. It's pretty flexible and provides a bunch of options.

## Readme Links

While I didn't spent much time creating WIKI, working on `Get-Help` documentation, I did write 3 articles that should help you get started.

- [x] [PSTeams – PowerShell Module](https://evotec.xyz/hub/scripts/psteams-powershell-module/)
- [x] [PSTeams – Send notifications to MS Teams from Mac / Linux or Windows](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
- [x] [Sending Messages to Microsoft Teams from PowerShell just got easier and better](https://evotec.xyz/sending-to-microsoft-teams-from-powershell-just-got-easier-and-better/)

## Supported Cards

While `WebHook Notifications` in theory only support `Office 365 Connector Card` it's possible to do more than that.

- Supported in 0.X.0 - 1.0.X
  - [x] [Office 365 Connector Card](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#office-365-connector-card) - The Office 365 Connector card provides a flexible layout with multiple sections, fields, images, and actions. This card encapsulates a connector card so that it can be used by bots. See the notes section for differences between connector cards and the O365 card.
- Supported in 2.X.X (PreRelease)
  - [x] [AdaptiveCard](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card)
  - [x] [List Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#list-card)
  - [x] [Hero Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#hero-card)

Below you can find how to send them and what they display. You should be aware that while I've added some features, not all of them will work in Teams.

### List Cards

Here's a simple way to send List Cards to Teams using WebHook

- List Items do not support `tapType` `imBack`. When clicked action is not taken
- List Items do not support `tapAction`. It's there, but doesn't work.
- List Items do not support type `file`. It displays, but no action is taken. It's better to use `resultItem`

Code

```powershell
New-CardList {
    New-CardListItem -Type file -Title 'Report' -SubTitle 'teams > new > design' -TapType openUrl -TapValue "https://contoso.sharepoint.com/teams/new/Shared%20Documents/Report.xlsx" -TapAction editOnline
    New-CardListItem -Type resultItem -Title 'Report' -SubTitle 'teams > new > design' -TapType openUrl -TapValue "https://contoso.sharepoint.com/teams/new/Shared%20Documents/Report.xlsx"
    New-CardListItem -Type resultItem -Title 'Trello title' -SubTitle 'A Trello subtitle' -TapType openUrl -TapValue "http://trello.com" -Icon "https://cdn2.iconfinder.com/data/icons/social-icons-33/128/Trello-128.png"
    New-CardListItem -Type section -Title 'Manager'
    New-CardListItem -Type person -Title "John Doe" -SubTitle 'Manager' -TapType imBack -TapValue "JohnDoe@contoso.com" -TapAction whois
    New-CardListButton -Type openUrl -Title 'Show' -Value 'https://evotec.xyz'
} -Uri $Env:TEAMSPESTERID -Title 'Card Title'
```

Output

![List Card](Docs/Images/CardList.png)

### Hero Cards

Here's a simple way to send List Cards to Teams using WebHook

- Hero Buttons (`New-HeroButton`)do not support button type other then `openUrl`
  - When using Type `imBack` action is not taken
  - When using Type `file` button is not displayed
- Using more than 3 buttons causes carousel for card. I've blocked it out, as all that happens is text is doubled/image is doubled but buttons don't show up over 3

Code

```powershell
New-HeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text "The Seattle Center Monorail is an elevated train line between Seattle Center (near the Space Needle) and downtown Seattle. It was built for the 1962 World's Fair. Its original two trains, completed in 1961, are still in service." {
    New-HeroImage -Url 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg'
    New-HeroButton -Type openUrl -Title 'Official website' -Value 'https://www.seattlemonorail.com'
    New-HeroButton -Type openUrl -Title 'Wikipeda page' -Value 'https://www.seattlemonorail.com'
    New-HeroButton -Type openUrl -Title 'Evotec page' -Value 'https://www.evotec.xyz'
} -Uri $Env:TEAMSPESTERID
```

Output

![Hero Card](Docs/Images/HeroCard.png)

### Thumbnail Cards

Here's a simple way to send Thumbnail Cards to Teams using WebHook

- Images are not supported in buttons, you can send them but it's not displayed
- imBack action is not supported in buttons, you can send them but once you click it an notification message appears

Code

```powershell
New-ThumbnailCard -Title 'Bender' -SubTitle "tale of a robot who dared to love" -Text "Bender Bending Rodríguez is a main character in the animated television series Futurama. He was created by series creators Matt Groening and David X. Cohen, and is voiced by John DiMaggio" {
    New-ThumbnailImage -Url 'https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png' -AltText "Bender Rodríguez"
    New-ThumbnailButton -Type imBack -Title 'Thumbs Up' -Value 'I like it' -Image "http://moopz.com/assets_c/2012/06/emoji-thumbs-up-150-thumb-autox125-140616.jpg"
    New-ThumbnailButton -Type openUrl -Title 'Thumbs Down' -Value 'https://evotec.xyz'
    New-ThumbnailButton -Type openUrl -Title 'I feel luck' -Value 'https://www.bing.com/images/search?q=bender&qpvt=bender&qpvt=bender&qpvt=bender&FORM=IGRE'
} -Uri $Env:TEAMSPESTERID
```

Output

![Thumbnail Card](Docs/Images/ThumbnailCard.png)

## Updates

- 2.0.0 Alpha1 / PreRelease / Testing
  - [x] Added initial support for [AdaptiveCard](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card) using `New-AdaptiveCard`
    - [x] Added `New-AdaptiveColumn`
    - [x] Added `New-AdaptiveImage`
    - [x] Added `New-AdaptiveFactSet`
      - [x] Added `New-AdaptiveFact`
    - [x] Added `New-AdaptiveRichTextBlock`
    - [x] Added `New-AdaptiveSection`
    - [x] Added `New-AdaptiveTextBlock`
  - [x] Added support for [List Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#list-card) using `New-CardList`
    - [x] Added `New-CardListItem`
    - [x] Added `New-CardListButton` (Maximum 6 buttons)
    - [x] Please notice this isn't really supported in Connectors and is added mostly for fun or basic usage as most of the features do not work
  - [x] Added support for [Hero Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#hero-card) using `New-HeroCard`
    - [x] Added `New-HeroImage` -> actually an alias `New-AdaptiveImage`
    - [x] Added `New-HeroButton` -> actually an alias `New-CardListButton`
- 1.0.6 / Unreleased
  - [x] Improved `Send-TeamsMessageBody`
  - [x] Removed `ShowErrors` from `Send-TeamsMessage`
    - [x] Replaced it with `ErrorAction Stop`
  - [x] Add support for custom activity image file system path [#24](https://github.com/EvotecIT/PSTeams/pull/24) by R3ality
- 1.0.5 / 2020.08.10
  - [x] Fix for ActivityImage images not working correctly [#22](https://github.com/EvotecIT/PSTeams/issues/22)
- 1.0.4 / 2020.07.29
  - [x] Small update to error handling
- 1.0.3 / 2020.07.29
  - [x] Small update to `Send-TeamsMessage` to potentially display warning if cmdlet fails
  - [x] Renamed `Supress` to `Suppress` in `Send-TeamsMessage` - this will hunt me forever all over my modules (left alias in place)
- 1.0.2 / 2020.07.28
  - [x] Added missing images `New-TeamsActivityImage`
- 1.0.1 / 2020.07.28
  - [x] Module is now signed
  - [x] Added `ConvertTo-TeamsFact` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - [x] Added `ConvertTo-TeamsSection` - tnx theramiyer [#15](https://github.com/EvotecIT/PSTeams/pull/15)
  - [x] Fixed problem with missing image
  - [x] Added missing image choice
- 1.0.0 / 2019.12.22 - [full blog post](https://evotec.xyz/sending-to-microsoft-teams-from-powershell-just-got-easier-and-better/)
  - [x] New way of sending to Teams, old way still works
  - [x] Added new aliases
  - [x] Reworked lists/list items creation
  - [x] Removed Enums in favour of ArgumentCompleters with strings.
  - [x] Changed 149 colors into 800 (+-) colors (same as [PSWriteHTML](https://github.com/EvotecIT/PSWriteHTML))
- 0.6.0 / 2019.04.12
  - [x] Stability issues
- 0.4.0 / 2019.04.03
  - [x] fix for UTF-8 charset - (provided by hjorslev)
  - [x] emoji support added - (provided by hjorslev) - to use it you may need UTF-8 with BOM file encoding
- 0.3.x / 2019.02.21
  - [x] added summary for message that is visible in Activity pane
- 0.2.x / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
  - [x] added cross-platform (works on linux, mac os, windows)
  - [x] added azure pipelines
  - [x] added some pester tests

- 0.1.x / 2018.07.12
  [x] first release

## Documentation for Message Cards (for development)

This module uses Message Cards to send information to Teams. You can find out what is supported in [Legacy actionable message card reference](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference) just in case you would like to help out with development of this module.

Additional links:

- <https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/cards/cards-format>
- <https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference>

![Teams Card Explanatiuion](https://evotec.xyz/wp-content/uploads/2019/12/TeamsExplanation1.png)

![Teams Card Explanatiuion](https://evotec.xyz/wp-content/uploads/2019/12/TeamsExplanation2.png)

![Teams Card Explanatiuion](https://evotec.xyz/wp-content/uploads/2019/12/TeamsExplanation3.png)

```powershell
Send-TeamsMessage -Verbose {
    New-TeamsSection -ActivityTitle "**Elon Musk**" -ActivitySubtitle "@elonmusk - 9/12/2016 at 5:33pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg" -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732"
    New-TeamsSection -ActivityTitle "**Mark Knopfler**" -ActivitySubtitle "@MarkKnopfler - 9/12/2016 at 1:12pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/1042367841117384704/YvrqQiBK_400x400.jpg" -ActivityText "Mark Knopfler features on B.B King's all-star album of Blues greats, released on this day in 2005..."
    New-TeamsSection -ActivityTitle "**Elon Musk**" -ActivitySubtitle "@elonmusk - 9/12/2016 at 5:33pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg" -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732"
} -Uri $TeamsID -MessageSummary 'Tweet'
```

## Installing on Windows / Linux / MacOS

```powershell
Install-Module PSTeams
#Install-Module PSTeams -Scope CurrentUser
#Update-Module PSTeams
```

## Usage

```powershell
$TeamsID = 'YourCodeGoesHere'
$Button1 = New-TeamsButton -Name 'Visit English Evotec Website' -Link "https://evotec.xyz"
$Fact1 = New-TeamsFact -Name 'PS Version' -Value "**$($PSVersionTable.PSVersion)**"
$Fact2 = New-TeamsFact -Name 'PS Edition' -Value "**$($PSVersionTable.PSEdition)**"
$Fact3 = New-TeamsFact -Name 'OS' -Value "**$($PSVersionTable.OS)**"
$CurrentDate = Get-Date
$Section = New-TeamsSection `
    -ActivityTitle "**PSTeams**" `
    -ActivitySubtitle "@PSTeams - $CurrentDate" `
    -ActivityImage Add `
    -ActivityText "This message proves PSTeams Pester test passed properly." `
    -Buttons $Button1 `
    -ActivityDetails $Fact1, $Fact2, $Fact3
Send-TeamsMessage `
    -URI $TeamsID `
    -MessageTitle 'PSTeams - Pester Test' `
    -MessageText "This text won't show up" `
    -Color DodgerBlue `
    -Sections $Section
```

## How does it look like

- When executed from Linux

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb6509e8013e.png)

- When executed from Windows

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650ade0d73.png)

- When executed from MacOS

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650be35f4b.png)

- And this is more advanced option sent by [PSWinReporting](https://evotec.xyz/hub/scripts/pswinreporting-powershell-module/)

![image](https://evotec.xyz/wp-content/uploads/2018/09/img_5b9e830101081.png)
