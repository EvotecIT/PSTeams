# Moving to MessageX

MessageX is the successor to PSTeams and PSDiscord. It supplies one PowerShell module and separate .NET provider packages for Teams, Slack, and Discord. The initial candidate is unpublished; installing PSTeams or PSDiscord from PowerShell Gallery still installs the historical implementation.

## Module identity

New scripts use `Import-Module MessageX`. MessageX has a new module GUID and starts at `0.1.0`; it does not continue PSTeams' `2.4.x` version sequence. Public installation instructions will be added when the signed candidate is published and verified.

Do not import the old modules into the same session. Some command names overlap but accept different parameters or return typed objects instead of dictionaries. During migration, start a fresh PowerShell process and use module-qualified names such as `MessageX\Send-DiscordMessage` when command resolution could be ambiguous.

## Discord

The simple notification path is:

```powershell
Import-Module MessageX
Send-DiscordMessage -Text 'Release ready' -WebhookUri $env:MESSAGEX_DISCORD_WEBHOOK_URL -PassThru -ErrorAction Stop
```

Use `New-DiscordMessage`, `New-DiscordSection`, and `New-DiscordWebhookTarget` for typed rich messages. Use `ConvertTo-DiscordJson` for serialization without sending. Old `-Sections`, `-CreateConfig`, and `-OutputJSON` send-command parameters are not compatibility promises. Store webhook URLs and bot tokens in a secret store or process environment; MessageX does not migrate the old `.psdiscord` configuration file.

Ordinary webhooks can send link buttons. Buttons and selects that invoke an application require a bot or application-owned webhook. Mention parsing defaults to nobody; configure an explicit allowed-mentions policy to notify users or roles.

## Teams

Create a Teams Workflow and use `New-TeamsWebhookTarget -Workflow` with a webhook-supported Adaptive Card. Microsoft retired Office 365 connectors in May 2026. Replacing the module does not reactivate an old connector URL.

Workflows accept notifications; their URLs do not supply reads, replies, edits, deletes, proactive bot delivery, or Graph administration. Check the Workflow run and the rendered Teams message during migration. An accepted HTTP request is not confirmation that a later Workflow action succeeded.

Familiar card builders remain where useful. Prefer the typed `New-TeamsAdaptive*` commands for new scripts. MessageCard HTTP action buttons do not become supported merely by changing the endpoint; redesign interactive flows around supported Adaptive Cards and the appropriate application transport.

## Retirement sequence

PSTeams and PSDiscord remain available while MessageX is validated. After the replacement is published and its installation path is proven, update their project notices and archive the retired repositories. Repository identity, redirects, tags, and historical releases must be settled before archiving; archiving the current repository would also freeze MessageX while its source is hosted here.

No separate legacy implementation or compatibility wrapper is planned. Report a missing capability in terms of the operation needed, so it can be implemented once in the owning MessageX library.
