# PSTeams and MessageX

PSTeams is evolving from a Teams-focused PowerShell module into MessageX: reusable, provider-native .NET libraries with thin PowerShell cmdlets for Microsoft Teams, Slack, and Discord.

[![Test .NET](https://github.com/EvotecIT/PSTeams/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/EvotecIT/PSTeams/actions/workflows/test-dotnet.yml)
[![Test PowerShell](https://github.com/EvotecIT/PSTeams/actions/workflows/test-powershell.yml/badge.svg)](https://github.com/EvotecIT/PSTeams/actions/workflows/test-powershell.yml)
[![license](https://img.shields.io/github/license/EvotecIT/PSTeams.svg)](LICENSE)

## Release status

The PowerShell Gallery currently contains the historical PSTeams release. The MessageX NuGet packages and the rebuilt PSTeams binary module described below are release candidates in this repository; they are not public packages yet.

Do not use `Install-Module PSTeams` as proof that the MessageX code is installed. Before publication, build the staged artifacts locally and test them as described in [ROADMAP.md](ROADMAP.md).

## Design

MessageX keeps one reusable C# owner for each capability and leaves PowerShell, ASP.NET Core, and product integrations as thin surfaces:

- `MessageX.Core` owns delivery results, durable references, capability flags, errors, bounded provider data, and shared HTTP behavior.
- `MessageX.Teams` owns Teams webhook payloads, Adaptive Cards, webhook-safe actions, and typed future bot-transport models.
- `MessageX.Slack` owns Slack incoming webhooks, Web API messaging, Block Kit, file upload, and transient interaction responses.
- `MessageX.Discord` owns Discord webhooks, bot REST messaging, embeds, attachments, components, and transient interaction responses.
- `MessageX.Hosting` owns provider-neutral routing, acknowledgement deadlines, deduplication, queues, retries, and durable dispatch contracts.
- Provider hosting packages verify native requests and project them into the shared hosting pipeline.
- `MessageX.Persistence.DbaClientX` persists MessageX domain state through DbaClientX without taking ownership of database-provider behavior.
- `MessageX.PowerShell` exposes compiled cmdlets over the same libraries.

Provider-native rich content stays provider-native. MessageX does not flatten Adaptive Cards, Block Kit, and Discord components into a lowest-common-denominator document model.

## Current capability matrix

| Capability | Teams | Slack | Discord |
| --- | --- | --- | --- |
| Notification send | Workflow and incoming webhook | Incoming webhook and bot Web API | Incoming webhook and bot REST |
| Rich content | Adaptive Cards, webhook-safe actions, legacy wrapper cards | Sections, headers, context, actions, buttons, modal inputs | Embeds, attachments, buttons, selects, modal inputs |
| Message lifecycle | Workflow URLs remain send-only | Reply, update, delete, reactions | Reply, read, update, delete, reactions |
| File delivery | Images and provider card media | Current external upload workflow | Multipart attachments |
| Verified HTTP receive | Teams app activities and card actions | Events API, commands, actions, views | Commands, components, autocomplete, modals |
| Interaction continuation | Verified app activity receipt; bot-owned outbound actions are deferred | Transient response URL and `views.open` | Follow-up, edit, and delete within token lifetime |
| Durable hosting | Shared queue, replay, retry, dead-letter, health, DbaClientX adapter | Shared | Shared |
| Realtime connection | Deferred | Socket Mode deferred | Gateway deferred |

Teams Graph administration and collaboration lifecycle belong in GraphEssentialsX. MessageX does not embed a second Microsoft Graph client.

## PowerShell quick start

Credentials should come from environment variables, a secret store, or `SecureString`; never commit webhook URLs or tokens.

### Teams Workflow notification

```powershell
$target = New-TeamsWebhookTarget `
    -Uri $Env:MESSAGEX_TEAMS_WORKFLOW_URL `
    -Workflow `
    -Destination Channel `
    -DisplayName 'Release alerts'

$openBuild = New-TeamsAdaptiveOpenUrlAction `
    -Title 'Open build' `
    -Url 'https://example.com/build/42'

$card = New-TeamsAdaptiveCard -Version '1.2' -Body @(
    New-TeamsAdaptiveTextBlock -Text 'Build 42 is ready' -Weight Bolder
) -Actions $openBuild

$message = New-TeamsMessage -Summary 'Build ready' -AdaptiveCard $card
Send-TeamsMessage -Message $message -Target $target -PassThru
```

Workflow URLs are send-only capabilities. They do not grant conversation reads, replies, updates, deletes, or inbound events. `Action.Execute` and Adaptive Card refresh require a bot-owned outbound transport, so current Workflow and incoming-webhook targets reject those contracts before sending.

### Slack Block Kit and file upload

```powershell
$connection = New-SlackConnection `
    -BotToken (Read-Host 'Slack bot token' -AsSecureString) `
    -WorkspaceId 'T0123456789'
$target = New-SlackConversationTarget -ConversationId 'C0123456789' -DisplayName 'Release alerts'

$message = New-SlackMessage -Text 'Build 42 is ready' -Blocks @(
    New-SlackHeader -Text 'Build approval'
    New-SlackSection -Markdown 'Build *42* is ready for review.'
    New-SlackActions -Elements @(
        New-SlackButton -Text 'Approve' -ActionId 'approve-build' -Value '42' -Style Primary
        New-SlackButton -Text 'Reject' -ActionId 'reject-build' -Value '42' -Style Danger
    )
)

Send-SlackMessage -Message $message -Target $target -Connection $connection -PassThru
Send-SlackFile -Path .\build.log -ConversationId 'C0123456789' -Connection $connection -PassThru
```

`Send-SlackFile` uses `files.getUploadURLExternal`, the provider upload URL, and `files.completeUploadExternal`. It does not use the retired `files.upload` API.

### Discord components

```powershell
$connection = New-DiscordConnection -BotToken (Read-Host 'Discord bot token' -AsSecureString)
$target = New-DiscordChannelTarget -ChannelId '123456789012345678' -GuildId '223456789012345678'

$message = New-DiscordMessage -Content 'Build 42 is ready' -Components @(
    New-DiscordActionRow -Components @(
        New-DiscordButton -Label 'Approve' -CustomId 'approve-build' -Style Success
        New-DiscordButton -Label 'Open build' -Url 'https://example.com/build/42'
    )
)

Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru
```

Discord mention parsing defaults to nobody. Opt in with an explicit `New-DiscordAllowedMentions` policy.

## C# example

```csharp
using MessageX.Slack;

var connection = SlackConnection.ForBotToken(
    Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN")!,
    workspaceId: "T0123456789");

var message = new SlackMessageRequest { Text = "Deployment completed" };
message.Blocks.Add(new SlackHeaderBlock {
    Text = SlackTextObject.Plain("Release")
});
message.Blocks.Add(new SlackSectionBlock {
    Text = SlackTextObject.Markdown("Production deployment completed successfully.")
});

using var client = new SlackClient(connection);
var result = await client.SendAsync(
    message,
    SlackMessageTarget.ForConversation("C0123456789"));

if (!result.IsSuccess) {
    throw new InvalidOperationException($"Slack delivery failed: {result.ErrorKind}");
}
```

## Hosting

The ASP.NET Core provider packages expose verified endpoints over the shared MessageX host. The host acknowledges within provider deadlines, bounds synchronous work, deduplicates retries, and can move asynchronous work into durable storage.

Routes that must open a Slack or Discord modal use an explicit inline registration so the handler runs before the provider acknowledgement:

```csharp
router.OnAction<SlackInteractionEvent>(
    "open-approval",
    HandleApprovalAsync,
    MessageDispatchMode.Synchronous);
```

Keep ordinary handlers deferred. Synchronous registrations use bounded process-local replay protection and cannot be persisted or replayed after restart.

Transient Slack response URLs, Slack trigger IDs, and Discord interaction tokens never enter durable codecs. A restored durable event intentionally cannot use those short-lived capabilities.

Realtime transports are deliberately outside the first package candidate. Slack Socket Mode and the Discord Gateway need separate reconnect, heartbeat, sequence, resume, and health contracts; HTTP receive remains the supported production path for this preview.

## Supported runtimes

- `MessageX.Core`, provider libraries, hosting core, and `MessageX.PowerShell`: .NET Framework 4.7.2, .NET 8, and .NET 10 where the project contract allows it.
- ASP.NET Core hosting and DbaClientX persistence: .NET 8 and .NET 10.
- PowerShell: Windows PowerShell 5.1 and supported PowerShell 7 runtimes through target-specific binary assets.

## Build and test

```powershell
dotnet restore Sources/MessageX.slnx
dotnet build Sources/MessageX.slnx --configuration Release --no-restore
dotnet test --project Sources/MessageX.Tests/MessageX.Tests.csproj --configuration Release --no-build
```

NuGet staging is configured by `Build/project.build.json`. The PowerShell module is built by `Build/Build-Module.ps1` through PSPublishModule/PowerForge. Both publish paths are disabled by default.

## Compatibility and boundaries

- The `legacy` branch preserves the historical script implementation.
- Existing PSTeams command names remain available where they represent supported behavior.
- New code should target typed `MessageX.*` libraries and binary cmdlets rather than adding another PowerShell implementation layer.
- Repository renaming and public package publication are separate maintainer decisions; neither is implied by a local release-candidate build.

See [ARCHITECTURE.md](ARCHITECTURE.md), [SUPPORT.md](SUPPORT.md), and [ROADMAP.md](ROADMAP.md) for ownership, supported boundaries, and the remaining release gates.

## License and support

This project is licensed under the repository [LICENSE](LICENSE). Issues and focused pull requests are welcome. Commercial users can support ongoing maintenance through [GitHub Sponsors](https://github.com/sponsors/PrzemyslawKlys).
