# MessageX.Slack

`MessageX.Slack` is an owned, `System.Text.Json`-based Slack protocol client for incoming webhooks and authenticated bot delivery through `chat.postMessage`. It targets .NET Framework 4.7.2, .NET 8, and .NET 10 without SlackNet or Newtonsoft.Json.

```csharp
using MessageX.Slack;

var connection = SlackConnection.ForBotToken(Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN")!);
using var client = new SlackClient(connection);
var target = SlackMessageTarget.ForConversation("C0123456789");
var message = new SlackMessageRequest { Text = "Deployment completed" };
message.Blocks.Add(new SlackSectionBlock {
    Text = SlackTextObject.Markdown("*Deployment completed*")
});

var result = await client.SendAsync(message, target);
Console.WriteLine($"{result.ConversationId}/{result.TimestampId}");
```

Incoming-webhook URLs and bot tokens are credentials. Resolve them from a secret store, do not persist webhook targets, and do not write raw response bodies to normal application logs. Incoming webhooks are fixed-destination and do not return durable message identifiers; use a bot connection when later reply or lifecycle operations need the returned conversation ID and Slack timestamp.
