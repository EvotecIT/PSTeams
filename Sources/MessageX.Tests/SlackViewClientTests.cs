using System.Net;
using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackViewClientTests {
    [Fact]
    public async Task OpenModalUsesVerifiedTriggerAndTypedInputBlocks() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":true,\"view\":{\"id\":\"V123ABC456\"}}");
        using var client = new SlackViewClient(
            SlackConnection.ForBotToken("xoxb-test-token"),
            new HttpClient(handler),
            disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            "trigger-42",
            "https://hooks.slack.com/actions/T/B/secret");
        var view = new SlackModalView {
            CallbackId = "approval-details",
            Title = SlackTextObject.Plain("Approval"),
            Submit = SlackTextObject.Plain("Submit"),
            Close = SlackTextObject.Plain("Cancel"),
            Blocks = {
                new SlackInputBlock {
                    BlockId = "reason-block",
                    Label = SlackTextObject.Plain("Reason"),
                    Element = new SlackPlainTextInputElement {
                        ActionId = "reason",
                        Multiline = true,
                        MaximumLength = 500,
                        Placeholder = SlackTextObject.Plain("Why?")
                    }
                }
            }
        };

        var result = await client.OpenModalAsync(
            context,
            view,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("V123ABC456", result.ViewId);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("https://slack.com/api/views.open", handler.Uri?.AbsoluteUri);
        Assert.Contains("\"trigger_id\":\"trigger-42\"", handler.Body);
        Assert.Contains("\"type\":\"plain_text_input\"", handler.Body);
        Assert.Contains("\"max_length\":500", handler.Body);
        Assert.DoesNotContain("secret", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenModalRejectsUnavailableTriggerBeforeNetworkUse() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = new SlackViewClient(
            SlackConnection.ForBotToken("xoxb-test-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.OpenModalAsync(
            SlackTransientInteractionContext.Unavailable,
            new SlackModalView {
                CallbackId = "approval",
                Title = SlackTextObject.Plain("Approval"),
                Blocks = {
                    new SlackDividerBlock()
                }
            },
            TestContext.Current.CancellationToken));

        Assert.Null(handler.Uri);
    }

    [Fact]
    public async Task OpenModalRejectsExpiredTriggerBeforeNetworkUse() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = new SlackViewClient(
            SlackConnection.ForBotToken("xoxb-test-token"),
            new HttpClient(handler),
            disposeHttpClient: true);
        var context = new SlackTransientInteractionContext(
            "trigger-42",
            null,
            DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.OpenModalAsync(
            context,
            new SlackModalView {
                CallbackId = "approval",
                Title = SlackTextObject.Plain("Approval"),
                Blocks = { new SlackDividerBlock() }
            },
            TestContext.Current.CancellationToken));

        Assert.Null(handler.Uri);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, MessageErrorKind.Authentication)]
    [InlineData(HttpStatusCode.Forbidden, MessageErrorKind.Authorization)]
    [InlineData(HttpStatusCode.TooManyRequests, MessageErrorKind.RateLimited)]
    public async Task OpenModalClassifiesHttpFailuresEvenWhenBodyIsInvalid(
        HttpStatusCode statusCode,
        MessageErrorKind expected) {
        using var handler = new RecordingHandler(statusCode, "not-json");
        using var client = new SlackViewClient(
            SlackConnection.ForBotToken("xoxb-test-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await client.OpenModalAsync(
            new SlackTransientInteractionContext("trigger-42", null),
            new SlackModalView {
                CallbackId = "approval",
                Title = SlackTextObject.Plain("Approval"),
                Blocks = { new SlackDividerBlock() }
            },
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.ErrorKind);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody) {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public Uri? Uri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            Uri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            Body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
