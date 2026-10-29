---
title: "Compose a notification card"
description: "Build a Teams Workflow Adaptive Card with facts, incident ID, fallback text, and a report link."
layout: docs
---

Import the locally built MessageX module as shown on the [installation page](/projects/psteams/docs/install/). This example needs no endpoint. It produces the Workflow JSON payload for inspection.

```powershell
$reportUrl = 'https://example.test/reports/inc-42'
$card = New-TeamsAdaptiveCard -Body {
    New-TeamsAdaptiveTextBlock -Text 'Monitoring alert'
    New-TeamsAdaptiveTextBlock -Text 'The health probe failed for server\_name.'
    New-TeamsAdaptiveFactSet -Facts {
        New-TeamsAdaptiveFact -Title 'Incident ID' -Value 'inc-42'
        New-TeamsAdaptiveFact -Title 'Severity' -Value 'Critical'
    }
} -Actions {
    New-TeamsAdaptiveOpenUrlAction -Title 'Open report' -Url $reportUrl
} -FallbackText "Critical: Monitoring alert. Probe failed. Incident inc-42. $reportUrl"

$message = New-TeamsMessage -AdaptiveCard $card -Summary 'Critical monitoring alert'
$message | ConvertTo-TeamsJson | ConvertFrom-Json
```

The backslash in `server\_name` is intentional: Teams Markdown can treat an underscore as formatting. MessageX preserves authored text and does not escape every underscore automatically. `Action.OpenUrl` is the supported Workflow action for a report link; submit, toggle, and reply interactions need another verified transport.
