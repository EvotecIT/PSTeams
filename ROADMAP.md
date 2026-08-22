# MessageX Roadmap

MessageX is a C#-first messaging toolkit for Microsoft Teams, Slack, Discord, and future providers. It should make simple notification delivery easy, support real conversations when an application needs them, and give PowerShell users provider-native commands without forcing them through a generic ChatOps abstraction.

This document is the working product and delivery roadmap. It records decisions, boundaries, phases, validation expectations, and the conditions that must be met before MessageX is considered ready for other Evotec projects and public PowerShell use.

## Outcome

MessageX succeeds when:

- a C# project can reference only the provider packages it needs and send a useful message with a small, typed API;
- a service can receive, verify, acknowledge, route, and respond to provider events without implementing each protocol itself;
- a Windows PowerShell 5.1 or PowerShell 7 user can send messages with commands such as `Send-TeamsMessage`, `Send-SlackMessage`, and `Send-DiscordMessage`;
- PowerShell users retain provider-native builders such as `New-AdaptiveCard`, rather than learning an artificial cross-provider card language;
- TestimoX, EventViewerX, and other Evotec C# projects consume MessageX through thin adapters instead of carrying their own Teams, Slack, Discord, retry, authentication, or webhook clients;
- provider limitations are represented as capabilities and documented behavior, not hidden behind methods that fail at runtime;
- NuGet packages, the PowerShell module, generated documentation, and release artifacts are built and versioned through PowerForge and PSPublishModule;
- local source, packed artifacts, published packages, downstream adoption, and deployed service state are validated as separate release stages.

## Current decisions

- [x] Reuse the existing `EvotecIT/PSTeams` GitHub repository and rename it to `EvotecIT/MessageX` so repository history, stars, issues, links, and community continuity are retained.
- [x] Use the current non-legacy `main` branch and its TeamsX C# migration as the implementation baseline; evolve and rename it rather than replacing it.
- [x] Treat "clean start" as intentional source-tree, API, namespace, package, documentation, and build cleanup—not an orphan branch, history reset, or rewrite of working TeamsX capabilities.
- [x] Complete repository hygiene and TeamsX stabilization before renaming projects, adding providers, or expanding the public API.
- [x] Preserve legacy branches and tags; do not rewrite repository history to make the source tree look new.
- [x] Keep the retired `EvotecIT/PSTeams` repository name unused after the rename so GitHub redirects continue to work.
- [x] Use **MessageX** as the working product, renamed repository, assembly, package-prefix, namespace, and PowerShell module name.
- [x] Keep PowerShell commands provider-specific and familiar.
- [x] Keep reusable behavior in C# and the PowerShell surface thin.
- [x] Target `net472`, `net8.0`, and `net10.0` for core and provider libraries.
- [x] Use `System.Text.Json` internally and avoid serializer-specific public contracts.
- [x] Own focused provider protocol clients instead of making SlackNet, Discord.Net, NetCord, or a Teams SDK foundational dependencies.
- [x] Use BCL cryptography for Slack HMAC verification.
- [x] Isolate `BouncyCastle.Cryptography` inside MessageX.Discord for Ed25519 verification where required.
- [x] Keep Microsoft identity/JWT packages optional and isolated to Teams authentication support.
- [x] Keep GraphEssentialsX as the owner of authenticated Microsoft Graph collaboration lifecycle behavior.
- [x] Treat Telegram and WhatsApp as future candidates, not initial scope.
- [x] Defer commercial and distribution terms until the project is ready for that decision.

## Existing TeamsX baseline

The current PSTeams `main` branch already contains the completed first C# migration and is the authoritative starting point. At the roadmap audit, `main` was `f712d1c4c498cb1514f63daf80601bdecf796377`; the former `feature/teamsx-csharp-migration-phase1` head was an ancestor and `main` was ten commits ahead. This baseline already provides:

- `TeamsX`, `TeamsX.PowerShell`, `TeamsX.Tests`, and `TeamsX.Examples` projects;
- typed Teams targets, composition, Workflow/webhook delivery, starter Graph delivery, and Adaptive Card support;
- compiled PowerShell cmdlets including `Send-TeamsMessage`, `Send-TeamsMessageBody`, and the existing `New-Adaptive*` and `New-Teams*` builders;
- multi-target builds covering Windows PowerShell 5.1 and modern PowerShell/.NET consumers;
- tests and packaging work that should be retained and strengthened.

The intended evolution is therefore:

```text
TeamsX                 -> MessageX.Teams
TeamsX.PowerShell      -> MessageX.PowerShell
TeamsX.Tests           -> MessageX provider/core test projects
TeamsX.Examples        -> MessageX provider and cross-provider examples
Module/PSTeams         -> Module/MessageX
```

This mapping is architectural direction, not permission for a mechanical rename. Each current public API, cmdlet, test, example, and build behavior must be classified as **retain**, **refactor**, **replace**, or **remove with migration guidance** before it changes.

## Product principles

### C# owns behavior

Provider clients, payload validation, serialization, authentication, request verification, retries, rate-limit handling, event parsing, conversation addressing, and message lifecycle behavior belong in MessageX C# libraries.

PowerShell cmdlets should bind parameters, construct typed objects, call the C# API, honor cancellation and `ShouldProcess` where appropriate, and return useful typed results. Cmdlets must not carry a second implementation of provider behavior.

### Provider-native experiences stay native

MessageX may share basic text, delivery results, event metadata, and handler mechanics. It must not pretend these formats are interchangeable:

- Teams Adaptive Cards and Activities;
- Slack Block Kit, views, and interaction payloads;
- Discord embeds, components, modals, replies, and thread channels.

A portable plain-text fallback is useful. Arbitrary conversion between rich provider formats is not an initial goal.

### Simple delivery stays simple

Sending one notification must not require service hosting, persistence, dependency injection, OAuth installation flows, or a generic ChatOps runtime.

Each provider should offer a direct path for its simplest supported delivery mechanism and a richer client path for applications that need conversations, lifecycle operations, or inbound events.

### Common abstractions must be earned

The first vertical slices should prove real Teams, Slack, and Discord behavior. Shared contracts should be extracted only where at least two providers and real consumers demonstrate the same semantic need.

MessageX must not start with a large `IMessageProvider` interface where unsupported methods throw. Prefer small capability interfaces and provider-specific clients.

### Dependencies remain contained

A consumer of MessageX.Teams must not acquire Discord or Slack dependencies. A consumer of MessageX.Core must not acquire provider clients. PowerShell packaging may bundle all supported providers, but C# consumers choose packages individually.

Public models must not expose `JsonDocument`, `JsonElement`, Newtonsoft.Json types, Bouncy Castle types, HTTP implementation types, or third-party SDK models as stable API contracts.

## Scope

### Initial providers

1. Microsoft Teams
2. Slack
3. Discord

### Initial capability levels

1. **Notify** - send text or native rich content to a configured destination.
2. **Reply** - reply in a conversation, thread, or provider-specific reply chain.
3. **Manage** - retrieve the resulting reference and update or delete messages created by the application where supported.
4. **Interact** - receive commands, mentions, buttons, forms, and modal submissions.
5. **Converse** - receive ordinary messages in supported scopes and route them to application handlers.
6. **Proactive** - send later using a stored installation and conversation reference.

### Explicit non-goals for the first stable release

- Cross-provider message mirroring or synchronization.
- A universal rich-card language.
- AI or LLM conversation orchestration.
- Presence, voice, calls, meetings, streaming, or media sessions.
- Full tenant, workspace, server, role, and channel administration.
- Historical export or compliance discovery across every provider.
- Automatic migration of PSTeams or PSDiscord scripts.
- Discord self-bots, user tokens, or unsupported automation.
- WhatsApp or Telegram implementation before the provider-extension gate is met.

## Proposed repository and package structure

