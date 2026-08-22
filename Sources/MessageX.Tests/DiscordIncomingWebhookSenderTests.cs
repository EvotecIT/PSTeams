using System.Net;
using System.Net.Http;
using System.Text;
using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordIncomingWebhookSenderTests {
    private static readonly Uri WebhookUri = new(
        "https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456");

    [Fact]
    public async Task WebhookForcesWaitAndReturnsDurableReference() {
        const string responseBody = "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345679\",\"timestamp\":\"2026-08-21T08:00:00Z\"}";
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new DiscordIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);
        var target = DiscordMessageTarget.ForIncomingWebhook(WebhookUri, "123456789012345679", "Release alerts");

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("?wait=true&thread_id=123456789012345679", handler.RequestUri?.Query);
        Assert.Equal("application/json", handler.ContentType);
        Assert.Equal("623456789012345678", result.Reference?.MessageId);
        Assert.Equal("123456789012345679", result.Reference?.ConversationId);
        Assert.Equal("123456789012345679", result.Reference?.ThreadId);
        Assert.Equal(MessageCapabilities.None, result.Reference?.Capabilities);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz", result.Target, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AttachmentsUseMultipartPayloadJsonAndIndexedFiles() {
        const string responseBody = "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\"}";
        using var handler = new RecordingHandler(HttpStatusCode.OK, responseBody);
        using var sender = new DiscordIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);
        var message = new DiscordMessageRequest { Content = "report" };
        message.Attachments.Add(DiscordAttachment.FromBytes(
            "report.txt",
            Encoding.UTF8.GetBytes("hello"),
            isSpoiler: true));

        var result = await sender.SendAsync(
            message,
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("multipart/form-data", handler.ContentType);
        Assert.Contains("name=payload_json", handler.Body, StringComparison.Ordinal);
        Assert.Contains("name=\"files[0]\"", handler.Body, StringComparison.Ordinal);
        Assert.Contains("filename=report.txt", handler.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("SPOILER_report.txt", handler.Body, StringComparison.Ordinal);
        Assert.Contains("\"is_spoiler\":true", handler.Body, StringComparison.Ordinal);
        Assert.Contains("hello", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RateLimitCarriesBucketScopeAndRetryMetadata() {
        using var handler = new RecordingHandler(
            HttpStatusCode.TooManyRequests,
            "{\"code\":20028,\"message\":\"rate limited\",\"retry_after\":2.5,\"global\":true}");
        using var sender = new DiscordIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.RateLimited, result.ErrorKind);
        Assert.Equal(TimeSpan.FromSeconds(4), result.RetryAfter);
        Assert.Equal("bucket-42", result.RateLimitBucket);
        Assert.Equal("global", result.RateLimitScope);
        Assert.True(result.IsGlobalRateLimit);
        Assert.Equal("discord-ray-42", result.CorrelationId);
        Assert.DoesNotContain("rate limited", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidSuccessEnvelopeIsRejectedAsTransient() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "not-json");
        using var sender = new DiscordIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Equal("invalid_response", result.ProviderCode);
    }

    [Theory]
    [InlineData("bucket-42", "bucket-42")]
    [InlineData("unsafe bucket", null)]
    [InlineData("value\r\nInjected", null)]
    public void RateLimitDiagnosticTokensAreSanitized(string input, string? expected) {
        Assert.Equal(expected, DiscordHttpResponseSupport.NormalizeDiagnosticToken(input));
    }

    [Fact]
    public async Task TransportTimeoutCoversStalledResponseBody() {
        using var httpClient = new HttpClient(new StallingResponseHandler()) { Timeout = TimeSpan.FromMilliseconds(50) };
        using var sender = new DiscordIncomingWebhookSender(httpClient);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
    }

    [Fact]
    public async Task ResponseBodyIoFailureReturnsSanitizedTransientFailure() {
        using var sender = new DiscordIncomingWebhookSender(
            new HttpClient(new ThrowingResponseStreamHandler()),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task CallerCancellationWinsWhenResponseStreamReportsIoFailure() {
        using var sender = new DiscordIncomingWebhookSender(
            new HttpClient(new ThrowingResponseStreamHandler(throwAfterCancellation: true)),
            disposeHttpClient: true);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri),
            cancellation.Token));

        Assert.IsNotType<MessageDeliveryException>(exception);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody) {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public Uri? RequestUri { get; private set; }
        public string? ContentType { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            RequestUri = request.RequestUri;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
            response.Headers.TryAddWithoutValidation("Retry-After", "4");
            response.Headers.TryAddWithoutValidation("X-RateLimit-Bucket", "bucket-42");
            response.Headers.TryAddWithoutValidation("X-RateLimit-Scope", "global");
            response.Headers.TryAddWithoutValidation("CF-Ray", "discord-ray-42");
            return response;
        }
    }
}
