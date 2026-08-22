using System.Net;
using System.Net.Http;
using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackIncomingWebhookSenderTests {
    [Fact]
    public async Task SuccessfulWebhookSendUsesJsonAndReturnsSafeTarget() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "ok");
        using var sender = new SlackIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);
        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"),
            "Release alerts");

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Release alerts", result.Target);
        Assert.Equal("application/json", handler.ContentType);
        Assert.Contains("Build completed", handler.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", result.Target, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RateLimitReturnsRetryAndSanitizedDiagnostics() {
        using var handler = new RecordingHandler(HttpStatusCode.TooManyRequests, "rate_limited");
        using var sender = new SlackIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);
        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"));

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.RateLimited, result.ErrorKind);
        Assert.Equal(TimeSpan.FromSeconds(30), result.RetryAfter);
        Assert.Equal("slack-request-42", result.CorrelationId);
        Assert.Equal("rate_limited", result.ProviderCode);
        Assert.DoesNotContain("secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ArchivedWebhookDestinationIsClassifiedAsNotFound() {
        using var handler = new RecordingHandler(HttpStatusCode.Gone, "channel_is_archived");
        using var sender = new SlackIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForIncomingWebhook(new Uri("https://hooks.slack.com/services/T/B/secret")),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.NotFound, result.ErrorKind);
        Assert.Equal("channel_is_archived", result.ProviderCode);
    }

    [Fact]
    public async Task UnsafeProviderAndCorrelationTextIsNotPromotedToDiagnostics() {
        using var handler = new RecordingHandler(
            HttpStatusCode.BadRequest,
            "rejected https://hooks.slack.com/services/secret",
            "https://hooks.slack.com/services/secret");
        using var sender = new SlackIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForIncomingWebhook(new Uri("https://hooks.slack.com/services/T/B/secret")),
            TestContext.Current.CancellationToken);

        Assert.Null(result.ProviderCode);
        Assert.Null(result.CorrelationId);
        Assert.DoesNotContain("secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NetworkExceptionDoesNotRetainWebhookSecret() {
        using var sender = new SlackIncomingWebhookSender(new HttpClient(new ThrowingHandler()), disposeHttpClient: true);
        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task ResponseBodyIoFailureReturnsSanitizedTransientFailure() {
        using var sender = new SlackIncomingWebhookSender(
            new HttpClient(new ThrowingResponseStreamHandler()),
            disposeHttpClient: true);
        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task CallerCancellationWinsWhenResponseStreamReportsIoFailure() {
        using var sender = new SlackIncomingWebhookSender(
            new HttpClient(new ThrowingResponseStreamHandler(throwAfterCancellation: true)),
            disposeHttpClient: true);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            target,
            cancellation.Token));

        Assert.IsNotType<MessageDeliveryException>(exception);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResponseBodyLimitStopsStreamingBeforeLargeContentIsBuffered() {
        using var handler = new LargeResponseHandler(1024 * 1024);
        using var sender = new SlackIncomingWebhookSender(new HttpClient(handler), disposeHttpClient: true);

        var result = await sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForIncomingWebhook(new Uri("https://hooks.slack.com/services/T/B/secret")),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.ResponseBody);
        Assert.InRange(handler.BytesRead, 1, MessageHttpResponseReader.DefaultMaximumBytes + 4096);
    }

    [Fact]
    public async Task TransportTimeoutCoversStalledResponseBody() {
        using var httpClient = new HttpClient(new StallingResponseHandler()) {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        using var sender = new SlackIncomingWebhookSender(httpClient);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new SlackMessageRequest { Text = "Build completed" },
            SlackMessageTarget.ForIncomingWebhook(new Uri("https://hooks.slack.com/services/T/B/secret")),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;
        private readonly string _requestId;

        public RecordingHandler(
            HttpStatusCode statusCode,
            string responseBody,
            string requestId = "slack-request-42") {
            _statusCode = statusCode;
            _responseBody = responseBody;
            _requestId = requestId;
        }

        public string? Body { get; private set; }
        public string? ContentType { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            ContentType = request.Content.Headers.ContentType?.MediaType;
            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "text/plain")
            };
            response.Headers.TryAddWithoutValidation("x-slack-req-id", _requestId);
            if (_statusCode == HttpStatusCode.TooManyRequests) {
                response.Headers.TryAddWithoutValidation("Retry-After", "30");
            }
            return response;
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            throw new HttpRequestException($"Connection failed for {request.RequestUri}");
        }
    }

    private sealed class LargeResponseHandler : HttpMessageHandler {
        private readonly int _length;

        public LargeResponseHandler(int length) {
            _length = length;
        }

        public int BytesRead { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var stream = new CountingStream(_length, count => BytesRead += count);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StreamContent(stream)
            });
        }
    }

    private sealed class CountingStream : Stream {
        private readonly byte[] _content;
        private readonly Action<int> _recordRead;
        private int _offset;

        public CountingStream(int length, Action<int> recordRead) {
            _content = Enumerable.Repeat((byte)'x', length).ToArray();
            _recordRead = recordRead;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _content.Length;
        public override long Position { get => _offset; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) {
            var read = Math.Min(count, _content.Length - _offset);
            Array.Copy(_content, _offset, buffer, offset, read);
            _offset += read;
            _recordRead(read);
            return read;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Read(buffer, offset, count));
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
