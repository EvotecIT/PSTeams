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
- `Module\PSTeams\PSTeams.psm1` now follows the `DnsClientX`-style development loader and prefers `net8.0` for PowerShell 7.x, with `net10.0` only as a fallback development build when present.
- Remaining work is now quality and parity polish: warnings cleanup, docs/examples refresh, and feature expansion on the typed cmdlet surface.
- Authenticated Graph lifecycle and governed Teams chat/channel delivery belong to GraphEssentialsX; TeamsX keeps webhook composition and delivery independent of a Graph SDK.

## Design Rules

- New public PowerShell features should be implemented as C# cmdlets.
- `TeamsX.PowerShell` should stay thin over `TeamsX`.
- If a feature needs more composition support, add typed .NET models first, then expose cmdlets.
- Keep the existing `PSTeams` public names available, but prefer implementing them as cmdlets or aliases.
- Delete PowerShell implementations only after the matching C# cmdlet path is in place and tested.
- Keep new delivery backends dependency-light; prefer direct HTTP clients over large SDK dependencies unless the SDK adds clear value.
- Use `Build\Build-Module.ps1` for the PowerForge project, module, documentation, and package flow.

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

## Microsoft Graph Boundary

TeamsX does not ship a second authenticated Graph client. GraphEssentialsX owns authentication, discovery, paging, throttling, lifecycle operations, and governed Teams chat/channel writes. A later MessageX adapter can compose TeamsX messages and delegate delivery to that owner after GraphEssentialsX is available as a consumable package.