The existing repository provides continuity, while the default source tree becomes MessageX through reviewed changes:

```text
EvotecIT/PSTeams  ->  EvotecIT/MessageX
```

This preserves the value of the existing repository and the current TeamsX implementation without freezing its present internal architecture.

```text
MessageX/
  Build/
    Build-Module.ps1
    project.build.json
  Sources/
    MessageX.slnx
    MessageX.Core/
    MessageX.Teams/
    MessageX.Teams.Graph/
    MessageX.Slack/
    MessageX.Discord/
    MessageX.Hosting/
    MessageX.Hosting.AspNetCore/
    MessageX.Persistence.DbaClientX/
    MessageX.PowerShell/
    MessageX.Tests/
    MessageX.IntegrationTests/
    MessageX.Examples/
  Module/
  Docs/
  Examples/
  README.md
  ROADMAP.md
```

The final shape should stay proportional to delivered capability. Projects should be introduced when their first real contract is implemented, not created as empty placeholders.

### Package responsibilities

| Package or project | Responsibility | Target frameworks |
|---|---|---|
| `MessageX.Core` | Small shared contracts, capability metadata, delivery results, common errors, event envelopes | `net472;net8.0;net10.0` |
| `MessageX.Teams` | Workflows, Teams bot/agent protocol, Activities, Adaptive Cards, Teams targets and events | `net472;net8.0;net10.0` |
| `MessageX.Teams.Graph` | Optional adapter to published GraphEssentialsX capabilities | Match usable GraphEssentialsX targets |
| `MessageX.Slack` | Incoming webhooks, Web API, Block Kit, Events API, interactions, Socket Mode | `net472;net8.0;net10.0` |
| `MessageX.Discord` | Incoming webhooks, bot REST API, embeds/components, interactions, Gateway | `net472;net8.0;net10.0` |
| `MessageX.Hosting` | Host-neutral routing, handler contracts, acknowledgement and dispatch pipeline | `net472;net8.0;net10.0` |
| `MessageX.Hosting.AspNetCore` | HTTP endpoints, middleware, health checks, dependency injection, hosted services | `net8.0;net10.0` |
| `MessageX.Persistence.DbaClientX` | Optional durable installations, references, deduplication, cursors, and outbox state | Match usable DbaClientX targets |
| `MessageX.PowerShell` | Compiled thin cmdlets and PowerShell-facing type surface | `net472;net8.0;net10.0` |
| `MessageX` PowerShell module | Bundled module selected correctly for Windows PowerShell and PowerShell 7 | PowerShell 5.1 and supported PowerShell 7 releases |

There should be no aggregate NuGet package that silently installs every provider in the first release. Revisit an aggregate package only if real C# consumers benefit from it.

## Shared contract design

### Addressing

Use typed provider targets rather than a provider name plus arbitrary string identifiers.

Examples include:

- `TeamsWorkflowTarget`
- `TeamsConversationTarget`
- `TeamsChannelTarget`
- `TeamsChatTarget`
- `SlackWebhookTarget`
- `SlackConversationTarget`
- `DiscordWebhookTarget`
- `DiscordChannelTarget`
- `DiscordThreadTarget`
- `DiscordDirectMessageTarget`

Provider targets should carry non-secret routing coordinates. Credentials belong to a connection or credential provider, not inside serializable targets.

### Message and result references

Every successful send should return a typed result containing the identifiers needed for supported follow-up operations:

- provider and installation identity;
- tenant, workspace, or guild scope where applicable;
- conversation, channel, chat, thread, or reply-chain coordinates;
- message or activity identifier;
- timestamp and provider correlation identifiers when available;
- capability information for update, delete, reply, or reaction operations.

The reference must be safe to persist without containing access tokens, webhook secrets, interaction tokens, or private keys.

### Event envelopes

Shared event metadata may include:

- event identifier and deduplication key;
- provider and installation;
- sender identity and conversation reference;
- message reference;
- event time and receive time;
- normalized event kind;
- correlation identifier;
- typed provider payload.

Normalized event kinds should remain small: message received, app mentioned, command invoked, action invoked, modal submitted, reaction changed, message changed, message deleted, installed, and removed. Provider-specific data remains available through typed payloads.

### Capability interfaces

Prefer focused contracts such as:

- `IMessageSender`
- `IMessageLifecycleClient`
- `IConversationDirectory`
- `IInteractionReceiver`
- `IEventReceiver`
- `IReactionClient`
- `IProviderCapabilities`
- `IMessageCredentialProvider`
- `IMessageStateStore`

Do not require every provider to implement every interface. Capabilities should be discoverable before an operation is attempted.

### Async and cancellation

Reusable C# operations should be asynchronous and accept `CancellationToken`. Network calls, pagination, Gateway or Socket connections, acknowledgement deadlines, retries, and shutdown must propagate cancellation correctly.

PowerShell should expose simple synchronous command behavior to users while the compiled cmdlet safely drives the asynchronous implementation.

## Microsoft Teams roadmap

### Notification delivery

- [x] Support Power Automate Workflow webhook URLs for external notifications.
- [x] Support Workflow destinations configured for channels, group chats, and chats as descriptive target metadata.
- [x] Support plain text fallback and Adaptive Card payloads accepted by the configured Workflow.
- [x] Return a useful delivery result even when the Workflow does not expose a durable Teams message identifier.
- [x] Model Workflow endpoints as send-only capabilities rather than pretending they provide inbound conversation support.
- [x] Add transport options for proxy, timeout, cancellation, a product user agent, safe correlation/retry headers, and redacted diagnostics.

### Adaptive Cards and legacy card surfaces

- [ ] Preserve the useful `New-Adaptive*` PowerShell command family from PSTeams.
- [ ] Make builders return owned typed C# models instead of dictionaries or serializer objects.
- [ ] Cover escaping, underscores, Unicode, mentions, inline images, media, tables, action sets, and nested elements with serialization fixtures.
- [ ] Decide which older Hero Card, Thumbnail Card, Card List, Activity Image, and connector-card surfaces remain supported by current Teams delivery paths.
- [ ] Mark obsolete card families explicitly rather than silently carrying dead protocol behavior.
- [ ] Provide import-from-JSON and export-to-JSON paths for interoperability and troubleshooting without making JSON the primary API.

### Teams app conversations

- [ ] Evaluate the current Teams Outgoing Webhook `@mention`/response path as an optional simple service integration; support it only if its per-team scope and response deadline still solve a real use case better than a Teams app.
- [ ] Implement the minimum Teams bot/agent Activity and Connector protocol required for owned service hosting.
- [ ] Validate inbound bearer tokens and claims before parsing or dispatching activities.
- [ ] Support personal, group-chat, and channel installation scopes where the platform allows them.
- [ ] Support `@mention` handling and removal of the bot mention from command text.
- [ ] Represent resource-specific consent for receiving all channel or chat messages as an explicit optional capability.
- [ ] Receive messages, invokes, card actions, installation events, and relevant message lifecycle events.
- [ ] Reply in the correct channel reply chain or chat conversation.
- [ ] Update and delete activities created by the application where supported.
- [ ] Store `conversationId`, `tenantId`, `serviceUrl`, bot-scoped user identifiers, and activity identifiers needed for proactive delivery.
- [ ] Support proactive personal messages and new channel reply chains only when the app is installed and the required coordinates are available.
- [ ] Report unsupported private/shared-channel behavior clearly instead of claiming general channel parity.

### GraphEssentialsX boundary

- [ ] Prepare GraphEssentialsX as a consumable NuGet package before MessageX publicly depends on it.
- [ ] Keep Microsoft Graph authentication, paging, throttling, typed failures, governed writes, discovery, history, and Teams collaboration lifecycle in GraphEssentialsX.
- [ ] Add `MessageX.Teams.Graph` only as a thin mapping adapter.
- [ ] Do not duplicate a Graph HTTP client inside MessageX.Teams.
- [ ] Do not use Graph application-only migration permissions as a normal service-send mechanism.
- [ ] Keep MessageX.Teams usable without GraphEssentialsX for Workflows and bot/agent conversations.

