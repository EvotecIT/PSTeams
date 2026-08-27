using System.Net;
using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackInteractionResponseClientTests {
    [Fact]
    public async Task RespondPostsToVerifiedTransientCapabilityWithoutAuthorization() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            "trigger-42",
            "https://hooks.slack.com/actions/T123/B456/secret-value");

        var result = await client.RespondAsync(context, new SlackInteractionResponseRequest {
            Message = new SlackMessageRequest {
                Text = "Approval recorded"
            },
            ReplaceOriginal = true,
            Visibility = SlackInteractionResponseVisibility.InChannel
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("verified Slack interaction response", result.Target);
        Assert.NotNull(handler.Request);
        Assert.Null(handler.Request!.Authorization);
        Assert.Equal("https://hooks.slack.com/actions/T123/B456/secret-value", handler.Request.Uri.AbsoluteUri);
        Assert.Contains("\"text\":\"Approval recorded\"", handler.Request.Body);
        Assert.Contains("\"replace_original\":true", handler.Request.Body);
        Assert.Contains("\"response_type\":\"in_channel\"", handler.Request.Body);
        Assert.DoesNotContain("secret-value", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RespondRendersDeleteWithoutMessageContent() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            null,
            "https://hooks.slack-gov.com/actions/T123/B456/secret-value");

        var result = await client.RespondAsync(context, new SlackInteractionResponseRequest {
            DeleteOriginal = true
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("{\"delete_original\":true}", handler.Request!.Body);
    }

    [Fact]
    public async Task RespondAcceptsVerifiedSlashCommandCapability() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            null,
            "https://hooks.slack.com/commands/T123/B456/secret-value");

        var result = await client.RespondAsync(context, new SlackInteractionResponseRequest {
            Message = new SlackMessageRequest { Text = "Command completed" }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "https://hooks.slack.com/commands/T123/B456/secret-value",
            handler.Request?.Uri.AbsoluteUri);
    }

    [Fact]
    public async Task RespondRejectsUntrustedResponseCapabilityBeforeSending() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            null,
            "https://attacker.example/actions/T123/B456/secret-value");

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.RespondAsync(
            context,
            new SlackInteractionResponseRequest {
                Message = new SlackMessageRequest { Text = "No" }
            },
            TestContext.Current.CancellationToken));

        Assert.Null(handler.Request);
    }

    [Fact]
    public async Task RespondRejectsDurablyRestoredUnavailableContext() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.RespondAsync(
            SlackTransientInteractionContext.Unavailable,
            new SlackInteractionResponseRequest {
                Message = new SlackMessageRequest { Text = "Too late" }
            },
            TestContext.Current.CancellationToken));

        Assert.Null(handler.Request);
    }

    [Fact]
    public async Task RespondRejectsExpiredCapabilityBeforeSending() {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var client = new SlackInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            null,
            "https://hooks.slack.com/actions/T123/B456/secret-value",
            DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(31)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.RespondAsync(
            context,
            new SlackInteractionResponseRequest {
                Message = new SlackMessageRequest { Text = "Too late" }
            },
            TestContext.Current.CancellationToken));

        Assert.Null(handler.Request);
    }

    [Fact]
    public void RendererRejectsDeleteWithReplacementContent() {
        Assert.Throws<ArgumentException>(() => SlackMessageRenderer.RenderInteractionResponse(
            new SlackInteractionResponseRequest {
                DeleteOriginal = true,
                Message = new SlackMessageRequest { Text = "Invalid" }
            }));
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;

        public RecordingHandler(HttpStatusCode statusCode) {
            _statusCode = statusCode;
        }

        public RecordedRequest? Request { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Request = new RecordedRequest(request.RequestUri!, request.Headers.Authorization, body);
            return new HttpResponseMessage(_statusCode) {
                Content = new StringContent("ok", Encoding.UTF8, "text/plain")
            };
        }
    }

    private sealed record RecordedRequest(
        Uri Uri,
        System.Net.Http.Headers.AuthenticationHeaderValue? Authorization,
        string Body);
}
