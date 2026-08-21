# MessageX.Teams

MessageX.Teams provides typed Microsoft Teams message composition and Workflow/incoming-webhook delivery. It depends on `MessageX.Core` and does not include a second Microsoft Graph client.

```csharp
using MessageX.Teams;

var message = new TeamsMessageRequest {
    Title = "Deployment completed",
    Text = "Production is healthy."
};
var target = TeamsMessageTarget.ForWorkflowWebhook(workflowUri);

using var sender = new WebhookTeamsMessageSender();
var result = await sender.SendAsync(message, target, cancellationToken);
```

Webhook URLs are credentials: resolve them from the consuming application's secret store and do not persist them in message references or logs. Authenticated Teams conversation lifecycle will integrate through Teams app/bot contracts or a thin adapter to GraphEssentialsX rather than duplicating Graph authentication, paging, throttling, and governance.
