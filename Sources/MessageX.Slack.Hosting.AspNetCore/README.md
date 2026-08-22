# MessageX.Slack.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Slack request verifiers and receivers.

```csharp
builder.Services.AddMessageXSlackAspNetCore();

var slack = new SlackEndpointConfiguration("workspace-primary", signingSecret);
app.MapMessageXSlackEvents("/messagex/slack/events", slack);
app.MapMessageXSlackInteractions("/messagex/slack/interactions", slack);
```

Map separate endpoint configuration for each installation. Signing secrets remain transient host configuration and are never placed in MessageX envelopes or health state.
