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

## Prepare downstream pilots

Implement the first real consumers before freezing the release candidate. During development they may consume unpublished staged packages, but never repository-relative MessageX source shortcuts.

- [ ] Add an adapter in a private downstream notification consumer that keeps its existing domain notification layer as the owner and MessageX as transport.
- [ ] Add optional [EventViewerX](https://github.com/EvotecIT/EventViewerX) provider sinks without pulling every provider into its core engine.
- [ ] Reuse EventViewerX buffering/outbox behavior and keep HTML/email reporting in its existing owner.
- [ ] Bring the MessageX, private downstream consumer, and EventViewerX branches through their required CI and review gates without publishing any package or module.

## Freeze the signed verification candidate

Complete the release decisions and freeze the exact candidate before mandatory verification begins. Every subsequent gate applies to this signed candidate, not an earlier staging build.

- [ ] Choose final public package IDs and confirm whether the PowerShell module remains `PSTeams` or gains a separate `MessageX` identity.
- [ ] Confirm repository identity; do not rename the GitHub repository as an incidental build change.
- [ ] Choose the preview version and use three-part public versions.
- [ ] Freeze accepted exact MessageX, private downstream consumer, and EventViewerX commits for the complete verification and release operation.
- [ ] Rebuild the complete NuGet and PowerShell candidate from the frozen MessageX commit using the intended public three-part PowerForge/PSPublishModule version.
- [ ] Enable signing only with the intended release certificate; verify signatures, package contents, repository metadata, and SHA-256 hashes.
- [ ] Create one content-addressed verification manifest that binds all three frozen commits, public versions, package files, module files, signatures, hashes, build-tool versions, and the exact configuration used to produce them.
- [ ] Give the manifest a candidate ID derived from its digest. Record every CI, review, live, clean-consumer, and downstream evidence item against that candidate ID and its exact test inputs.
- [ ] Configure publication to fail when a target NuGet version, PowerShell Gallery version, Git tag, or GitHub release already exists; unconditional duplicate skipping or tag replacement is not allowed for a coordinated release.
- [ ] Replace the independent or timestamp-based GitHub publisher paths with one dry-run coordinated plan whose version-derived tag targets the frozen MessageX commit and whose single release contains only the manifest-authorized NuGet and PSTeams artifacts. Keep every upload and GitHub publication switch disabled while building and verifying the candidate.

The verification manifest is immutable after it is generated. Any source, dependency, build, signing, package-content, deployment-configuration, manifest-input, private downstream consumer, or EventViewerX change creates a new candidate ID and invalidates all prior mandatory evidence. Freeze, sign, and record the replacement candidate in a new manifest, then rerun every mandatory CI, review, live, clean-consumer, and downstream verification gate. Evidence is never copied or reassigned between candidate IDs.

## Mandatory live verification before publication

These checks are release blockers and must exercise the exact signed verification candidate. Run them only against the designated test installations with credentials supplied through approved secret storage. Missing credentials or an unavailable test environment keeps the candidate unpublished; it does not turn live verification into an optional gate.

- [ ] Teams Workflow and incoming-webhook notifications with webhook-supported Adaptive Cards in the designated test tenant.
- [ ] Authenticated Teams app HTTP activity and Adaptive Card action through the real test installation, endpoint route, request verification, routing, and action dispatch path.
- [ ] Slack incoming-webhook notification plus bot send, reply, update, delete, reaction, conversation resolution, external file upload, button response, and modal open in the designated test workspace.
- [ ] Verified Slack Events API event, slash command, block action, shortcut, and view submission through request verification, acknowledgement, routing, and dispatch.
- [ ] Discord incoming-webhook notification plus bot send, reply, read, update, delete, reaction, attachment, and follow-up lifecycle in the designated test guild.
- [ ] Verified Discord command, component, autocomplete, and modal submission through request verification, acknowledgement, routing, and dispatch.
- [ ] Verify negative authentication, scope/permission, expiry, replay, and rate-limit behavior without exposing response bodies or secrets.
- [ ] Clean all test messages, files, views, and other artifacts that the provider allows us to remove.

## Mandatory clean-consumer verification before publication

Use clean environments with no repository-relative source, shared MessageX package cache, or previously installed candidate module. Restore into isolated package locations with cache reuse disabled, and install the module into an isolated module path.

- [ ] Restore and build representative C# notification and hosting consumers using only the signed staged NuGet feed.
- [ ] Run those consumers for every supported C# runtime family; verify host startup, actual loaded assembly locations, versions, and hashes, plus one provider-neutral dispatch path.
- [ ] Install the signed staged PSTeams module and import it in Windows PowerShell 5.1 and supported PowerShell 7 environments.
- [ ] Exercise representative Teams, Slack, and Discord composition and delivery commands through the installed signed module.
- [ ] Confirm the consumers and module resolve only the frozen package/module versions recorded in the artifact manifest.

## Mandatory downstream verification before publication

A private downstream notification consumer and EventViewerX must consume the signed verification packages from the staged feed, never repository-relative source shortcuts. These pilots prove that the public boundaries work for real consumers before the first release.

- [ ] Validate incident, recovery, aggregation, suppression, restart, backpressure, and duplicate-delivery behavior.
- [ ] Validate burst handling, cancellation, throttling, restart, and partial multi-target failure.
- [ ] Rebuild the frozen private downstream consumer and EventViewerX commits from clean environments using only the signed staged package feed.
- [ ] Confirm both pilots resolve the exact MessageX versions and hashes recorded in the artifact manifest.

Keep the pilot source commits frozen. During coordinated publication, rebuild them with an isolated restore configuration, empty package cache, and package location that expose only NuGet.org and the exact authorized versions. Persist normal public-feed configuration in downstream branches only after the coordinated release completes.

## Publication authorization

Publication remains a separate, explicitly authorized operation. Request that authorization only after the exact signed candidate passes every mandatory gate above.

- [ ] Confirm the artifact manifest's frozen commits, versions, signatures, and hashes match every signed staged artifact and completed mandatory gate.
- [ ] Confirm NuGet, PowerShell Gallery, and GitHub credentials and produce a dry-run publication plan without uploading anything.
- [ ] Obtain explicit authorization for this exact commit, version set, artifact manifest, and coordinated release operation.

## Coordinated publication

The MessageX NuGet packages, rebuilt PSTeams module, exact-commit tag, and GitHub release are one release unit. Dependency order is internal sequencing, not permission to leave a partial release as the supported state. Do not begin this section until every candidate-freeze, mandatory-verification, and authorization item above is complete.

- [ ] Reconfirm the accepted commits, candidate ID, manifest digest, signatures, hashes, target-version absence, credentials, and explicit release authorization immediately before upload.
- [ ] Use only the files named in the authorized manifest as publisher inputs; abort if any local file hash or signer differs.
- [ ] Publish the complete MessageX NuGet set in dependency order with duplicate skipping disabled. Download every package from NuGet.org, verify the author and repository signatures, compare the immutable payload entries, dependencies, and repository commit with the authorized manifest, and record the repository-signed archive digest separately; do not compare the rewritten public `.nupkg` byte-for-byte with its pre-upload archive hash.
- [ ] Rebuild and run the clean C# consumers and frozen downstream pilots using only NuGet.org and the exact public MessageX versions. Confirm their resolved payload identities match the authorized manifest before publishing the PowerShell module or GitHub release.
- [ ] Publish the matching PSTeams module to PowerShell Gallery in that same guarded release operation. Download it into clean Windows PowerShell 5.1 and PowerShell 7 environments, then verify its file hashes, manifest metadata, Authenticode signer, import, and representative commands against the authorized manifest.
- [ ] Only after both public-feed verification gates pass, create the exact-commit tag and visible GitHub prerelease with generated release notes and the verified release artifacts. Do not enable the GitHub publication step earlier in the operation.
- [ ] Record the final versions, frozen commits, candidate payload hashes, author signatures, public archive digests and repository signatures, feed URLs, and verification results in the GitHub release.

If any upload or public verification fails, stop the release operation, record the partial public state outside a visible GitHub release, and resolve it before announcing the release. Never overwrite, delete, or reuse an already-public version as recovery; create a new version, candidate ID, manifest, and complete verification run when replacement artifacts are required. NuGet and PowerShell Gallery publication is not transactionally atomic, so the guarded operation and post-upload verification are the fail-closed boundary.

GitHub Releases and generated release notes are the release-history source of truth. Before declaring that history complete, backfill the missing `0.1.0` through `0.6.0` notes from Git history under explicit release authorization. This repository does not maintain a duplicate changelog file.

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
