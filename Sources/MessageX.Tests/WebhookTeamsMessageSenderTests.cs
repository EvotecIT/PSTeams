using System.Net;
using System.Net.Http;
using System.Text;
using MessageX.Teams;

namespace MessageX.Tests;

public sealed class WebhookTeamsMessageSenderTests {
    [Fact]
    public async Task SendJsonAsyncRejectsManuallyConstructedHttpTarget() {
        using var handler = new RecordingHandler();
        using var sender = new WebhookTeamsMessageSender(new HttpClient(handler), disposeHttpClient: true);
        var target = new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.WorkflowWebhook,
            TargetUri = new Uri("http://example.test/workflows/secret-token")
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => sender.SendJsonAsync(
            "{}",
            target,
            TestContext.Current.CancellationToken));

        Assert.Contains("HTTPS", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(handler.RequestUri);
    }

    [Fact]
    public void DefaultClientDisablesAutomaticRedirects() {
        using var handler = WebhookTeamsMessageSender.CreateDefaultHandler();

        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public async Task SendAsyncReturnsSafeTargetWithoutWebhookSecret() {
        using var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler);
        using var sender = new WebhookTeamsMessageSender(httpClient);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"),
            "Release alerts");

        var result = await sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("Release alerts", result.Target);
        Assert.DoesNotContain("secret-token", result.Target, StringComparison.Ordinal);
        Assert.Equal("https://example.test/workflows/secret-token", handler.RequestUri?.AbsoluteUri);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        public Uri? RequestUri { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            RequestUri = request.RequestUri;
            _ = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("accepted", Encoding.UTF8, "text/plain")
            };
        }
    }
}
