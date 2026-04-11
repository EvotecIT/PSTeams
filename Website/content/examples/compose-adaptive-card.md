---
title: "Compose an Adaptive Card"
description: "Use PSTeams to compose a basic Adaptive Card payload."
layout: docs
---

This pattern is useful when a notification needs structure instead of one line of text.

It comes from the source example at `Examples/Adaptive Card/AdaptiveCard-Native01.ps1`.

## When to use this pattern

- You need a structured message in Teams.
- The payload should be authored in PowerShell.
- Operators need facts or grouped information in one card.

## Example

```powershell
Import-Module .\PSTeams.psd1 -Force

New-AdaptiveCard {
    New-AdaptiveTextBlock -Size 'Medium' -Weight Bolder -Text 'Backup summary' -Wrap
    New-AdaptiveFactSet {
        New-AdaptiveFact -Title 'Server' -Value 'DC01'
        New-AdaptiveFact -Title 'Status' -Value 'Completed'
    }
} -Uri $env:TEAMS_WEBHOOK_URL
```

## What this demonstrates

- building card structure with commands
- grouping facts for operators
- sending the payload to a webhook

## Source

- [AdaptiveCard-Native01.ps1](https://github.com/EvotecIT/PSTeams/blob/master/Examples/Adaptive%20Card/AdaptiveCard-Native01.ps1)

