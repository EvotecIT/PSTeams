
 <!---
[![Build Status Windows/Linux/Mac](https://dev.azure.com/evotecpl/PSTeams/_apis/build/status/EvotecIT.PSTeams)](https://dev.azure.com/evotecpl/PSTeams/_build/latest?definitionId=2)
--->

<center>

[![Build Status Windows/Linux/Mac](https://img.shields.io/azure-devops/build/evotecpl/50de6197-c7ea-433e-b6a6-689968cabe05/2.svg?label=builds%20on%20windows/macOs/linux&style=for-the-badge)](https://dev.azure.com/evotecpl/PSTeams/_build/latest?definitionId=2)
[![PowerShellGallery Version](https://img.shields.io/powershellgallery/v/PSTeams.svg?style=for-the-badge)](https://www.powershellgallery.com/packages/PSTeams)

[![PowerShellGallery Platform](https://img.shields.io/powershellgallery/p/PSTeams.svg?style=for-the-badge)](https://www.powershellgallery.com/packages/PSTeams)
[![PowerShellGallery Preview Version](https://img.shields.io/powershellgallery/vpre/PSTeams.svg?label=powershell%20gallery%20preview&colorB=yellow&style=for-the-badge)](https://www.powershellgallery.com/packages/PSTeams)


![Top Language](https://img.shields.io/github/languages/top/evotecit/psteams.svg?style=for-the-badge)
![Code](https://img.shields.io/github/languages/code-size/evotecit/psteams.svg?style=for-the-badge)
[![PowerShellGallery Downloads](https://img.shields.io/powershellgallery/dt/PSTeams.svg?style=for-the-badge)](https://www.powershellgallery.com/packages/PSTeams)

</center>

# PSTeams - PowerShell Module

[PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) is a **PowerShell Module** working on **Windows** / **Linux** and **Mac**. It allows to send notifications to _Microsoft Teams_. It's pretty flexible and provides a bunch of options.

For description and **advanced** usage visit [PSTeams](https://evotec.xyz/hub/scripts/psteams-powershell-module/) dedicated page.

## Updates

-   0.3.x / 2019.02.21
    -   added summary for message that is visible in Activity pane

-   0.2.x / 2018.10.04 - [full blog post](https://evotec.xyz/psteams-send-notifications-to-ms-teams-from-mac-linux-or-windows/)

    -   added cross-platform (works on linux, mac os, windows)
    -   added azure pipelines
    -   added some pester tests

-   0.1.x / 2018.07.12
    -   first release

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

-   When executed from Linux

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb6509e8013e.png)

-   When executed from Windows

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650ade0d73.png)

-   When executed from MacOS

![image](https://evotec.xyz/wp-content/uploads/2018/10/img_5bb650be35f4b.png)

-   And this is more advanced option sent by [PSWinReporting](https://evotec.xyz/hub/scripts/pswinreporting-powershell-module/)

![image](https://evotec.xyz/wp-content/uploads/2018/09/img_5b9e830101081.png)
