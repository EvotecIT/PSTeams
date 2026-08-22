# MessageX.Discord.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Discord interaction verifier and receiver.

```csharp
builder.Services.AddMessageXDiscordAspNetCore();

var discord = new DiscordEndpointConfiguration("application-primary", publicKeyHex);
app.MapMessageXDiscordInteractions("/messagex/discord/interactions", discord);
```

Map separate endpoint configuration for each installation. Discord owns the Ed25519 public key; interaction tokens remain transient provider payload state and never enter MessageX references or health state.
