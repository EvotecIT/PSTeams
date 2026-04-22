using TeamsX;

namespace TeamsX.Tests;

public class TeamsClientTests {
    [Fact]
    public async Task SendJsonAsyncUsesLaterRawCapableSender() {
        var target = TeamsMessageTarget.ForIncomingWebhook(new Uri("https://example.test/webhook"));
        var rawSender = new RawWebhookSender();
        var client = new TeamsClient(new ITeamsMessageSender[] {
            new TypedOnlyWebhookSender(),
            rawSender
        });

        var result = await client.SendJsonAsync("{\"text\":\"hello\"}", target);

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
        }, target);

        Assert.True(rawSender.WasCalled);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Contains("\"type\":\"message\"", result.ResponseBody);
        Assert.Contains("\"contentType\":\"application/vnd.microsoft.card.hero\"", result.ResponseBody);
    }

    [Fact]
    public async Task SendAsyncHeroCardRejectsUnsupportedDeliveryMethods() {
        var target = TeamsMessageTarget.ForGraphChatMessage("19:testchat@thread.v2", "token-1");
        var client = new TeamsClient(new ITeamsMessageSender[] {
            new TypedOnlyWebhookSender(),
            new RawWebhookSender()
        });

        var action = async () => await client.SendAsync(new TeamsHeroCard {
            Title = "Hero"
        }, target);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("incoming and workflow webhooks", exception.Message);
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
                TargetUri = target.TargetUri,
                IsSuccessStatusCode = true,
                StatusCode = 200,
                ResponseBody = jsonBody
            });
        }
    }
}
