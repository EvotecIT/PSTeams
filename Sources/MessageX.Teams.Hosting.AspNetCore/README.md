# MessageX.Teams.Hosting.AspNetCore

This package connects the Microsoft Teams SDK for .NET to MessageX hosting. Microsoft owns the authenticated `/api/messages` endpoint and activity parsing; this adapter only maps verified typed activities into MessageX routes and safe event envelopes.

The hosting package targets modern .NET because the Microsoft Teams SDK does. `MessageX.Teams` continues to target .NET Framework 4.7.2, .NET 8, and .NET 10 for reusable message composition and delivery.
