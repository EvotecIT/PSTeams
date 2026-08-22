using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordClientTests {
    [Fact]
    public void DefaultClientsReuseSharedHttpTransport() {
        using var first = new DiscordClient();
        using var second = new DiscordClient();

        var httpClient = ReadWebhookHttpClient(first);
        Assert.Same(httpClient, ReadWebhookHttpClient(second));
        Assert.Equal(DiscordHttpClientFactory.DefaultUserAgent, httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void ConfiguredClientUsesProviderDefaultOrExplicitUserAgent() {
        using var defaulted = new DiscordClient(new MessageHttpTransportOptions { Timeout = TimeSpan.FromSeconds(30) });
        using var explicitAgent = new DiscordClient(new MessageHttpTransportOptions {
            UserAgent = "MessageX.Tests/1.0"
        });

        Assert.Equal(
            DiscordHttpClientFactory.DefaultUserAgent,
            ReadWebhookHttpClient(defaulted).DefaultRequestHeaders.UserAgent.ToString());
        Assert.Equal("MessageX.Tests/1.0", ReadWebhookHttpClient(explicitAgent).DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void DefaultBotSenderUsesProviderUserAgent() {
        using var sender = new DiscordBotMessageSender(DiscordConnection.ForBotToken("discord-super-secret-token-value"));
        var httpClient = Assert.IsType<HttpClient>(
            typeof(DiscordBotMessageSender)
                .GetField("_httpClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(sender));

        Assert.Equal(DiscordHttpClientFactory.DefaultUserAgent, httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public async Task ClientRoutesToMatchingSender() {
        var webhook = new RecordingSender(DiscordDeliveryMethod.IncomingWebhook);
        var bot = new RecordingSender(DiscordDeliveryMethod.BotChannel);
        using var client = new DiscordClient(new IDiscordMessageSender[] { webhook, bot });

        await client.SendAsync(
            new DiscordMessageRequest { Content = "hello" },
            DiscordMessageTarget.ForChannel("123456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.False(webhook.WasCalled);
        Assert.True(bot.WasCalled);
    }

    [Fact]
    public async Task WebhookOnlyClientExplainsMissingBotConnection() {
        using var client = new DiscordClient(new IDiscordMessageSender[] {
            new RecordingSender(DiscordDeliveryMethod.IncomingWebhook)
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(
            new DiscordMessageRequest { Content = "hello" },
            DiscordMessageTarget.ForDirectMessage("423456789012345678"),
            TestContext.Current.CancellationToken));

        Assert.Contains("DiscordConnection", exception.Message, StringComparison.Ordinal);
    }

    private static HttpClient ReadWebhookHttpClient(DiscordClient client) {
        var senders = Assert.IsAssignableFrom<IReadOnlyList<IDiscordMessageSender>>(
            typeof(DiscordClient)
                .GetField("_senders", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(client));
        var sender = Assert.IsType<DiscordIncomingWebhookSender>(senders[0]);
        return Assert.IsType<HttpClient>(
            typeof(DiscordIncomingWebhookSender)
                .GetField("_httpClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(sender));
    }

    private sealed class RecordingSender : IDiscordMessageSender {
        private readonly DiscordDeliveryMethod _method;

        public RecordingSender(DiscordDeliveryMethod method) {
            _method = method;
        }

        public bool WasCalled { get; private set; }
        public bool CanSend(DiscordDeliveryMethod deliveryMethod) => deliveryMethod == _method;

        public Task<DiscordDeliveryResult> SendAsync(
            DiscordMessageRequest message,
            DiscordMessageTarget target,
            CancellationToken cancellationToken = default) {
            WasCalled = true;
            return Task.FromResult(new DiscordDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                IsSuccess = true,
                StatusCode = 200,
                Target = target.ToString()
            });
        }
    }
}
