using MessageX.Teams;

namespace MessageX.Tests;

public class TeamsClientTests {
    [Fact]
    public async Task SendJsonAsyncUsesLaterRawCapableSender() {
        var target = TeamsMessageTarget.ForIncomingWebhook(new Uri("https://example.test/webhook"));
        var rawSender = new RawWebhookSender();
        var client = new TeamsClient(new ITeamsMessageSender[] {
            new TypedOnlyWebhookSender(),
            rawSender
        });

        var result = await client.SendJsonAsync("{\"text\":\"hello\"}", target, TestContext.Current.CancellationToken);

        Assert.True(rawSender.WasCalled);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("{\"text\":\"hello\"}", result.ResponseBody);
    }

    [Fact]
    public async Task SendAsyncHeroCardUsesRawCapableSender() {
        var target = TeamsMessageTarget.ForIncomingWebhook(new Uri("https://example.test/webhook"));
        var rawSender = new RawWebhookSender();
        var client = new TeamsClient(new ITeamsMessageSender[] {
            new TypedOnlyWebhookSender(),
            rawSender
        });

        var result = await client.SendAsync(new TeamsHeroCard {
            Title = "Hero"
        }, target, TestContext.Current.CancellationToken);

        Assert.True(rawSender.WasCalled);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Contains("\"type\":\"message\"", result.ResponseBody);
        Assert.Contains("\"contentType\":\"application/vnd.microsoft.card.hero\"", result.ResponseBody);
    }

    [Fact]
    public async Task SendAsyncHeroCardRejectsUnsupportedDeliveryMethods() {
        var target = new TeamsMessageTarget {
            DeliveryMethod = (TeamsDeliveryMethod)999,
            TargetUri = new Uri("https://example.test/unsupported")
        };
        var client = new TeamsClient(new ITeamsMessageSender[] {
            new TypedOnlyWebhookSender(),
            new RawWebhookSender()
        });

        var action = async () => await client.SendAsync(new TeamsHeroCard {
            Title = "Hero"
        }, target, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("incoming and workflow webhooks", exception.Message);
    }

    [Fact]
    public async Task CustomSenderCanReadWebhookEndpointThroughExplicitAccessor() {
        var uri = new Uri("https://example.test/workflows/secret-token");
        var target = TeamsMessageTarget.ForWorkflowWebhook(uri);
        var sender = new EndpointReadingSender();
        var client = new TeamsClient(new[] { sender });

        _ = await client.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.Equal(uri, sender.Endpoint);
    }

    private sealed class TypedOnlyWebhookSender : ITeamsMessageSender {
        public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
            return deliveryMethod is TeamsDeliveryMethod.IncomingWebhook;
        }

        public Task<TeamsDeliveryResult> SendAsync(
            TeamsMessageRequest message,
            TeamsMessageTarget target,
            CancellationToken cancellationToken = default) {
            throw new InvalidOperationException("Typed sender should not be selected for raw JSON sends.");
        }
    }

    private sealed class RawWebhookSender : ITeamsRawMessageSender {
        public bool WasCalled { get; private set; }

        public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
            return deliveryMethod is TeamsDeliveryMethod.IncomingWebhook;
        }

        public Task<TeamsDeliveryResult> SendAsync(
            TeamsMessageRequest message,
            TeamsMessageTarget target,
            CancellationToken cancellationToken = default) {
            throw new InvalidOperationException("Raw JSON test should not use typed send.");
        }

        public Task<TeamsDeliveryResult> SendJsonAsync(
            string jsonBody,
            TeamsMessageTarget target,
            CancellationToken cancellationToken = default) {
            WasCalled = true;

            return Task.FromResult(new TeamsDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                Target = target.DisplayName ?? target.TargetUri.Host,
                IsSuccessStatusCode = true,
                StatusCode = 200,
                ResponseBody = jsonBody
            });
        }
    }

    private sealed class EndpointReadingSender : ITeamsMessageSender {
        public Uri? Endpoint { get; private set; }

        public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
            return deliveryMethod is TeamsDeliveryMethod.WorkflowWebhook;
        }

        public Task<TeamsDeliveryResult> SendAsync(
            TeamsMessageRequest message,
            TeamsMessageTarget target,
            CancellationToken cancellationToken = default) {
            Endpoint = target.GetWebhookUri();
            return Task.FromResult(new TeamsDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                Target = "custom sender",
                IsSuccessStatusCode = true,
                StatusCode = 200
            });
        }
    }
}