### Teams completion evidence

- [ ] Live Workflow send to a test channel and test chat.
- [ ] Live personal bot conversation.
- [ ] Live channel mention and threaded reply.
- [ ] Live Adaptive Card action round trip.
- [ ] Proactive send after process restart using a persisted conversation reference.
- [ ] Update and delete of an application-owned activity.
- [ ] Negative validation for wrong tenant, invalid token, missing installation, expired credentials, and unsupported scope.
- [ ] Conservative validation in the product-like tenant and broader create/update/delete validation in the designated MVP test tenant.

## Slack roadmap

### Notification delivery

- [x] Support Slack incoming webhooks as simple fixed-destination senders.
- [x] Support plain text and initial section/divider Block Kit payloads.
- [x] Make webhook limitations explicit, including message lifecycle operations that require Web API credentials.

### Web API messaging

- [x] Implement bot-token calls needed to send to public channels, private channels, direct messages, and multiparty conversations when scopes and membership allow them.
- [x] Support thread replies using `thread_ts` and optional reply broadcast.
- [x] Return channel and timestamp identifiers required for update, delete, reply, and reaction operations.
- [x] Support opening or resolving direct-message conversations for one to eight explicit user identifiers without accepting display names or bulk discovery.
- [x] Implement update and delete for application-owned messages.
- [x] Add and remove reactions through authenticated Web API connections.
- [ ] Add current Slack file-upload workflows after the message lifecycle is stable.
- [ ] Handle pagination and workspace/enterprise identifiers without assuming one installation per process.

### Rich content

- [ ] Add typed builders for common Block Kit blocks, elements, actions, views, and modals.
- [x] Preserve top-level fallback text for accessibility and notifications.
- [x] Validate limits for the implemented message, section, field, identifier, and block contracts before sending.
- [ ] Add a safe provider-native extension model for unsupported new Slack elements without weakening typed validation.

### Inbound events and interactions

- [ ] Verify HTTP requests from the raw body with timestamp replay protection and HMAC-SHA256.
- [ ] Support URL verification, Events API envelopes, retries, and event deduplication.
- [ ] Support app mentions, direct messages, subscribed channel messages, slash commands, shortcuts, buttons, selections, and modal submissions.
- [ ] Acknowledge valid commands and interactions within Slack's deadline, then continue work asynchronously.
- [ ] Support HTTP Events API as the primary production path.
- [ ] Add Socket Mode for local, on-premises, and firewall-constrained services.
- [ ] Implement rotating WebSocket URLs, acknowledgement by envelope ID, reconnect behavior, and overlapping connections where required for uptime.

### Slack completion evidence

- [ ] Live incoming-webhook send.
- [ ] Live bot send to a channel and direct message.
- [ ] Thread reply, update, delete, and reaction round trip.
- [ ] Block Kit button and modal round trip.
- [ ] Events API retry and deduplication proof.
- [ ] Socket Mode reconnect proof.
- [ ] Negative validation for invalid signature, stale timestamp, missing scope, missing membership, revoked installation, and rate limiting.

## Discord roadmap

### Notification delivery

- [x] Support incoming webhooks for channels and thread targets.
- [x] Request and return the created message when lifecycle operations require its identifier.
- [x] Support webhook-authored message retrieval, update, and deletion where the token permits it.

### Bot REST messaging

- [x] Send to guild text channels, direct-message channels, and thread channels.
- [x] Open a one-to-one DM from one explicit user identifier without adding bulk-recipient discovery.
- [x] Do not treat legacy group-DM creation as a supported bot feature.
- [x] Keep reply references and thread channels as distinct concepts.
- [x] Support retrieval, update, delete, reactions, allowed mentions, attachments, and safe nonce/idempotency behavior for the implemented bot REST surface.
- [x] Parse rate-limit buckets and `Retry-After`; never hard-code current provider limits.

### Rich content

- [x] Preserve and modernize useful PSDiscord builder names such as `New-DiscordAuthor`, `New-DiscordFact`, `New-DiscordImage`, and `New-DiscordSection` where they map to current API concepts.
- [ ] Add typed embeds, components, buttons, select menus, application commands, and modals.
- [ ] Validate content, embed, component, attachment, and mention limits before sending.
- [ ] Provide provider-native JSON import/export for forward compatibility.

### HTTP interactions

- [ ] Verify `X-Signature-Ed25519` and `X-Signature-Timestamp` against the raw request body before parsing.
- [x] Keep Bouncy Castle types internal to MessageX.Discord.
- [ ] Respond to endpoint validation pings.
- [ ] Support commands, message/user commands, components, autocomplete, and modal submissions.
- [ ] Send or defer the initial response within the provider deadline and manage the limited follow-up token lifetime explicitly.
- [ ] Support editing and deleting original and follow-up interaction responses.

### Gateway conversations

- [ ] Implement Gateway discovery, identify, heartbeat, sequence tracking, reconnect, resume, and invalid-session handling.
- [ ] Declare only required intents.
- [ ] Treat Message Content as a privileged optional capability and document the content available without it.
- [ ] Receive message, reaction, thread, guild-installation, and interaction events required by supported MessageX features.
- [ ] Support graceful shutdown and durable resume state where it provides real recovery value.
- [ ] Add sharding only after scale requires it.

### Discord completion evidence

- [ ] Live webhook send to a channel and thread.
- [ ] Live bot send to a channel and direct message.
- [ ] Reply, thread, update, delete, reaction, and attachment round trip.
- [ ] Command, component, and modal interaction round trip.
- [ ] Gateway disconnect and resume proof.
- [ ] Negative validation for bad signatures, missing intents, missing permissions, expired interaction tokens, invalid sessions, and rate limiting.

## PowerShell experience

### Command naming

Keep public commands provider-specific:

```powershell
Send-TeamsMessage
Send-SlackMessage
Send-DiscordMessage

New-AdaptiveCard
New-AdaptiveTextBlock
New-AdaptiveAction

New-SlackBlock
New-SlackSection
New-SlackModal

New-DiscordEmbed
New-DiscordButton
New-DiscordComponent
```

Do not require `Send-MessageXMessage -Provider Teams`. A generic send command may be considered later only if it makes a real automation scenario simpler without hiding provider behavior.

### Parameter-set rules

- [x] Give simple webhook/Workflow delivery a small dedicated parameter set.
- [x] Give authenticated app/bot delivery a parameter set based on a typed connection and typed target.
- [x] Accept text, native rich content, and pipeline input without ambiguous binding.
- [x] Avoid parameter sets that expose every provider option at once.
- [x] Reject conflicting destination, authentication, and content combinations during binding or early validation.
- [x] Use provider identifiers rather than display names where names are ambiguous.
- [x] Support cancellation and user interruption.
- [x] Use `ShouldProcess` for sends, updates, deletes, reactions, and other externally visible mutations.
- [x] Keep tokens, webhook URLs, signatures, and secret headers out of verbose output, errors, history-friendly examples, and returned objects.
- [x] Return typed references from lookup commands and make typed mutation results available through `-PassThru`.

### Builders and type exposure

- [x] Builders return owned C# types accepted directly by send cmdlets.
- [ ] Curate PowerShell type accelerators for public models and enums used in parameters, output, or examples.
- [ ] Do not expose every internal or dependency type.
- [x] Generate command help from compiled cmdlet XML documentation and source metadata.
- [x] Keep one function or cmdlet per source file and split builders by provider responsibility.

### Runtime compatibility

