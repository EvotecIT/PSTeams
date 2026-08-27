# MessageX roadmap

This roadmap tracks the release candidate, not the history of the migration. Completed migration notes belong in Git and pull requests rather than an ever-growing checklist.

## Release objective

Produce an unpublished, reproducible MessageX package set and PSTeams binary module that can be consumed from a clean environment. Publication and product adoption happen only after that candidate passes the gates below.

## Implemented candidate

- [x] Provider-neutral delivery results, references, capabilities, error classification, HTTP configuration, and bounded provider data.
- [x] Teams Workflow/incoming-webhook delivery and typed Adaptive Card composition.
- [x] Typed Teams Universal Action and refresh models with explicit rejection on webhook-only delivery.
- [x] Verified Teams app HTTP activities and Adaptive Card action dispatch through durable hosting contracts.
- [x] Slack incoming-webhook and bot Web API send, reply, update, delete, reactions, and conversation resolution.
- [x] Slack Block Kit sections, dividers, headers, context, actions, buttons, modal inputs, and `views.open`.
- [x] Slack external file upload through `files.getUploadURLExternal` and `files.completeUploadExternal`.
- [x] Verified Slack Events API, commands, block actions, shortcuts, and view submissions.
- [x] Safe transient Slack response URL handling that is excluded from durable storage.
- [x] Discord incoming-webhook and bot REST send, replies, reads, updates, deletes, reactions, embeds, and attachments.
- [x] Discord buttons, string selects, modal text inputs, commands, components, autocomplete, and modal submissions.
- [x] Discord interaction follow-up, original-response update/delete, and follow-up lifecycle with explicit token expiry.
- [x] Shared routing, acknowledgement deadlines, bounded synchronous dispatch, deduplication, retries, dead-letter behavior, health state, and DbaClientX persistence.
- [x] Compiled PowerShell cmdlets and provider-native builders over the C# libraries.
- [x] .NET Framework 4.7.2, .NET 8, and .NET 10 build coverage where supported by each project.

## Current release-candidate checklist

- [x] Remove superseded MessageX branches and worktrees after verifying ancestry and clean state.
- [x] Remove the exposed test Workflow URL from the protected legacy checkout; rotate or revoke it in Microsoft 365 before reuse.
- [x] Build the complete solution in Release with warnings treated as errors.
- [x] Run the complete contract suite on .NET 8 and .NET 10.
- [x] Replace stale generated module exports and command documentation from the build source of truth.
- [x] Build all NuGet packages into an isolated staging directory with publication disabled.
- [x] Inspect package IDs, versions, dependencies, target frameworks, symbols, repository metadata, licenses, and contents.
- [x] Build the PSTeams module from staged binaries with publication and signing disabled.
- [x] Import the staged module in Windows PowerShell 5.1 and PowerShell 7, then exercise representative Teams, Slack, and Discord commands.
- [x] Restore and run clean C# consumers using only the staged NuGet feed.
- [x] Run Linux build/test and clean-consumer checks for provider-neutral and HTTP behavior.
- [x] Run trim/AOT analysis and document that provider serialization is not trim/Native AOT compatible until it moves to source-generated JSON metadata.
- [x] Complete security, public API, package-content, and dependency review.
- [x] Complete one independent read-only local review of the frozen candidate and address validated findings.
- [x] Open a release-ready pull request.
- [ ] Settle exact-head CI and review feedback.

## Optional live proof before publication

Live tests run only when the expected test installation and credentials are available. Missing credentials are a disclosed proof gap, not a reason to weaken local contracts.

- [ ] Teams Workflow notification with a webhook-supported Adaptive Card in the designated test tenant.
- [ ] Slack bot send, lifecycle, external file upload, button response, and modal open in the designated test workspace.
- [ ] Discord bot send, attachment, component/modal interaction, follow-up, edit, and delete in the designated test guild.
- [ ] Verify negative authentication, scope/permission, expiry, replay, and rate-limit behavior without exposing response bodies or secrets.
- [ ] Clean all test messages, files, views, and other artifacts that the provider allows us to remove.

## Publication gate

Publication is a separate authorized step. Before publishing:

- [ ] Choose final public package IDs and confirm whether the PowerShell module remains `PSTeams` or gains a separate `MessageX` identity.
- [ ] Confirm repository identity; do not rename the GitHub repository as an incidental build change.
- [ ] Choose the preview version and use three-part public versions.
- [ ] Enable signing only with the intended release certificate and verify signed artifacts.
- [ ] Publish NuGet packages in dependency order and verify them from the public feed.
- [ ] Publish the PowerShell module only after it restores against the public dependency set.
- [ ] Create and verify tags/releases against the exact accepted commit.

## Product adoption after package proof

TestimoX and EventViewerX must consume staged or public packages, never repository-relative source shortcuts.

- [ ] Add a TestimoX notification adapter that keeps `ADPlayground.Notifications` as the domain owner and MessageX as transport.
- [ ] Validate incident, recovery, aggregation, suppression, restart, backpressure, and duplicate-delivery behavior.
- [ ] Add optional EventViewerX provider sinks without pulling every provider into its core engine.
- [ ] Reuse EventViewerX buffering/outbox behavior and keep HTML/email reporting in its existing owner.
- [ ] Validate burst handling, cancellation, throttling, restart, and partial multi-target failure.

These pilots may be developed against the staged feed before public publication, but they are not part of this repository's release-candidate PR.

## Deferred platform work

The first package candidate intentionally uses verified HTTP receive paths. These items require their own transport lifecycle and operational evidence:

- [ ] Slack Socket Mode: rotating URLs, envelope acknowledgement, reconnect overlap, health, and backpressure.
- [ ] Discord Gateway: discovery, intents, identify, heartbeat, sequence tracking, resume, invalid sessions, and graceful shutdown.
- [ ] Teams bot-owned outbound delivery for Universal Actions, refresh, and proactive messages: trusted persisted service URL and conversation coordinates, installation lifecycle, restart proof, and update/delete ownership.
- [ ] Multi-tenant Slack OAuth installation and token-rotation service contracts.
- [ ] Additional Block Kit and Discord Components V2 elements driven by real consumers.
- [ ] A thin `MessageX.Teams.Graph` adapter after GraphEssentialsX is packaged and a consumer needs it.
- [ ] Source-generated provider JSON contexts plus runnable trimmed and Native AOT consumers before setting `IsTrimmable` or `IsAotCompatible`.

Realtime transports are optional packages or opt-in host features. They must not become hidden background behavior in the basic notification clients.

## Definition of done

The candidate is ready to request publication only when the staged NuGet and PowerShell artifacts are reproducible, clean consumers pass, the supported runtime matrix is truthful, security/API/package review is settled, exact-head CI is green, and every remaining gap above is either completed or explicitly deferred without contradicting public documentation.
