# PowerShell Surface

`main` exposes a cmdlet-only PowerShell API.

## Current Cmdlets

- `ConvertTo-TeamsJson`
- `New-TeamsMessage`
- `New-TeamsWebhookTarget`
- `Send-TeamsMessage`
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
- `New-TeamsAdaptiveToggleVisibilityAction`
- `New-TeamsAdaptiveActionSet`
- `New-TeamsAdaptiveTextRun`

## Design Rules

- New public PowerShell features should be implemented as C# cmdlets.
- `TeamsX.PowerShell` should stay thin over `TeamsX`.
- If a feature needs more composition support, add typed .NET models first, then expose cmdlets.
- Do not add new wrapper functions for old `PSTeams` command names on `main`.
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
