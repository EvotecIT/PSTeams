# MessageX.Discord

`MessageX.Discord` is an owned, `System.Text.Json`-based Discord protocol client for incoming webhooks, bot REST delivery, embeds, attachments, and interaction signature verification. It targets .NET Framework 4.7.2, .NET 8, and .NET 10 without Discord.Net, NetCord, or Newtonsoft.Json.

```csharp
using MessageX.Discord;

var connection = DiscordConnection.ForBotToken(
    Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN")!);
using var client = new DiscordClient(connection);
var target = DiscordMessageTarget.ForChannel("123456789012345678");
var message = new DiscordMessageRequest { Content = "Deployment completed" };
message.Embeds.Add(new DiscordEmbed {
    Title = "Release",
    Description = "The deployment completed successfully.",
    Color = 0x2EB886
});

var result = await client.SendAsync(message, target);
Console.WriteLine(result.Reference?.MessageId);
```

Incoming-webhook URLs and bot tokens are credentials. Resolve them from a secret store, do not persist webhook targets, and do not write raw response bodies to normal application logs. Mention parsing is disabled by default. Direct-message delivery should be used for user-initiated or otherwise expected conversations rather than unsolicited bulk messaging.

Interaction signature verification proves authenticity and can bound request age, but it does not by itself prevent replay inside that window. An inbound host must also keep a short-lived cache of accepted signatures and reject duplicates before dispatching an interaction.
