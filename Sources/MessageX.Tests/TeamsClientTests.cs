using MessageX.Teams;

namespace MessageX.Tests;

public class TeamsClientTests {
    [Fact]
    public async Task SendAsyncDispatchesWorkflowRequestToRegisteredSender() {
        var target = TeamsMessageTarget.ForWorkflowWebhook(new Uri("https://example.test/workflow/secret"));
        var sender = new RecordingSender();
        var client = new TeamsClient(new[] { sender });
        var message = new TeamsMessageRequest { Text = "Build completed" };

        var result = await client.SendAsync(message, target, TestContext.Current.CancellationToken);

        Assert.Same(message, sender.Message);
        Assert.Equal(target.GetWebhookUri(), sender.Endpoint);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendAsyncRejectsUnregisteredDeliveryMethod() {
        var target = new TeamsMessageTarget { DeliveryMethod = (TeamsDeliveryMethod)999 };
        var client = new TeamsClient(Array.Empty<ITeamsMessageSender>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" }, target,
            TestContext.Current.CancellationToken));
    }

    private sealed class RecordingSender : ITeamsMessageSender {
        public TeamsMessageRequest? Message { get; private set; }
        public Uri? Endpoint { get; private set; }
        public bool CanSend(TeamsDeliveryMethod method) => method == TeamsDeliveryMethod.WorkflowWebhook;
        public Task<TeamsDeliveryResult> SendAsync(TeamsMessageRequest message, TeamsMessageTarget target,
            CancellationToken cancellationToken = default) {
            Message = message;
            Endpoint = target.GetWebhookUri();
            return Task.FromResult(new TeamsDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                Target = "test",
                IsSuccessStatusCode = true,
                StatusCode = 200
            });
        }
    }
}