- [x] Import the built module in Windows PowerShell 5.1 using `net472` assets.
- [x] Import the built module in supported PowerShell 7 versions using the correct modern assets.
- [x] Run the same command-contract smoke tests in `powershell.exe` and `pwsh.exe`.
- [x] Keep parameter names, output properties, error categories, and observable behavior aligned across hosts.
- [x] Isolate unavoidable runtime-specific loading or API behavior at one packaging/host boundary.
- [ ] Validate Windows, Linux, and macOS PowerShell 7 for provider-neutral network operations.

### User-success examples

For each provider, documentation must include runnable examples for:

- [ ] install and first send;
- [ ] proxy and timeout configuration;
- [ ] plain text and native rich content;
- [ ] channel and direct-message targets where supported;
- [ ] reply/thread behavior;
- [ ] update and delete;
- [ ] file or attachment support when delivered;
- [ ] credential handling without embedding secrets in scripts;
- [ ] common permission and installation failures;
- [ ] starting a service receiver when that provider phase is available.

Examples must use installed packages/modules and public entry points, not local `bin` paths or unpublished compatibility workarounds.

## Service hosting and interaction runtime

### Receive pipeline

```text
Receive raw HTTP, WebSocket, or Gateway envelope
    -> identify installation and provider
    -> verify signature, token, timestamp, and replay window
    -> parse the provider envelope
    -> create a deduplication key
    -> persist or enqueue before the acknowledgement deadline
    -> acknowledge or defer
    -> route asynchronously to an application handler
    -> reply, update, react, or send proactively
    -> persist resulting references and delivery state
```

### Hosting requirements

- [ ] One application can host several installations across tenants, workspaces, and guilds.
- [ ] HTTP routes identify the intended installation without trusting unvalidated payload fields.
- [ ] Signature verification has access to the exact raw request body.
- [ ] Slow application handlers cannot cause provider acknowledgement deadlines to be missed.
- [ ] Handler failures have explicit retry, dead-letter, and operator-diagnostic behavior.
- [ ] Shutdown drains or safely abandons work according to a documented policy.
- [ ] Background connections expose health, reconnect state, last event, last acknowledgement, and rate-limit state.
- [ ] Service logs use correlation identifiers and redact message content and secrets according to configuration.

### Handler model

The hosting layer should support focused registrations such as:

```csharp
router.OnCommand("status", HandleStatusAsync);
router.OnMention(HandleMentionAsync);
router.OnDirectMessage(HandleDirectMessageAsync);
router.OnAction("approve", HandleApprovalAsync);
```

Handlers receive common routing metadata and a typed provider event. A handler should not need to parse raw JSON, validate signatures, construct provider authentication headers, or implement rate limiting.

### Persistence boundary

MessageX.Hosting defines domain storage contracts. DbaClientX owns database providers, connections, migrations, and low-level storage behavior through the optional `MessageX.Persistence.DbaClientX` adapter.

Persist only what is needed:

- installations and provider scope identifiers;
- non-secret conversation references;
- message references needed for lifecycle operations;
- event deduplication and idempotency records;
- scheduled/proactive work and outbox state;
- Gateway or Socket resume coordinates where useful;
- references to externally stored credentials.

Do not store access tokens or webhook secrets in conversation records. Secrets are resolved through `IMessageCredentialProvider` or the consuming application's established secret store.

## Reliability, security, and operations

### HTTP and connection behavior

- [ ] Reuse managed HTTP clients safely without socket exhaustion.
- [ ] Provide proxy support through shared transport configuration.
- [ ] Bound connect, request, acknowledgement, handler, and no-progress timeouts separately where they represent different failures.
- [ ] Respect provider `Retry-After` and rate-limit headers.
- [ ] Add jittered retries only for operations that are safe to repeat.
- [ ] Use provider idempotency or nonce mechanisms where available.
- [ ] Never automatically retry an ambiguous non-idempotent send without a duplicate-delivery policy.
- [ ] Validate configurable endpoints and prevent credentials from being forwarded to unexpected hosts.

### Security

- [ ] Verify inbound authenticity before deserialization and dispatch.
- [ ] Use constant-time comparison for signatures where applicable.
- [ ] Enforce timestamp/replay windows.
- [ ] Validate issuer, audience, tenant, application, key rotation, and token lifetime for Teams endpoints.
- [ ] Keep Discord Ed25519 verification vectors and Slack HMAC vectors as regression tests.
- [ ] Redact secrets, authorization headers, webhook URLs, interaction tokens, and message content from default logs.
- [ ] Support secret rotation without restarting the entire service where practical.
- [ ] Document least-privilege scopes, intents, permissions, installation requirements, and data-access implications per provider.

### Observability

- [ ] Structured logs with provider, installation, operation, correlation ID, outcome, retry count, and latency.
- [ ] Metrics for sends, failures, throttling, acknowledgement latency, queue depth, deduplication, reconnects, and handler duration.
- [ ] Health checks per HTTP receiver and persistent connection.
- [ ] Diagnostic objects and PowerShell errors that preserve provider correlation IDs without leaking secrets.
- [ ] Optional OpenTelemetry integration only after the base diagnostics contract is stable.

## C# consumer success

### General consumer contract

A C# consumer should be able to:

- reference one provider package without pulling the other providers;
- construct or inject a provider client;
- configure authentication, proxy, timeout, retry, and target information through typed options;
- send text or provider-native rich content asynchronously;
- receive a stable result reference;
- use cancellation correctly;
- mock a narrow MessageX boundary or use a supported fake for contract tests;
- upgrade provider packages without adopting the service-hosting stack.

### Dependency injection

- [ ] Provide modern .NET dependency-injection registration without making DI mandatory for `net472` consumers.
- [ ] Support named installations/connections.
- [ ] Validate configuration at service start where the host supports it.
- [ ] Keep secret resolution lazy enough to support rotation and avoid accidental configuration dumps.
- [ ] Avoid service-locator APIs and global mutable clients.

### TestimoX adoption pilot

TestimoX and ADPlayground already own monitoring-specific notification policy. MessageX must not replace incident state, severity, routing, bundling, quiet hours, suppression, escalation, cooldown, queueing, or recovery logic.

- [ ] Keep `ADPlayground.Notifications` as the domain notification owner.
- [ ] Add thin MessageX-backed channel sinks or a small adapter package.
- [ ] Map `NotificationMessage` into a provider-specific MessageX message and target.
- [ ] Use MessageX for provider transport, serialization, authentication, retry classification, and delivery results.
- [ ] Keep TestimoX configuration responsible for routes, severity selection, targets, and secret references.
- [ ] Avoid a generic webhook payload when a supported provider adapter is configured.
- [ ] Validate incident, recovery, aggregate, suppression, queue backpressure, restart, and duplicate-delivery scenarios.
- [ ] Prove package consumption using published or locally packed MessageX artifacts, not repository-relative source assumptions.

### EventViewerX adoption pilot

EventViewerX owns event collection, subscription backpressure, report composition, buffering, and its CLI watch workflow. Mailozaurr remains the email transport owner.

- [ ] Define a narrow EventViewerX notification sink boundary if the existing CLI delivery path cannot host multiple transports cleanly.
- [ ] Add optional MessageX-backed Teams, Slack, and Discord sinks without referencing all providers from the EventViewerX core engine.
- [ ] Map event summaries and report links into provider-native messages.
- [ ] Keep full HTML/email rendering in the existing reporting and Mailozaurr path.
- [ ] Integrate with existing watch buffering and outbox behavior rather than creating a second queue.
- [ ] Validate burst handling, cancellation, outbox replay, provider throttling, restart, and partial multi-target failure.
- [ ] Prove the CLI and library consume packed MessageX packages cleanly.

### Other Evotec consumers

