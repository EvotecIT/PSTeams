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
  <a href="https://github.com/EvotecIT/PSTeams"><img src="https://img.shields.io/powershellgallery/dt/PSTeams.svg"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=social"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn"></a>
</p>

# PSTeams - PowerShell Module

[PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) is a **PowerShell Module** working on **Windows** / **Linux** and **Mac**. It allows to send notifications to _Microsoft Teams_. It's pretty flexible and provides a bunch of options.

For description and **advanced** usage visit [PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) dedicated page.

## Readme Links

While I didn't spent much time creating WIKI, working on `Get-Help` documentation, I did write 3 articles that should help you get started.

- [x] https://evotec.xyz/hub/scripts/psteams-powershell-module/
- [x] https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/
- [x] https://evotec.xyz/sending-to-microsoft-teams-from-powershell-just-got-easier-and-better/

## Updates

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
  -[x] added summary for message that is visible in Activity pane
- 0.2.x / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
  - [x] added cross-platform (works on linux, mac os, windows)
  - [x] added azure pipelines
  - [x] added some pester tests

- 0.1.x / 2018.07.12
  [x] first release

## Documentation for Message Cards (for development)

This module uses Message Cards to send information to Teams. You can find out what is supported in [Legacy actionable message card reference](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference) just in case you would like to help out with development of this module.

Additional links:

- https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/cards/cards-format
- https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference

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
