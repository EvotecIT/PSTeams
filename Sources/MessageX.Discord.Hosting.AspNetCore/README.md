# MessageX.Discord.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Discord interaction verifier and receiver.

```csharp
builder.Services.AddMessageXDiscordAspNetCore();

var discord = new DiscordEndpointConfiguration(
    "application-primary",
    publicKeyHex,
    applicationId: "100000000000000002",
    installationOwnerId: "100000000000000003");
app.MapMessageXDiscordInteractions("/messagex/discord/interactions", discord);
```

Map separate endpoint configuration for each installation. Signed payload application and authorizing-owner identity must match that route before acknowledgement. Discord owns the Ed25519 public key; interaction tokens remain transient provider payload state and never enter MessageX references or health state.

Commands and components use deferred acknowledgements before asynchronous dispatch. Autocomplete is dispatched inline so its handler can return typed choices in the initial response; the empty choice list is only the fallback acknowledgement. Because that response belongs to the original request, autocomplete uses bounded process-local replay protection and is explicitly outside durable persistence and restart replay. Durable follow-up responses require the still-pending secure interaction-capability owner; tokens are never stored in durable envelopes.
