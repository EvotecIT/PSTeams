# MessageX.Discord.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Discord interaction verifier and receiver.

```csharp
builder.Services.AddMessageXDiscordAspNetCore();
builder.Services.AddSingleton<IDiscordInstallationResolver, DiscordInstallationResolver>();

var discord = new DiscordApplicationEndpointConfiguration(
    publicKeyHex,
    applicationId: "100000000000000002");
app.MapMessageXDiscordInteractions("/messagex/discord/interactions", discord);
```

Discord sends every installation of an application to its one configured interaction endpoint. The installation resolver therefore receives the signed application, integration type, and authorizing owner after request verification and returns the trusted MessageX installation identifier. `DiscordEndpointConfiguration` remains available for an application that is intentionally bound to one installation. Discord owns the Ed25519 public key; interaction tokens remain transient provider payload state and never enter MessageX references or health state.

Commands and components use deferred acknowledgements before asynchronous dispatch. Autocomplete is dispatched inline so its handler can return typed choices in the initial response; the empty choice list is only the fallback acknowledgement. Because that response belongs to the original request, autocomplete uses bounded process-local replay protection and is explicitly outside durable persistence and restart replay. Durable follow-up responses require the still-pending secure interaction-capability owner; tokens are never stored in durable envelopes.
