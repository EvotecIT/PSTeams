# From PSTeams to MessageX

PSTeams 2.4.0 remains on PowerShell Gallery for existing scripts. MessageX is a new, unpublished module and does not load or translate PSTeams commands. Create a Teams Workflow and use its new HTTPS URL; Office 365 connector URLs stopped working in May 2026.

| PSTeams command or pattern | MessageX command or status |
| --- | --- |
| `Send-TeamsMessage -Uri ... -MessageText ...` | `New-TeamsWebhookTarget`, `New-TeamsAdaptiveCard`, `New-TeamsMessage`, then `Send-TeamsMessage -Message ... -Target ...`. The target is always a Workflow. |
| `New-AdaptiveCard { ... } -Uri ...` | `New-TeamsAdaptiveCard -Body { ... }`, `New-TeamsMessage`, then `Send-TeamsMessage`. The typed builders use `New-TeamsAdaptive*` names. |
| `New-AdaptiveCard -Proxy ...` | `Send-TeamsMessage -Proxy ...`. The card builder has no HTTP transport; the send command owns proxy, timeout, and user-agent settings. |
| `New-AdaptiveAction -ActionUrl ...` | `New-TeamsAdaptiveOpenUrlAction -Title ... -Url ...`. Workflow actions are URL links only. |
| `New-TeamsSection`, `New-TeamsFact`, `ConvertTo-TeamsSection`, `ConvertTo-TeamsFact` | Not supported: these build retired connector MessageCards. Use `New-TeamsAdaptiveFactSet` and `New-TeamsAdaptiveFact`. |
| `New-HeroCard`, `New-ThumbnailCard`, `New-CardList` and their builders | Not supported: wrapper card send routes were removed with the connector path. Compose an Adaptive Card. |
| `Send-TeamsMessageBody` | Not supported: raw connector JSON bypasses the Workflow message contract. Use `ConvertTo-TeamsJson` to inspect a typed message without sending. |
| `ActivityImage`, `TeamsMessage` and other short aliases | Not supported: the unreleased MessageX surface has no compatibility aliases. |
| Connector HTTP action buttons, Adaptive Card submit/show/toggle/execute actions | Not supported by the send-only Workflow transport. Use `Action.OpenUrl` for a report link. |
| File upload to a Teams channel or reply to a Teams message | Not supported by a Workflow URL. These operations need a separate verified app or Graph collaboration route. |

An underscore in Teams text can be interpreted as Markdown formatting by the Teams client. If the intended output is a literal underscore, escape it in the authored text as `server\_name`; MessageX does not rewrite all Markdown text automatically. Check the actual Workflow run and rendered channel message after migrating a script. An HTTP success only confirms that the Workflow endpoint accepted the request.
