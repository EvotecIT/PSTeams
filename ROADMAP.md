# MessageX roadmap

This roadmap tracks the release candidate, not the history of the migration. Completed migration notes belong in Git and pull requests rather than an ever-growing checklist.

## Release objective

Produce an unpublished, reproducible MessageX package set and PSTeams binary module that can be consumed from clean environments and proven against the supported providers. No NuGet package, PowerShell module, tag, or GitHub release is published until every mandatory pre-publication gate below passes.

## Implemented candidate

- Provider-neutral delivery results, references, capabilities, error classification, HTTP configuration, and bounded provider data.
- Teams Workflow/incoming-webhook delivery and typed Adaptive Card composition.
- Typed Teams Universal Action and refresh models with explicit rejection on webhook-only delivery.
- Verified Teams app HTTP activities and Adaptive Card action dispatch through durable hosting contracts.
- Slack incoming-webhook and bot Web API send, reply, update, delete, reactions, and conversation resolution.
- Slack Block Kit sections, dividers, headers, context, actions, buttons, modal inputs, and `views.open`.
- Slack external file upload through `files.getUploadURLExternal` and `files.completeUploadExternal`.
- Verified Slack Events API, commands, block actions, shortcuts, and view submissions.
- Safe transient Slack response URL handling that is excluded from durable storage.
- Discord incoming-webhook and bot REST send, replies, reads, updates, deletes, reactions, embeds, and attachments.
- Discord buttons, string selects, modal text inputs, commands, components, autocomplete, and modal submissions.
- Discord interaction follow-up, original-response update/delete, and follow-up lifecycle with explicit token expiry.
- Shared routing, acknowledgement deadlines, bounded synchronous dispatch, deduplication, retries, dead-letter behavior, health state, and DbaClientX persistence.
- Compiled PowerShell cmdlets and provider-native builders over the C# libraries.
- .NET Framework 4.7.2, .NET 8, and .NET 10 build coverage where supported by each project.

## Verified candidate baseline

- The provider libraries, hosting packages, persistence adapter, and compiled PowerShell surface are merged on `main` with exact-head CI and review feedback settled.
- Release builds and the complete contract suites pass on the supported Windows, Linux, .NET, and PowerShell lanes.
- All NuGet packages and the PSTeams module have been built into isolated staging locations with publication and signing disabled.
- Package metadata, contents, dependencies, generated command documentation, clean C# consumers, and representative PowerShell commands have been validated from staged artifacts.
- Security, public API, dependency, package-content, and independent code reviews are complete for the merged candidate.
- Trim/Native AOT analysis is documented as a supported-boundary limitation until provider serialization uses source-generated JSON metadata.

## Freeze the signed verification candidate

Complete the release decisions and freeze the exact candidate before mandatory verification begins. Every subsequent gate applies to this signed candidate, not an earlier staging build.

- [ ] Choose final public package IDs and confirm whether the PowerShell module remains `PSTeams` or gains a separate `MessageX` identity.
- [ ] Confirm repository identity; do not rename the GitHub repository as an incidental build change.
- [ ] Choose the preview version and use three-part public versions.
- [ ] Freeze one accepted exact commit for every package, module, downstream-pilot, tag, and release artifact.
- [ ] Rebuild the complete NuGet and PowerShell candidate from that commit using the intended public three-part PowerForge/PSPublishModule version.
- [ ] Enable signing only with the intended release certificate; verify signatures, package contents, repository metadata, and SHA-256 hashes.

Any source, dependency, build, signing, package-content, or deployment-configuration change that can affect the candidate invalidates the verification evidence. Freeze and sign the replacement candidate, then rerun every mandatory live, clean-consumer, and downstream verification gate.

## Mandatory live verification before publication

These checks are release blockers and must exercise the exact signed verification candidate. Run them only against the designated test installations with credentials supplied through approved secret storage. Missing credentials or an unavailable test environment keeps the candidate unpublished; it does not turn live verification into an optional gate.

