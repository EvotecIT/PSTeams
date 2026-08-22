using System.Net;
using System.Net.Http;
using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackWebApiMessageSenderTests {
    [Fact]
    public async Task SuccessfulSendUsesBearerAuthAndReturnsDurableReference() {
        const string responseBody = "{\"ok\":true,\"channel\":\"C0123456789\",\"ts\":\"1712345678.123456\"}";
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token", workspaceId: "T0123"),
            new HttpClient(handler),
            disposeHttpClient: true);
        var message = new SlackMessageRequest {
            Text = "Build completed",
            ThreadTimestamp = "1712000000.000001"
        };

        var result = await sender.SendAsync(
            message,
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("xoxb-secret-token", handler.AuthorizationParameter);
        Assert.Equal("https://slack.com/api/chat.postMessage", handler.RequestUri?.AbsoluteUri);
        Assert.Equal(MessageProviders.Slack, result.Reference?.Provider);
        Assert.Equal("1712345678.123456", result.Reference?.MessageId);
        Assert.Equal("C0123456789", result.Reference?.ConversationId);
        Assert.Equal("1712000000.000001", result.Reference?.ThreadId);
        Assert.Equal(MessageConversationKind.Thread, result.Reference?.ConversationKind);
        Assert.Equal("T0123", result.Reference?.ScopeId);
        Assert.Equal(
            MessageCapabilities.Reply |
            MessageCapabilities.Update |
            MessageCapabilities.Delete |
            MessageCapabilities.React,
            result.Reference?.Capabilities);
        Assert.Equal(
            DateTimeOffset.FromUnixTimeSeconds(1712345678).AddTicks(1_234_560),
            result.Reference?.Timestamp);
    }

    [Fact]
    public async Task SlackApiErrorIsClassifiedWithoutRawResponseInSafeDiagnostics() {
        const string responseBody = "{\"ok\":false,\"error\":\"missing_scope\",\"needed\":\"chat:write\"}";
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.Authorization, result.ErrorKind);
        Assert.Equal("missing_scope", result.ProviderCode);
        Assert.Contains("missing_scope", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("chat:write", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(responseBody, result.ResponseBody);
    }

    [Fact]
    public async Task InvalidSuccessPayloadIsRejectedAsTransient() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "not-json");
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Equal("invalid_response", result.ProviderCode);
    }

    [Fact]
    public async Task ProviderEvolvedResponseIdentifierRemainsASuccessfulDelivery() {
        const string responseBody = "{\"ok\":true,\"channel\":\"Xprovider-evolved-α\",\"ts\":\"1712345678.123456\"}";
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Xprovider-evolved-α", result.Reference?.ConversationId);
        Assert.Equal(MessageConversationKind.Unknown, result.Reference?.ConversationKind);
    }

    [Theory]
    [InlineData("C0123456789", MessageConversationKind.Channel)]
    [InlineData("D0123456789", MessageConversationKind.DirectMessage)]
    [InlineData("G0123456789", MessageConversationKind.Unknown)]
    public async Task SuccessfulSendPersistsOnlyConversationShapesKnownFromCoordinates(
        string conversationId,
        MessageConversationKind expectedKind) {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            $"{{\"ok\":true,\"channel\":\"{conversationId}\",\"ts\":\"1712345678.123456\"}}");
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation(conversationId),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedKind, result.Reference?.ConversationKind);
    }

    [Fact]
    public async Task NegativeEnvelopeWithoutErrorCodeIsRejectedAsTransient() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":false}");
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
    }

    [Theory]
    [InlineData("{\"ok\":true,\"channel\":\" \",\"ts\":\"1712345678.123456\"}")]
    [InlineData("{\"ok\":true,\"channel\":\"C0123456789\",\"ts\":\"not-a-timestamp\"}")]
    [InlineData("{\"ok\":true,\"channel\":\"C0123456789\",\"ts\":\"9999999999999999999999999999.1\"}")]
    public async Task MalformedSuccessCoordinatesAreRejectedWithoutThrowing(string responseBody) {
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reference);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
    }

    [Fact]
    public async Task HttpRateLimitIsClassifiedAndCarriesRetryAfter() {
        using var handler = new RecordingHandler(
            HttpStatusCode.TooManyRequests,
            "{\"ok\":false,\"error\":\"ratelimited\"}",
            retryAfterSeconds: 45);
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.Equal(MessageErrorKind.RateLimited, result.ErrorKind);
        Assert.Equal(TimeSpan.FromSeconds(45), result.RetryAfter);
        Assert.Equal("slack-request-42", result.CorrelationId);
    }

    [Fact]
    public async Task Http200RateLimitedEnvelopeIsRetryable() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":false,\"error\":\"rate_limited\"}");
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.Equal(MessageErrorKind.RateLimited, result.ErrorKind);
        Assert.Equal("rate_limited", result.ProviderCode);
    }

    [Theory]
    [InlineData("restricted_action_read_only_channel", MessageErrorKind.Authorization)]
    [InlineData("restricted_action_thread_locked", MessageErrorKind.Authorization)]
    [InlineData("restricted_action_thread_only_channel", MessageErrorKind.Authorization)]
    [InlineData("restricted_action_non_threadable_channel", MessageErrorKind.Authorization)]
    [InlineData("access_denied", MessageErrorKind.Authorization)]
    [InlineData("app_access_restricted", MessageErrorKind.Authorization)]
    [InlineData("ekm_access_denied", MessageErrorKind.Authorization)]
    [InlineData("not_in_channel", MessageErrorKind.Authorization)]
    [InlineData("is_archived", MessageErrorKind.NotFound)]
    [InlineData("channel_is_archived", MessageErrorKind.NotFound)]
    [InlineData("thread_not_found", MessageErrorKind.NotFound)]
    [InlineData("message_not_found", MessageErrorKind.NotFound)]
    [InlineData("duplicate_channel_not_found", MessageErrorKind.NotFound)]
    [InlineData("duplicate_message_not_found", MessageErrorKind.NotFound)]
    [InlineData("org_login_required", MessageErrorKind.Transient)]
    public async Task DocumentedProviderErrorsHaveActionableClassification(
        string providerCode,
        MessageErrorKind expectedKind) {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            $"{{\"ok\":false,\"error\":\"{providerCode}\"}}");
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(handler),
            disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedKind, result.ErrorKind);
        Assert.Equal(providerCode, result.ProviderCode);
    }

    [Fact]
    public async Task TransportTimeoutCoversStalledResponseBody() {
        using var httpClient = new HttpClient(new StallingResponseHandler()) {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            httpClient);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NetworkExceptionDoesNotRetainBotToken() {
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(new ThrowingHandler()),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken));

        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task ResponseBodyIoFailureReturnsSanitizedTransientFailure() {
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(new ThrowingResponseStreamHandler()),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task CallerCancellationWinsWhenResponseStreamReportsIoFailure() {
        using var sender = new SlackWebApiMessageSender(
            SlackConnection.ForBotToken("xoxb-secret-token"),
            new HttpClient(new ThrowingResponseStreamHandler(throwAfterCancellation: true)),
            disposeHttpClient: true);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForConversation("C0123456789"),
            cancellation.Token));

        Assert.IsNotType<MessageDeliveryException>(exception);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;
        private readonly int? _retryAfterSeconds;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody, int? retryAfterSeconds = null) {
            _statusCode = statusCode;
            _responseBody = responseBody;
            _retryAfterSeconds = retryAfterSeconds;
        }

        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public Uri? RequestUri { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            RequestUri = request.RequestUri;
            _ = await request.Content!.ReadAsStringAsync(cancellationToken);
            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
            response.Headers.TryAddWithoutValidation("x-slack-req-id", "slack-request-42");
            if (_retryAfterSeconds is not null) {
                response.Headers.TryAddWithoutValidation("Retry-After", _retryAfterSeconds.Value.ToString());
            }
            return response;
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            throw new HttpRequestException($"Connection failed with {request.Headers.Authorization}");
        }
    }
}