- [ ] Provide one minimal console-service example using MessageX directly.
- [ ] Provide one ASP.NET Core receiver example.
- [ ] Document the adapter pattern for domain-specific notification systems.
- [ ] Add a testing package only after at least two consumers need the same fake, recorder, or fixture support.
- [ ] Keep products such as TestimoX and EventViewerX thin; improvements to shared provider behavior return to MessageX.

## Build, packaging, and release

MessageX should follow the current Mailozaurr-style unified PowerForge/PSPublishModule release model.

### Build shape

- [ ] Use `Build/project.build.json` as the package build source of truth.
- [ ] Use a small `Build/Build-Module.ps1` wrapper with project-specific configuration only.
- [ ] Build NuGet packages before the PowerShell module and provide the local feed to the module build.
- [ ] Use one release version source and coordinated package/module release unless a real independent-versioning need appears.
- [ ] Build package and module artifacts without requiring publish credentials.
- [ ] Keep signing declarative and owned by PowerForge/PSPublishModule.
- [ ] Use public three-part versions; reserve fourth numeric segments for local/private build identities.
- [ ] Generate binary cmdlet documentation from source metadata.
- [ ] Stage NuGet, PSGallery, and GitHub artifacts through one release plan.

### Target matrix

- [ ] Restore, build, and test `net472` on Windows.
- [ ] Restore, build, test, trim-analyze, and AOT-analyze appropriate `net8.0` and `net10.0` projects.
- [ ] Build provider-neutral modern targets on Windows and Linux.
- [ ] Pack each NuGet package and inspect its dependency closure.
- [ ] Install packed packages into clean sample consumers.
- [ ] Build the PowerShell module and import the packed module in Windows PowerShell 5.1 and PowerShell 7.
- [ ] Verify the module chooses the correct binary assets without loading conflicting PowerShell assemblies.

### Release states

Track these independently:

1. local source is implemented and tested;
2. local NuGet packages and module artifacts are packed and validated;
3. repository CI is green for the exact head;
4. packages are published to NuGet and the PowerShell Gallery;
5. clean consumers restore the published versions;
6. TestimoX, EventViewerX, or other consumers adopt the published versions;
7. deployed services use the intended version.

Do not add downstream compatibility probes or duplicate implementations while waiting for a MessageX, GraphEssentialsX, DbaClientX, or PowerForge package publication.

## Testing and validation strategy

### Contract tests

- [ ] Exact serialization fixtures for supported outbound payloads.
- [ ] Exact parsing fixtures for inbound events and interactions.
- [ ] Public target, result, event, error, and capability contracts.
- [ ] Cancellation and timeout behavior.
- [ ] Retry classification and idempotency rules.
- [ ] Redaction and secret-boundary behavior.
- [ ] Compatibility tests for supported provider API versions.

Tests should protect current public behavior and real regressions. They should not merely assert that old PSTeams or PSDiscord files no longer exist.

### Protocol simulators

- [ ] Local HTTP fixtures for status codes, redirects, timeouts, malformed payloads, throttling, and retry headers.
- [ ] Slack signing fixtures over exact raw bodies.
- [ ] Discord Ed25519 verification fixtures over exact timestamp/body bytes.
- [ ] Teams token and Activity fixtures including key rotation and invalid claims.
- [ ] WebSocket/Gateway fixtures for disconnect, reconnect, heartbeat loss, resume, and invalid session.

### Live provider tests

- [ ] Dedicated Teams test app and test locations.
- [ ] Dedicated Slack test workspace and app installation.
- [ ] Dedicated Discord test guild and application.
- [ ] Tests create uniquely named or tagged messages and clean them when supported.
- [ ] Live tests are opt-in and fail closed when the expected tenant, workspace, guild, or app identity does not match.
- [ ] Rate-limit and destructive tests remain bounded and are never run against product-like environments by default.

### Artifact tests

- [ ] Clean NuGet restore from the staged package feed.
- [ ] Clean PowerShell module import from the packed artifact.
- [ ] C# compile/run samples for each provider and target family.
- [ ] PowerShell 5.1 and PowerShell 7 command samples.
- [ ] ASP.NET Core receiver startup, health, acknowledgement, persistence, and restart tests.
- [ ] TestimoX and EventViewerX adapter smoke tests against packed artifacts.

## Documentation and examples

### Documentation layers

- `README.md` explains the product, supported providers, installation, and first successful send.
- `ROADMAP.md` remains the current delivery plan and drops obsolete completed detail over time.
- `Docs/Architecture.md` records stable ownership and dependency boundaries after implementation proves them.
- Generated .NET API and PowerShell command documentation comes from XML docs and cmdlet metadata.
- Provider guides cover credentials, permissions, app installation, targets, conversations, and troubleshooting.
- Contributor documentation covers local source, live-test setup, fixtures, and release gates.

### Required examples

- [ ] Teams Workflow notification.
- [ ] Teams personal and channel bot conversation.
- [ ] Slack webhook and bot channel send.
- [ ] Slack threaded reply and interactive action.
- [ ] Discord webhook and bot send.
- [ ] Discord command/component interaction and Gateway message handler.
- [ ] Multi-provider service routing without cross-provider rich-content conversion.
- [ ] TestimoX-style domain notification adapter.
- [ ] EventViewerX-style streaming/report notification adapter.

Examples must state real limitations and required provider configuration. Do not claim a capability from successful JSON generation alone.

## Existing implementation and issue migration

The PSTeams GitHub repository is the future MessageX repository. Current TeamsX source on the non-legacy `main` branch is the implementation baseline and should be evolved in place. The dirty `legacy` branch and the separate PSDiscord repository are historical/reference migration inputs, not architectural owners for the new implementation.

### Baseline inventory

- [ ] Inventory the current TeamsX `main` projects, exported binary cmdlets, examples, public models, tests, build/package behavior, open issues, and widely used parameter shapes.
- [ ] Classify every current TeamsX public surface as retain, refactor, replace, or remove with migration guidance before renaming projects or namespaces.
- [ ] Preserve validated TeamsX behavior and tests while moving reusable contracts to MessageX packages; do not rewrite working code merely to make the repository look new.
- [ ] Record which behaviors remain valid on current provider APIs.
- [ ] Turn useful behaviors and reported regressions into MessageX contract tests or roadmap work.
- [ ] Replace obsolete legacy transport, serializer, dependency-loading, or build architecture only where the current TeamsX audit demonstrates that cleanup is needed.

### Known PSTeams backlog themes to account for

- [ ] Power Automate Workflow migration and Microsoft 365 connector retirement.
- [ ] Proxy support.
- [ ] Channel root messages and replies.
- [ ] Files and hosted/inline images.
- [ ] Adaptive Card and text escaping, including underscores.
- [ ] Activity images and legacy card image behavior.
- [ ] Complete help and examples.

### Known PSDiscord backlog themes to account for

- [ ] Discord events and interactive service behavior.
- [ ] Missing webhook, message lifecycle, rich-content, and attachment operations.

### Transition

