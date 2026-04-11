---
title: "Send a simple Teams message"
description: "Use PSTeams to send a straightforward Microsoft Teams webhook message."
layout: docs
---

This pattern is useful when a script only needs to notify a channel that something happened.

It comes from the source example at `Examples/Example-Short.ps1`.

## When to use this pattern

- You need a low-friction channel notification.
- A full card is unnecessary.
- The webhook URI is already managed as a secret.

## Example

```powershell
Import-Module .\PSTeams.psd1 -Force

$teamsWebhook = $env:TEAMS_WEBHOOK_URL
Send-TeamsMessage -URI $teamsWebhook -MessageText 'Deployment finished successfully.'
```

## What this demonstrates

- sending a basic webhook message
- keeping the webhook outside the script
- using Teams as an operations notification target

## Source

- [Example-Short.ps1](https://github.com/EvotecIT/PSTeams/blob/master/Examples/Example-Short.ps1)

