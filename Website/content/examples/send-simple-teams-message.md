---
title: "Send a Teams Workflow notification"
description: "Render a MessageX Adaptive Card locally, then send it to a configured Workflow."
layout: docs
---

Import the locally built MessageX module as shown on the [installation page](/projects/psteams/docs/install/). This complete example renders JSON without a network request. It sends only when you set `MESSAGEX_TEAMS_WORKFLOW_URL` to a Workflow URL you control.

```powershell
$card = New-TeamsAdaptiveCard -Body {
    New-TeamsAdaptiveTextBlock -Text 'Deployment finished successfully.'
} -FallbackText 'Deployment finished successfully.'
$message = New-TeamsMessage -AdaptiveCard $card -Summary 'Deployment finished'

$message | ConvertTo-TeamsJson | ConvertFrom-Json

if ($env:MESSAGEX_TEAMS_WORKFLOW_URL) {
    $target = New-TeamsWebhookTarget -Uri $env:MESSAGEX_TEAMS_WORKFLOW_URL -Destination Channel
    Send-TeamsMessage -Message $message -Target $target -PassThru
}
```

Create the Workflow in Teams and keep its URL in a secret store or process environment. A successful HTTP response means the Workflow accepted the request; inspect the Workflow run and the rendered channel message to confirm delivery. The Workflow target is send-only.