- [ ] Develop MessageX in a fresh isolated worktree and branch created from the fetched current PSTeams `main`; do not use or alter the dirty `legacy` checkout or treat a stale migration worktree as authoritative.
- [ ] Retain the present TeamsX code and tests through reviewed, cohesive moves into `MessageX.Teams` and `MessageX.PowerShell`; avoid a wholesale delete-and-recreate transition.
- [ ] Keep existing commit history, branches, tags, releases, issues, and pull-request records; do not use an orphan branch or force-push a replacement history.
- [ ] Audit repository URLs, badges, documentation links, webhooks, release automation, package metadata, and any `uses: EvotecIT/PSTeams@...` references before renaming.
- [ ] Rename `EvotecIT/PSTeams` to `EvotecIT/MessageX` before the first public MessageX preview and update active clones/remotes to the new URL.
- [ ] Verify old repository URLs and Git operations redirect after the rename.
- [ ] Do not create another `EvotecIT/PSTeams` repository after the rename because it would break the redirect.
- [ ] Treat MessageX NuGet packages and the MessageX PowerShell module as new package identities even though the GitHub repository retains its community history.
- [ ] Publish a MessageX preview before recommending migration.
- [ ] Provide a command-by-command migration table for supported PSTeams and PSDiscord commands.
- [ ] Preserve `Send-TeamsMessage`, `Send-DiscordMessage`, and useful builder names in the MessageX module.
- [ ] Introduce `Send-SlackMessage` and Slack-native builders consistently.
- [ ] Explain intentional breaks, obsolete provider behavior, and replacement examples.
- [ ] Replace the renamed repository README with MessageX documentation when the new default-branch source is ready, while keeping historical releases and migration guidance discoverable.
- [ ] Update the PSDiscord README only after a published MessageX replacement exists.
- [ ] Resolve or transfer legacy issues with a concrete MessageX capability, release, or explicit unsupported decision.
- [ ] Archive or retire PSDiscord only as a separate maintainer decision.

## Delivery phases

### Phase 0A - Clean repository baseline

- [x] Create a fresh dedicated cleanup worktree and branch from fetched `origin/main`; do not develop from the dirty `legacy` checkout, the behind local `main`, or the older TeamsX migration worktree.
- [x] Record the exact starting commit and prove the prior TeamsX migration branch is contained in it.
- [x] Inventory every registered worktree and local/remote task branch by cleanliness, PR state, merge state, ancestry, unique work, and current ownership.
- [x] Remove only clean, proven-stale, task-owned worktrees and merged local branches; preserve the open IntelligenceX reviewer worktree and all uncertain or user-owned state.
- [x] Record the dirty `legacy` files without resetting, stashing, importing, or deleting them. Classify their useful product behavior later as migration input.
- [x] Audit open pull requests, branch protection, release workflows, repository integrations, ignored/generated files, and tracked build artifacts.
- [x] Run the current repository-native restore, Release build, .NET tests, PowerShell 7 tests, Windows PowerShell 5.1 tests, module build, and package build before source cleanup.
- [x] Record warnings, failures, package contents, generated-document state, and environment limitations as the reproducible pre-cleanup baseline.
- [x] Keep repository/worktree hygiene separate from product refactoring so stale operational state is not mixed into the first code change.

**Exit:** one clean, current, isolated `origin/main` worktree is the sole MessageX starting point; stale task-owned worktrees are handled, user-owned dirty state is untouched, and the unmodified TeamsX build/test/package baseline is recorded.

#### Recorded Phase 0A baseline

