---
title: "Install MessageX"
description: "Build and import the MessageX preview until its coordinated release."
layout: docs
---

MessageX has not been published to PowerShell Gallery. `Install-Module PSTeams` installs the historical 2.4.0 module, not MessageX. To try the current source candidate locally, install the .NET SDK selected by `global.json` and PSPublishModule, then run:

```powershell
git clone https://github.com/EvotecIT/PSTeams.git
Set-Location ./PSTeams
Install-Module PSPublishModule -RequiredVersion 3.0.170 -Scope CurrentUser
./Build/Build-Module.ps1 -ConfigurationGateMode Build -SignModule:$false
Import-Module ./Module/MessageX/MessageX.psd1 -Force
Get-Command -Module MessageX
```

The build creates unsigned local validation files. The first public MessageX release will have separate installation instructions after its signed package and provider checks pass. See the [Workflow example](/projects/psteams/examples/send-simple-teams-message/) for a message you can render locally before configuring a destination.
