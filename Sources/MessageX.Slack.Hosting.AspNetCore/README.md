# MessageX.Slack.Hosting.AspNetCore

Thin ASP.NET Core endpoints over the MessageX Slack request verifiers and receivers.

```csharp
builder.Services.AddMessageXSlackAspNetCore();
builder.Services.AddSingleton<ISlackInstallationResolver, SlackInstallationResolver>();

var slack = new SlackApplicationEndpointConfiguration(
    signingSecret,
    applicationId: "A01234567");
app.MapMessageXSlackEvents("/messagex/slack/events", slack);
app.MapMessageXSlackInteractions("/messagex/slack/interactions", slack);
```

Slack sends every installation of an application to the same configured request URLs. The installation resolver therefore receives signed application, workspace, and Enterprise Grid coordinates after request verification and returns the trusted MessageX installation identifier. `SlackEndpointConfiguration` remains available for an application that is intentionally bound to one installation. Signing secrets remain transient host configuration and are never placed in MessageX envelopes or health state.

Modal view submissions run inline so the registered submission handler can return Slack validation errors or another modal response on the original request. They use bounded process-local replay protection and are not persisted or replayed after a host restart. Other accepted Slack work uses the configured ingress acceptance boundary.

Slack response URLs and trigger IDs are available only to the initial volatile handler context. Durable codecs intentionally remove them; a secure provider response-capability owner is still required before durable follow-up responses are supported.
