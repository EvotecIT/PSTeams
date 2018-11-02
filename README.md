# PSTeams - PowerShell Module

[![Build Status Windows/Linux/Mac](https://dev.azure.com/evotecpl/PSTeams/_apis/build/status/EvotecIT.PSTeams)](https://dev.azure.com/evotecpl/PSTeams/_build/latest?definitionId=2)

[PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) is a **PowerShell Module** working on **Windows** / **Linux** and **Mac**. It allows to send notifications to *Microsoft Teams*. It's pretty flexible and provides a bunch of options.

For description and **advanced** usage visit [PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) dedicated page.

## Updates
- 0.2.x / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)
    - added cross-platform (works on linux, mac os, windows)
    - added azure pipelines
    - added some pester tests

- 0.1.x / 2018.07.12
    - first release

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

## How does it look like?

- When executed from Linux

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb6509e8013e.png)

- When executed from Windows

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650ade0d73.png)

- When executed from MacOS

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650be35f4b.png)

- And this is more advanced option sent by [PSWinReporting](https://evotec.xyz/hub/scripts/pswinreporting-powershell-module/)

![image](https://evotec.xyz/wp-content/uploads/2018/09/img_5b9e830101081.png)