- Starting commit: `f712d1c4c498cb1514f63daf80601bdecf796377` from `origin/main` on branch `refactor/teamsx-stabilization`.
- Removed three clean task worktrees whose PRs were merged: #64, #66, and #67. Preserved the dirty `legacy` checkout and the clean worktree for open PR #65.
- Release solution build: zero warnings and zero errors. Tests: 31/31 on .NET 8, 31/31 on .NET 10, 36/36 on PowerShell 7.6.4, and 36/36 on Windows PowerShell 5.1.
- The TeamsX package was created with `netstandard2.0`, `net472`, `net8.0`, and `net10.0` assemblies and XML documentation, but package verification failed because it was unsigned. The configured code-signing certificate was not present on this machine (`NU3001`).
- The PowerShell module build staged its framework assets and then stopped making progress while signing was enabled. The run was terminated after the idle state was verified; its task-created staging output was moved to the Recycle Bin.
- The repository has no `main` branch protection, one unrelated open PR (#65), no tracked build outputs, no vulnerable direct/transitive NuGet packages, and no deprecated production packages.
- The test project currently uses deprecated xUnit v2 and has available test-tool updates. Dependency modernization belongs to Phase 0B.
- The repository has no SDK pin, so the local build selected an installed .NET 10 preview SDK despite stable .NET 10 SDKs being available. SDK selection belongs to Phase 0B.
- Generated PowerShell command documentation contains many unfilled descriptions and examples. Documentation must be fixed at cmdlet/XML metadata sources and regenerated, not patched by hand.

### Phase 0B - TeamsX cleanup and stabilization

- [x] Audit TeamsX types, cmdlets, tests, examples, Workflow and Graph boundaries, package contents, and generated documentation before changing names or locations.
- [x] Classify existing source, public APIs, cmdlets, tests, examples, build logic, and documentation as retain, refactor, replace, regenerate, or remove with migration guidance.
- [x] Remove dead code, obsolete compatibility paths, temporary migration scaffolding, stale plans, duplicate helpers, placeholder documentation, and accidental generated artifacts where evidence proves they are no longer needed.
- [x] Fix current warnings, flaky or low-signal tests, inconsistent cancellation/error behavior, serializer leakage, dependency drift, and packaging defects before adding another provider.
- [x] Keep tests that protect current public behavior, PowerShell 5.1/7 compatibility, Adaptive Card serialization, Workflow delivery, package layout, and known regressions; remove tests that only preserve obsolete implementation shape.
- [x] Move reusable build/package behavior into PowerForge/PSPublishModule and keep the repository wrapper declarative and small.
- [x] Confirm GraphEssentialsX owns authenticated Graph lifecycle and governance while TeamsX owns Teams composition and delivery mapping.
- [x] Produce a clean TeamsX release candidate under its current names and prove restore, build, tests, module assembly, package contents, and clean-consumer loading again.
- [x] Do not begin the MessageX namespace/project rename, Slack, Discord, hosting, or new public abstractions until this cleanup gate passes.

**Exit:** current TeamsX behavior is reproducible from a clean checkout; source, tests, build, packages, and docs agree; known cleanup blockers are resolved or explicitly separated; and the implementation is ready for intentional MessageX restructuring.

#### Recorded Phase 0B candidate evidence

- `TeamsX` and `TeamsX.PowerShell` build without warnings for `net472`, `netstandard2.0`, `net8.0`, and `net10.0` on Windows.
- The Microsoft Testing Platform suite passes 26/26 on both .NET 8 and .NET 10.
- The source-tree module suite passes 38/38 on PowerShell 7.6.4 and 38/38 on Windows PowerShell 5.1.
- The exact PowerForge-packed module passes 38/38 on PowerShell 7.6.4 while loading `Lib/Core-net10.0/TeamsX.PowerShell.dll`, and 38/38 on Windows PowerShell 5.1 while loading `Lib/Default/TeamsX.PowerShell.dll`.
- Generated help has 63 command pages and 63 MAML commands, uses readable fallback descriptions in both formats, contains no parameter-description placeholders, and is guarded against missing or stale command pages.
- Authenticated Microsoft Graph lifecycle and governed Teams chat/channel writes are removed from TeamsX and remain owned by GraphEssentialsX. Incoming and Workflow webhook composition/delivery stay in TeamsX.
- Webhook delivery revalidates absolute HTTPS at the transport boundary, default clients do not follow redirects, delivery results do not expose webhook secrets, and verbose output does not expose message or provider-response bodies.
- Legacy dictionary-shaped ShowCard content now fails with migration guidance instead of silently dropping nested fields; typed nested cards serialize Adaptive Card image alternative text as `altText`.
- Dependency management, stable SDK selection, MTP execution, documentation generation, manifest refresh, multi-runtime packing, and the small consumer build wrapper now follow PowerForge/PSPublishModule ownership.

### Phase 0C - MessageX architecture baseline

- [x] Prepare the MessageX source-tree baseline by evolving stabilized TeamsX projects without rewriting repository history or discarding validated behavior.
- [x] Plan the repository rename and default-branch landing so repository metadata, links, workflows, and documentation change together.
- [x] Verify the intended package/module identifiers are available; reserve them through the first explicitly authorized preview publication rather than publishing empty placeholders.
- [x] Evolve the stabilized TeamsX solution, shared build properties, dependency management, analyzers, formatting, and test projects into the MessageX layout; add only missing infrastructure.
- [x] Add the PowerForge/PSPublishModule coordinated release configuration required by the new package family.
- [x] Capture current PSTeams and PSDiscord public-surface and issue inventories for explicit migration decisions.
- [x] Confirm the exact GraphEssentialsX adapter boundary in the new package layout.
- [x] Use the shared repository version for coordinated packages, with `MessageX.Core` as the primary project in release configuration.

**Exit:** clean restore/build/pack skeleton for the renamed package family, with retained Teams behavior proven and no empty speculative provider packages published.

#### Recorded Phase 0C candidate evidence

- `Sources/MessageX.slnx` contains real `MessageX.Core`, `MessageX.Teams`, `MessageX.PowerShell`, and `MessageX.Tests` projects; no empty Slack, Discord, hosting, Graph, persistence, or aggregate projects were introduced.
- `MessageX.Core`, `MessageX.Teams`, and `MessageX.PowerShell` build without warnings for `net472`, `net8.0`, and `net10.0` on Windows.
- The provider-neutral core owns message references, capability flags, classified errors, delivery-result state, and the focused generic sender contract. Teams implements that sender/result boundary while retaining its provider-native models.
- The Microsoft Testing Platform suite passes 32/32 on .NET 8 and .NET 10, including capability discovery through the provider-neutral interface and fail-closed unsupported targets. The retained PSTeams surface passes 38/38 on PowerShell 7 and 38/38 on Windows PowerShell 5.1 while loading `MessageX.PowerShell` from the source tree.
- Local `MessageX.Core` and `MessageX.Teams` 0.1.0 packages contain the expected three target frameworks and package readme. `MessageX.Teams` depends only on `MessageX.Core`, plus `System.Text.Json` on .NET Framework.
- A clean .NET 10 sample restored `MessageX.Teams` from the staged feed, received `MessageX.Core` transitively, compiled against both namespaces, and ran successfully.
- [Legacy-Issue-Migration.md](Docs/Legacy-Issue-Migration.md) maps every currently open PSTeams and PSDiscord issue to an implementation phase and concrete closure evidence.
- GraphEssentialsX remains outside the initial dependency graph; a future `MessageX.Teams.Graph` project is introduced only with its first real adapter capability.

### Phase 1 - Teams notification vertical slice

- [x] Extract or refine minimal core results, errors, and transport options while retaining the current Teams Workflow target as the first provider vertical slice.
- [x] Retain and modernize existing typed Adaptive Card composition required by real examples.
- [x] Preserve and refine the existing `Send-TeamsMessage` and `New-Adaptive*` commands with deliberate parameter-set and compatibility decisions.
- [x] Add proxy, timeout, cancellation, safe diagnostics, and serialization fixtures.
- [x] Complete exact package/module artifact tests for this slice.
- [ ] Complete live Workflow proof for a test channel and chat.

#### Recorded Phase 1 candidate evidence

- `MessageX.Core`, `MessageX.Teams`, and `MessageX.PowerShell` build without warnings for `net472`, `net8.0`, and `net10.0`.
- The Microsoft Testing Platform suite passes 40/40 on .NET 8 and .NET 10. It covers Workflow destination metadata, send-only capability discovery, proxy/timeout/user-agent configuration, bounded safe correlation and retry headers, fully redacted exception chains, and the distinction between transport timeouts and caller cancellation.
- The source module passes 41/41 on PowerShell 7 and Windows PowerShell 5.1. Every webhook-capable retained command exposes the same enterprise transport parameters, response bodies stay out of default delivery errors, and pipeline records reuse one lifecycle-scoped client.
- The exact PowerForge-packed PSTeams 2.4.1 artifact passes 41/41 on PowerShell 7 while loading `MessageX.Core`, `MessageX.Teams`, and `MessageX.PowerShell` from `Lib/Core-net10.0`, and 41/41 on Windows PowerShell 5.1 while loading the same assemblies from `Lib/Default`.
- Live Workflow delivery remains intentionally pending until the configured test channel and chat URLs are supplied during the authorized provider-validation phase.

**Exit:** C# and PowerShell users can install a packed artifact and send text and Adaptive Cards through a current Teams Workflow.

### Phase 2 - Slack notification vertical slice

- [x] Implement incoming webhook and bot-token channel/direct-message sending.
- [x] Add initial Block Kit builders and `Send-SlackMessage`.
- [x] Return durable message references for authenticated sends.
- [ ] Add live send proof in the authorized Slack workspace; error, rate-limit, package, and dual-runtime PowerShell proof are part of the candidate gate.
- [x] Revisit core HTTP transport and safe diagnostic-token contracts and remove Teams-only ownership.

#### Recorded Phase 2 candidate evidence

- `MessageX.Slack` uses an owned `System.Text.Json` protocol surface and builds with `MessageX.Core` and `MessageX.PowerShell` for `net472`, `net8.0`, and `net10.0` without SlackNet, Discord libraries, or Newtonsoft.Json.
- Incoming-webhook and authenticated `chat.postMessage` senders cover safe targets, bearer authentication, Block Kit section/divider payloads, thread replies, retry classification, sanitized failures, and durable Slack channel/timestamp references.
- PowerShell exposes simple and typed parameter sets through `Send-SlackMessage`, secure bot connections, webhook and conversation targets, Block Kit builders, and exact JSON preview while retaining shared proxy, timeout, user-agent, cancellation, and `ShouldProcess` behavior.
- The committed candidate passes 76 .NET contracts on both .NET 8 and .NET 10 (152 target-framework executions per operating system) on Windows and Linux, plus 49 packed-artifact PowerShell contracts on both PowerShell 7 and Windows PowerShell 5.1. Standalone applications restored the packed `MessageX.Slack` and `MessageX.Core` NuGet artifacts from an isolated feed and executed the Slack composition API on .NET Framework 4.7.2, .NET 8, and .NET 10.
- Live Slack webhook, channel, direct-message, and thread sends remain intentionally pending for the final authorized provider-validation phase.

**Exit:** Slack webhook and bot sends work from C#, PowerShell 5.1, and PowerShell 7 without pulling Teams implementation details into Slack.

### Phase 3 - Discord notification vertical slice

- [x] Implement webhook and bot REST sends.
- [x] Implement channels, DMs, replies, threads, embeds, allowed mentions, and initial attachments.
- [x] Implement `Send-DiscordMessage` and current Discord builders.
- [ ] Add live send proof in the authorized Discord environment; rate-limit, Ed25519, package, and dual-runtime PowerShell proof are part of the candidate gate.
- [x] Revisit core contracts after all three providers are proven.

#### Recorded Phase 3 candidate evidence

- `MessageX.Discord` owns its `System.Text.Json` protocol surface and targets `net472`, `net8.0`, and `net10.0` without Discord.Net, NetCord, or Newtonsoft.Json. Bouncy Castle 2.7.0 supplies Ed25519 internally without leaking provider types into public APIs.
- Incoming webhooks force `wait=true` so acceptance includes durable message coordinates. Bot REST covers channels, semantic thread targets, one-to-one DM channel creation, replies, safe-by-default mentions, embeds, multipart attachments, nonce enforcement, rate-limit metadata, bounded streamed responses, and timeout/cancellation behavior.
- PowerShell exposes typed and simple parameter sets through `Send-DiscordMessage`, secure bot connections, distinct webhook/channel/thread/DM targets, exact JSON preview, attachment and embed builders, legacy PSDiscord builder aliases, and interaction signature verification.
- The candidate builds without warnings across all three production target frameworks and passes 103 .NET contracts on both .NET 8 and .NET 10. The exact PowerForge-packed PSTeams artifact passes 56/56 module contracts on PowerShell 7 while loading `MessageX.Discord` and `MessageX.PowerShell` from `Lib/Core-net10.0`, and 56/56 on Windows PowerShell 5.1 while loading them from `Lib/Default`.
- Live Discord webhook, channel, thread, reply, and direct-message sends remain intentionally pending for the final authorized provider-validation phase.

**Exit:** Discord webhook and bot sends work from C# and PowerShell, and shared contracts reflect three real providers rather than one provider generalized early.

### Phase 4 - Message lifecycle and conversation addressing

- [x] Add focused reply, read, update, delete, reaction, and conversation-directory capabilities in the order supported by real provider use cases.
- [x] Add typed provider targets and durable message/conversation references that never persist access tokens or webhook secrets.
- [x] Expose provider-specific PowerShell lifecycle commands with explicit bot/webhook parameter sets, `ShouldProcess`, cancellation, proxy, timeout, and typed output behavior.
- [x] Validate the exact PowerForge-packed module on PowerShell 7 and Windows PowerShell 5.1 with the correct runtime payload selected on each host.
- [ ] Add Slack file-upload lifecycle behavior after its current provider workflow and retention contracts are modeled.
- [ ] Complete authorized live Slack and Discord lifecycle round trips during the final provider-validation phase.

Current Phase 4 source and packed-artifact contracts are complete. Live provider mutation remains deliberately deferred until the final authorized validation phase.
- [ ] Define capability discovery and unsupported-operation behavior.
- [ ] Complete provider-native rich-content validation and JSON interoperability.
- [ ] Expand PowerShell parameter sets without creating generic provider switches.

**Exit:** applications can safely continue a conversation and manage application-owned messages using persisted references.

### Phase 5 - Inbound interactions and service hosting

- [x] Implement MessageX.Hosting routing and handler contracts.
- [ ] Implement ASP.NET Core raw-body receivers, verification, acknowledgement, queueing, and health checks.
- [ ] Deliver Teams bot/agent events, Slack Events API/interactions, and Discord HTTP interactions.
- [ ] Add DbaClientX persistence for installations, deduplication, references, and outbox state.
- [ ] Add restart, replay, duplicate, failure, and multi-installation tests.

**Exit:** one service can securely host interactive applications for Teams, Slack, and Discord over HTTP.

### Phase 6 - Persistent realtime conversations

- [ ] Implement Slack Socket Mode.
- [ ] Implement Discord Gateway connection, intents, resume, and health.
- [ ] Complete proactive Teams conversation delivery.
- [ ] Add reconnect, resume, scale, graceful shutdown, and long-running soak tests.

**Exit:** services can participate in ordinary supported conversations and recover persistent connections without losing or duplicating acknowledged work.

### Phase 7 - Evotec consumer pilots

- [ ] Add the TestimoX/ADPlayground notification adapter and validate monitoring policy remains in the consumer.
- [ ] Add the EventViewerX notification adapter and validate watch buffering/backpressure remains in the consumer.
- [ ] Run package-consumer, service restart, throttling, cancellation, and partial-failure tests.
- [ ] Move any duplicated provider behavior discovered by pilots back into MessageX.
- [ ] Add shared test support only for needs proven by both pilots.

**Exit:** two materially different Evotec products consume packed MessageX packages without local provider protocol implementations.

### Phase 8 - Preview and stable release

- [ ] Complete public API review and intentional breaking cleanup.
- [ ] Complete dependency, security, trim/AOT, package-content, documentation, and example audits.
- [ ] Rename `EvotecIT/PSTeams` to `EvotecIT/MessageX`, update active references, and verify redirects before publishing the first public MessageX preview.
- [ ] Publish coordinated preview packages and module.
- [ ] Validate clean public-feed restores and PowerShell Gallery installation.
- [ ] Collect migration feedback and address validated in-scope issues.
- [ ] Publish `1.0.0` only after provider support claims match live and artifact evidence.
- [ ] Publish PSTeams-to-MessageX migration guidance in the renamed repository and update PSDiscord with its published replacement path.

**Exit:** stable public packages and module are usable without source checkouts, unpublished dependencies, or undocumented provider setup.

## Future provider gate

Telegram and WhatsApp should be evaluated only after MessageX reaches a stable three-provider architecture and at least two external consumers prove the extension model.

Before adding another provider, document:

- official API and supported automation model;
- bot, business, phone-number, tenant, or application onboarding requirements;
- public channel, group, direct-message, and thread semantics;
- outbound, inbound, interaction, update, delete, reaction, and attachment capabilities;
- webhook, polling, or persistent-connection transport;
- authentication, signature verification, token rotation, and secret-storage requirements;
- templates, approval, opt-in, anti-spam, retention, and business-policy constraints;
- rate limits, retries, idempotency, and delivery receipts;
- availability of a safe live test environment;
- target-framework compatibility and dependency cost;
- what belongs in shared MessageX contracts and what must remain provider-native.

Add a provider only when it can deliver one end-to-end vertical slice in C#, PowerShell, artifact tests, and a live environment. Do not add placeholder packages or generic abstractions in anticipation of possible future support.

## Open design decisions

Resolve these through the delivery phases rather than speculation:

- [ ] Final package version source and whether a minimal `MessageX` package is useful or unnecessary.
- [ ] Exact public connection and credential-provider model across simple scripts, DI services, and multi-installation hosts.
- [ ] Which legacy Teams card types remain relevant after Workflow and bot/agent validation.
- [ ] Whether provider-specific persistence extensions are needed beyond the shared state-store contract.
- [ ] Whether a public `MessageX.Testing` package is justified by consumer demand.
- [ ] Minimum Slack Socket Mode and Discord Gateway scale guarantees for `1.0.0`.
- [ ] Timing and packaging of the optional GraphEssentialsX adapter.
- [ ] Final supported PowerShell 7 and operating-system matrix at each release.

## Definition of done for a capability

A MessageX capability is complete only when:

- [ ] the provider behavior and limitations are documented;
- [ ] the C# API is typed, asynchronous, cancellable, and does not leak implementation dependencies;
- [ ] the PowerShell surface is thin, has clean parameter sets, returns useful output, and works in supported hosts;
- [ ] serialization, parsing, validation, failure, retry, throttling, redaction, and cancellation contracts are tested;
- [ ] a live provider path is exercised safely;
- [ ] packed NuGet and PowerShell artifacts are installed and exercised in clean consumers;
- [ ] relevant generated API and command documentation is refreshed from source;
- [ ] downstream consumers use the published or explicitly staged package rather than a duplicate local implementation;
- [ ] local source, packed artifact, public package, consumer adoption, and deployed runtime states are reported separately.

## Primary references

- [Microsoft Teams Workflows webhooks](https://support.microsoft.com/en-us/workflows/send-messages-in-teams-using-incoming-webhooks)
- [Microsoft Teams proactive messages](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages)
- [Microsoft Teams channel and group conversations](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-and-group-conversations)
- [Microsoft Graph send chatMessage permissions](https://learn.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-1.0)
- [Slack chat.postMessage](https://docs.slack.dev/reference/methods/chat.postMessage/)
- [Slack Events API](https://docs.slack.dev/apis/events-api/)
- [Slack Socket Mode](https://docs.slack.dev/apis/events-api/using-socket-mode/)
- [Slack request verification](https://docs.slack.dev/authentication/verifying-requests-from-slack/)
- [Discord message resource](https://docs.discord.com/developers/resources/message)
- [Discord interactions](https://docs.discord.com/developers/interactions/receiving-and-responding)
- [Discord Gateway](https://docs.discord.com/developers/events/gateway)
- [Discord rate limits](https://docs.discord.com/developers/topics/rate-limits)
- [PSTeams issues](https://github.com/EvotecIT/PSTeams/issues)
- [PSDiscord issues](https://github.com/EvotecIT/PSDiscord/issues)
