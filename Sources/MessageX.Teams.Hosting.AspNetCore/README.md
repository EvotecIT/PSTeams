# MessageX.Teams.Hosting.AspNetCore

This package connects the Microsoft Teams SDK for .NET to MessageX hosting. Microsoft owns the authenticated `/api/messages` endpoint and activity parsing; this adapter only maps verified typed activities into MessageX routes and safe event envelopes.

The host calls `AddMessageXTeamsHosting()` to register the safe versioned Teams activity codec and supplies `ITeamsInstallationResolver` so each already-authenticated tenant/team/conversation is mapped to the correct non-secret MessageX installation. Asynchronously dispatched mapped work passes through the configured `IMessageIngressAcceptance` boundary, including durable acceptance when enabled, before the SDK callback succeeds. A restored durable payload retains handler-useful safe coordinates and content but does not recreate the transient Microsoft SDK activity. Adaptive Card invoke handlers are the explicit exception: their response must be produced inline on the original SDK callback, so they use bounded process-local replay protection and are not persisted or replayed after restart.

The hosting package targets modern .NET because the Microsoft Teams SDK does. `MessageX.Teams` continues to target .NET Framework 4.7.2, .NET 8, and .NET 10 for reusable message composition and delivery.
