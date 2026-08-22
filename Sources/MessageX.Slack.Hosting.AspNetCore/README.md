# MessageX.Slack.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Slack request verifiers and receivers.

```csharp
builder.Services.AddMessageXSlackAspNetCore();

var slack = new SlackEndpointConfiguration(
    "workspace-primary",
    signingSecret,
    applicationId: "A01234567",
    workspaceId: "T01234567");
app.MapMessageXSlackEvents("/messagex/slack/events", slack);
app.MapMessageXSlackInteractions("/messagex/slack/interactions", slack);
```

Map separate endpoint configuration for each installation. Signed payload application/workspace identity must match that route before acknowledgement. Signing secrets remain transient host configuration and are never placed in MessageX envelopes or health state.

Slack response URLs and trigger IDs are available only to the initial volatile handler context. Durable codecs intentionally remove them; a secure provider response-capability owner is still required before durable follow-up responses are supported.
