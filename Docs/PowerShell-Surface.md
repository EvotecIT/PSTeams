# PowerShell Surface

`main` exposes one `PSTeams` module whose public surface is now binary-backed through `TeamsX.PowerShell`, with the shipping shell in `Module\PSTeams`.

## Current Module Shape

- `TeamsX` is the reusable .NET library
- `TeamsX.PowerShell` is the thin binary cmdlet layer
- `Module\PSTeams` is the shipping module shell and alias bridge
- Legacy public names are preserved as cmdlets and aliases, not script functions
- The runtime module import is now binary-only: the shell loads `TeamsX.PowerShell.dll`, sets aliases, and does not dot-source public/private script functions
- The old module-local helper scripts and stale in-module legacy tests have been removed; the active validation suite lives in `Module\Tests`

## Current Cmdlets

- `New-AdaptiveAction`
- `New-AdaptiveActionSet`
- `New-AdaptiveCard`
- `New-AdaptiveColumn`
- `New-AdaptiveColumnSet`
- `New-AdaptiveContainer`
- `New-AdaptiveFact`
- `New-AdaptiveFactSet`
- `New-AdaptiveImage`
- `New-AdaptiveImageSet`
- `New-AdaptiveLineBreak`
- `New-AdaptiveMedia`
- `New-AdaptiveMediaSource`
- `New-AdaptiveMention`
- `New-AdaptiveRichTextBlock`
- `New-AdaptiveTable`
- `New-AdaptiveTextBlock`
- `ConvertTo-TeamsJson`
- `New-TeamsGraphTarget`
- `New-TeamsHeroCard`
- `New-TeamsThumbnailCard`
- `New-TeamsListCard`
- `New-TeamsMessage`
- `New-TeamsWebhookTarget`
- `New-TeamsAdaptiveCard`
- `New-TeamsAdaptiveTextBlock`
- `New-TeamsAdaptiveRichTextBlock`
- `New-TeamsAdaptiveImage`
- `New-TeamsAdaptiveImageSet`
- `New-TeamsAdaptiveMedia`
- `New-TeamsAdaptiveMediaSource`
- `New-TeamsAdaptiveMention`
- `New-TeamsAdaptiveFact`
- `New-TeamsAdaptiveFactSet`
- `New-TeamsAdaptiveContainer`
- `New-TeamsAdaptiveColumn`
- `New-TeamsAdaptiveColumnSet`
- `New-TeamsAdaptiveOpenUrlAction`
- `New-TeamsAdaptiveShowCardAction`
- `New-TeamsAdaptiveSubmitAction`
- `New-TeamsAdaptiveToggleVisibilityAction`
- `New-TeamsAdaptiveActionSet`
- `New-TeamsAdaptiveTextRun`

## Migration Status

- `FunctionsToExport` in `Module\PSTeams\PSTeams.psd1` is now empty.
- The whole `New-Adaptive*` surface is binary-backed on `main`.
- `Module\PSTeams\PSTeams.psm1` now prefers the highest compatible local PowerShell Core build, including `net10.0` when the current host runtime can load it.
- Remaining work is now quality and parity polish: warnings cleanup, docs/examples refresh, and feature expansion on the typed cmdlet surface.
- `TeamsX` now includes a Graph sender starter for channel and chat posts, exposed through `New-TeamsGraphTarget`.

## Design Rules

- New public PowerShell features should be implemented as C# cmdlets.
- `TeamsX.PowerShell` should stay thin over `TeamsX`.
- If a feature needs more composition support, add typed .NET models first, then expose cmdlets.
- Keep the existing `PSTeams` public names available, but prefer implementing them as cmdlets or aliases.
- Delete PowerShell implementations only after the matching C# cmdlet path is in place and tested.
- Keep new delivery backends dependency-light; prefer direct HTTP clients over large SDK dependencies unless the SDK adds clear value.
- Use `Build\Build-Project.ps1` for project/library release flow.
- Use `Module\Build\Build-Module.ps1` for PowerShell module packaging flow.

## Short Example

```powershell
$card = New-TeamsAdaptiveCard -Body @(
    New-TeamsAdaptiveTextBlock -Text 'Build failed' -Weight Bolder -Color Attention
    New-TeamsAdaptiveFactSet -Facts @(
        New-TeamsAdaptiveFact -Title 'Run' -Value '42'
        New-TeamsAdaptiveFact -Title 'Status' -Value 'Failed'
    )
) -Actions @(
    New-TeamsAdaptiveOpenUrlAction -Title 'Open build' -Url 'https://example.test/build/42'
)

$message = New-TeamsMessage -Summary 'Build notification' -AdaptiveCard $card
$json = $message | ConvertTo-TeamsJson
```

Typed wrapper cards can also be composed as objects and rendered through `ConvertTo-TeamsJson`:

```powershell
$target = New-TeamsWebhookTarget -Uri 'https://example.test/webhook'
$heroCard = New-TeamsHeroCard -Title 'Seattle Center Monorail' -Images @(
    New-TeamsCardImage -Url 'https://example.test/monorail.jpg' -AlternateText 'Monorail'
) -Buttons @(
    New-CardListButton -Type OpenUrl -Title 'Official website' -Value 'https://example.test'
)

Send-TeamsMessage -HeroCard $heroCard -Target $target

$json = $heroCard | ConvertTo-TeamsJson
$wrapped = $json | Send-TeamsMessageBody -Uri 'https://example.test/webhook' -Wrap -Supress:$false -WhatIf
```

## Graph Starter

`main` now includes a starter Graph target cmdlet for chat and channel posts:

```powershell
$message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42 stopped.'
$target = New-TeamsGraphTarget -ChatId '19:testchat@thread.v2' -AccessTokenVariableName 'TEAMSX_GRAPH_TOKEN'

Send-TeamsMessage -Message $message -Target $target
```

Current scope:

- plain typed messages are rendered as Graph HTML message bodies
- adaptive cards are sent as Graph attachments
- adaptive cards should currently stick to `Action.OpenUrl`
- typed wrapper-card direct sending currently targets incoming and workflow webhooks only
- Graph targets can use a plain token, a secure string, or an environment-variable-backed token provider
- normal Graph chat/channel posting should use delegated tokens; application permissions are documented as migration-only for these endpoints
