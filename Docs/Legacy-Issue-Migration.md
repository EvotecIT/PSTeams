# PSTeams and PSDiscord issue migration

This inventory maps the current open issues in the two predecessor repositories to MessageX delivery work. It was refreshed from GitHub on 2026-08-21. An issue should be closed only after its replacement capability has exact source, package, PowerShell, and—where applicable—live provider proof.

## PSTeams open issues

| Issue | MessageX disposition | Completion evidence |
|---|---|---|
| [#61: New-AdaptiveCard behind a proxy](https://github.com/EvotecIT/PSTeams/issues/61) | The cleanup candidate adds proxy support to `New-AdaptiveCard` and other sending builders. Retain this as a Phase 1 transport contract. | Packed-module proxy test and a controlled proxy send. |
| [#59: Retirement of Office 365 connectors](https://github.com/EvotecIT/PSTeams/issues/59) | Power Automate Workflows are the primary simple Teams notification path. Connector-card compatibility remains bounded and must not imply long-term connector availability. | Workflow sends to a channel and chat, plus migration documentation. |
| [#58: ActivityImage](https://github.com/EvotecIT/PSTeams/issues/58) | Preserve the typed activity-image surface only where the selected Teams delivery format still renders it. Treat unsupported connector-era rendering explicitly. | Serialization fixture and live rendering decision for each retained card family. |
| [#40: Create a channel message, then reply](https://github.com/EvotecIT/PSTeams/issues/40) | This requires a Teams app/bot or governed Graph lifecycle path, not a send-only Workflow URL. It belongs to the typed conversation/message-reference and reply capability. | Initial send returns durable coordinates; replies land in the same channel thread after restart. |
| [#36: Send a file to a channel](https://github.com/EvotecIT/PSTeams/issues/36) | File upload belongs to authenticated message lifecycle support. Workflows must not claim it. | Provider capability discovery, upload result/reference, and live file round trip. |
| [#30: Underscores disappear from text](https://github.com/EvotecIT/PSTeams/issues/30) | Protect literal text through the System.Text.Json rendering path. | Serialization and packed-module fixtures containing underscores, slashes, Unicode, and emoji. |
| [#29: Inline images](https://github.com/EvotecIT/PSTeams/issues/29) | Keep data-image creation separate from transport, enforce provider limits, and keep diagnostics redacted. | File/URL/data fixtures for each supported card family and negative size-limit tests. |
| [#20: Missing help](https://github.com/EvotecIT/PSTeams/issues/20) | The stabilization candidate generates matching Markdown and MAML help for all 63 current commands. Continue the same contract for MessageX providers. | Zero placeholders, exact export/help parity, and `Get-Help` proof from the packed module. |

## PSDiscord open issues

| Issue | MessageX disposition | Completion evidence |
|---|---|---|
| [#7: New Discord Events](https://github.com/EvotecIT/PSDiscord/issues/7) | Split outbound scheduled-event administration from inbound interaction/Gateway events. Add only the provider operations that have a real C# and PowerShell use case. | Typed request/result contracts, permission/error fixtures, and live Discord proof. |
| [#3: Missing webhook message operations](https://github.com/EvotecIT/PSDiscord/issues/3) | Implement webhook get/edit/delete through focused lifecycle capabilities instead of a monolithic Discord client. | Create, retrieve, edit, and delete an application-owned webhook message using its durable reference. |

## Closed-issue regression themes

Closed issues remain useful regression input rather than migration work items. The provider suites should retain focused coverage for:

- literal text, Unicode, emoji, mentions, URLs, and escaping;
- empty and optional card collections;
- tables, wrapping, widths, activity images, and inline images;
- actionable HTTP failures, throttling, payload limits, and redacted diagnostics;
- portable PowerShell 5.1 and PowerShell 7 package loading;
- complete command help and examples that use installed public surfaces.

## Issue closure workflow

When a capability is complete:

1. link the implementation PR and the exact package/module version to the legacy issue;
2. state whether the behavior is supported, replaced by a different authenticated path, or intentionally unsupported;
3. include the smallest working migration example;
4. close only after the relevant public artifact is available, unless the issue was explicitly scoped to source-only work.
