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
    public void ConfiguredClientAppliesEnterpriseTransportOptions() {
        var proxyUri = new Uri("http://proxy.example.test:8080");
        var options = new MessageHttpTransportOptions {
            ProxyUri = proxyUri,
            Timeout = TimeSpan.FromSeconds(15),
            UserAgent = "MessageX.Tests/1.0"
        };

        using var handler = WebhookTeamsMessageSender.CreateDefaultHandler(options);
        using var client = WebhookTeamsMessageSender.CreateDefaultHttpClient(options);

        Assert.False(handler.AllowAutoRedirect);
        Assert.True(handler.UseProxy);
        Assert.Equal(proxyUri, handler.Proxy?.GetProxy(new Uri("https://example.test")));
        Assert.Equal(TimeSpan.FromSeconds(15), client.Timeout);
        Assert.Contains("MessageX.Tests/1.0", client.DefaultRequestHeaders.UserAgent.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ConfiguredClientRejectsUnsafeProxySchemes() {
        var options = new MessageHttpTransportOptions {
            ProxyUri = new Uri("ftp://proxy.example.test")
        };

        var action = () => new WebhookTeamsMessageSender(options);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.DoesNotContain("proxy.example.test", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RateLimitResponseReturnsClassifiedSafeDiagnostics() {
        using var handler = new RecordingHandler(HttpStatusCode.TooManyRequests);
        using var sender = new WebhookTeamsMessageSender(new HttpClient(handler), disposeHttpClient: true);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"),
            destination: TeamsWorkflowDestinationKind.GroupChat);

        var result = await sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.RateLimited, result.ErrorKind);
        Assert.Equal(TimeSpan.FromSeconds(30), result.RetryAfter);
        Assert.Equal("request-42", result.CorrelationId);
        Assert.DoesNotContain("secret-token", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-token", result.Target, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnsafeCorrelationHeadersAreDiscarded() {
        var unsafeValues = new[] {
            "https://example.test/workflows/secret-token",
            "request-42\r\nsecret-token",
            new string('a', 129)
        };

        foreach (var unsafeValue in unsafeValues) {
            using var handler = new RecordingHandler(HttpStatusCode.TooManyRequests, unsafeValue);
            using var sender = new WebhookTeamsMessageSender(new HttpClient(handler), disposeHttpClient: true);
            var target = TeamsMessageTarget.ForWorkflowWebhook(
                new Uri("https://example.test/workflows/secret-token"));

            var result = await sender.SendAsync(
                new TeamsMessageRequest { Text = "Build completed" },
                target,
                TestContext.Current.CancellationToken);

            Assert.Null(result.CorrelationId);
        }
    }

    [Fact]
    public async Task NetworkFailureReturnsClassifiedExceptionWithoutTargetSecret() {
        using var handler = new ThrowingHandler();
        using var sender = new WebhookTeamsMessageSender(new HttpClient(handler), disposeHttpClient: true);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.Ordinal);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task ResponseBodyIoFailureReturnsSanitizedTransientFailure() {
        using var sender = new WebhookTeamsMessageSender(
            new HttpClient(new ThrowingResponseStreamHandler()),
            disposeHttpClient: true);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.Ordinal);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task TransportTimeoutReturnsSanitizedTransientFailure() {
        using var handler = new DelayingHandler();
        using var httpClient = new HttpClient(handler) {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        using var sender = new WebhookTeamsMessageSender(httpClient);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.Ordinal);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task TransportTimeoutCoversStalledResponseBody() {
        using var httpClient = new HttpClient(new StallingResponseHandler()) {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        using var sender = new WebhookTeamsMessageSender(httpClient);
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CallerCancellationPropagatesWithoutTimeoutClassification() {
        using var handler = new DelayingHandler();
        using var httpClient = new HttpClient(handler) {
            Timeout = TimeSpan.FromSeconds(10)
        };
        using var sender = new WebhookTeamsMessageSender(httpClient);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.SendAsync(
            new TeamsMessageRequest { Text = "Build completed" },
            target,
            cancellation.Token));

        Assert.IsNotType<MessageDeliveryException>(exception);
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
        private readonly HttpStatusCode _statusCode;
        private readonly string _correlationId;

        public RecordingHandler(HttpStatusCode statusCode = HttpStatusCode.OK, string correlationId = "request-42") {
            _statusCode = statusCode;
            _correlationId = correlationId;
        }

        public Uri? RequestUri { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            RequestUri = request.RequestUri;
            _ = await request.Content!.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent("accepted", Encoding.UTF8, "text/plain")
            };
            if (_statusCode == HttpStatusCode.TooManyRequests) {
                response.Headers.TryAddWithoutValidation("Retry-After", "30");
                response.Headers.TryAddWithoutValidation("x-ms-request-id", _correlationId);
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

    private sealed class DelayingHandler : HttpMessageHandler {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
