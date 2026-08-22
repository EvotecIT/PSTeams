using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackClientTests {
    [Fact]
    public void DefaultClientsReuseSharedHttpTransport() {
        using var first = new SlackClient();
        using var second = new SlackClient();
        var connection = SlackConnection.ForBotToken("xoxb-secret-token", workspaceId: "T0123");
        using var lifecycle = new SlackWebApiLifecycleClient(connection);
        using var directory = new SlackConversationDirectory(connection);

        var shared = ReadWebhookHttpClient(first);
        Assert.Same(shared, ReadWebhookHttpClient(second));
        Assert.Same(shared, ReadOwnedHttpClient(lifecycle));
        Assert.Same(shared, ReadOwnedHttpClient(directory));
    }

    [Fact]
    public async Task ClientRoutesToMatchingSender() {
        var webhookSender = new RecordingSender(SlackDeliveryMethod.IncomingWebhook);
        var webApiSender = new RecordingSender(SlackDeliveryMethod.WebApi);
        using var client = new SlackClient(new ISlackMessageSender[] { webhookSender, webApiSender });

        await client.SendAsync(
            new SlackMessageRequest { Text = "hello" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.False(webhookSender.WasCalled);
        Assert.True(webApiSender.WasCalled);
    }

    [Fact]
    public async Task WebhookOnlyClientExplainsMissingAuthenticatedSender() {
        using var client = new SlackClient(new ISlackMessageSender[] {
            new RecordingSender(SlackDeliveryMethod.IncomingWebhook)
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(
            new SlackMessageRequest { Text = "hello" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken));

        Assert.Contains("SlackConnection", exception.Message, StringComparison.Ordinal);
    }

    private static HttpClient ReadWebhookHttpClient(SlackClient client) {
        var senders = Assert.IsAssignableFrom<IReadOnlyList<ISlackMessageSender>>(
            typeof(SlackClient)
                .GetField("_senders", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(client));
        var sender = Assert.IsType<SlackIncomingWebhookSender>(senders[0]);
        return Assert.IsType<HttpClient>(
            typeof(SlackIncomingWebhookSender)
                .GetField("_httpClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(sender));
    }

    private static HttpClient ReadOwnedHttpClient(object owner) {
        return Assert.IsType<HttpClient>(
            owner.GetType()
                .GetField("_httpClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(owner));
    }

    private sealed class RecordingSender : ISlackMessageSender {
        private readonly SlackDeliveryMethod _method;

        public RecordingSender(SlackDeliveryMethod method) {
            _method = method;
        }

        public bool WasCalled { get; private set; }
        public bool CanSend(SlackDeliveryMethod deliveryMethod) => deliveryMethod == _method;

        public Task<SlackDeliveryResult> SendAsync(
            SlackMessageRequest message,
            SlackMessageTarget target,
            CancellationToken cancellationToken = default) {
            WasCalled = true;
            return Task.FromResult(new SlackDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                IsSuccess = true,
                StatusCode = 200,
                Target = target.ToString()
            });
        }
    }
}