- [ ] Teams Workflow notification with a webhook-supported Adaptive Card in the designated test tenant.
- [ ] Authenticated Teams app HTTP activity and Adaptive Card action through the real test installation, endpoint route, request verification, routing, and action dispatch path.
- [ ] Slack bot send, lifecycle, external file upload, button response, and modal open in the designated test workspace.
- [ ] Discord bot send, attachment, component/modal interaction, follow-up, edit, and delete in the designated test guild.
- [ ] Verify negative authentication, scope/permission, expiry, replay, and rate-limit behavior without exposing response bodies or secrets.
- [ ] Clean all test messages, files, views, and other artifacts that the provider allows us to remove.

## Mandatory downstream verification before publication

TestimoX and EventViewerX must consume the signed verification packages from the staged feed, never repository-relative source shortcuts. These pilots prove that the public boundaries work for real Evotec consumers before the first release.

- [ ] Add a TestimoX notification adapter that keeps `ADPlayground.Notifications` as the domain owner and MessageX as transport.
- [ ] Validate incident, recovery, aggregation, suppression, restart, backpressure, and duplicate-delivery behavior.
- [ ] Add optional EventViewerX provider sinks without pulling every provider into its core engine.
- [ ] Reuse EventViewerX buffering/outbox behavior and keep HTML/email reporting in its existing owner.
- [ ] Validate burst handling, cancellation, throttling, restart, and partial multi-target failure.
- [ ] Rebuild both pilots from a clean environment using only the final staged package feed.

The public package references replace the staged feed only after the coordinated release completes. The pilots are then rebuilt once more against the public packages as release verification.

## Publication authorization

Publication remains a separate, explicitly authorized operation. Request that authorization only after the exact signed candidate passes every mandatory gate above.

- [ ] Confirm the recorded verification commit matches every signed staged artifact and completed mandatory gate.
- [ ] Confirm NuGet, PowerShell Gallery, and GitHub credentials and produce a dry-run publication plan without uploading anything.
- [ ] Obtain explicit authorization for this exact commit, version set, artifact manifest, and coordinated release operation.

## Coordinated publication

The MessageX NuGet packages, rebuilt PSTeams module, exact-commit tag, and GitHub release are one release unit. Dependency order is internal sequencing, not permission to leave a partial release as the supported state. Do not begin this section until every candidate-freeze, mandatory-verification, and authorization item above is complete.

- [ ] Reconfirm the accepted commit, artifact manifest, signatures, hashes, feed availability, credentials, and explicit release authorization immediately before upload.
- [ ] Publish the complete MessageX NuGet set in dependency order and verify every package from NuGet.org during the same guarded release operation.
- [ ] Publish the matching PSTeams module to PowerShell Gallery in that release operation and verify clean installs in Windows PowerShell 5.1 and PowerShell 7.
- [ ] Create the exact-commit tag and GitHub release with generated release notes and the verified release artifacts.
- [ ] Rebuild the clean C# consumers and downstream pilots against only the public packages.
- [ ] Record the final versions, commit, package hashes, signatures, feed URLs, and verification results in the GitHub release.

If any upload or public verification fails, stop the release operation, record the partial public state, and resolve it before announcing the release. NuGet and PowerShell Gallery publication is not transactionally atomic, so the guarded operation and post-upload verification are the fail-closed boundary.

GitHub Releases and generated release notes are the release-history source of truth. This repository does not maintain a duplicate changelog file.

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

The candidate is ready to request publication only when the staged NuGet and PowerShell artifacts are reproducible and signed, clean consumers and downstream pilots pass, mandatory live-provider verification passes, the supported runtime matrix is truthful, security/API/package review is settled, exact-head CI is green, and every first-preview gap is complete. Items under deferred platform work remain outside the first preview only when public documentation states that boundary clearly.

Publication readiness is not publication authority. After the candidate reaches this definition of done, obtain explicit authorization and execute the NuGet, PowerShell Gallery, tag, and GitHub release steps as one coordinated release operation.
