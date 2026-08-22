using System.Net;
using System.Net.Http;
using System.Text;
using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordBotMessageSenderTests {
    [Fact]
    public async Task ChannelSendUsesBotAuthAndReturnsDurableReference() {
        using var handler = new QueueHandler(new[] {
            Response(HttpStatusCode.OK, "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\",\"timestamp\":\"2026-08-21T08:00:00Z\"}")
        });
        using var sender = CreateSender(handler);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForChannel("123456789012345678", "223456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bot", request.AuthorizationScheme);
        Assert.Equal("discord-super-secret-token-value", request.AuthorizationParameter);
        Assert.Equal("https://discord.com/api/v10/channels/123456789012345678/messages", request.Uri.AbsoluteUri);
        Assert.Equal("623456789012345678", result.Reference?.MessageId);
        Assert.Equal("223456789012345678", result.Reference?.ScopeId);
        Assert.Null(result.Reference?.ThreadId);
        Assert.Equal(MessageCapabilities.Reply, result.Reference?.Capabilities);
    }

    [Fact]
    public async Task ThreadSendRetainsThreadCoordinateAndReplyReference() {
        using var handler = new QueueHandler(new[] {
            Response(HttpStatusCode.OK, "{\"id\":\"623456789012345678\",\"channel_id\":\"323456789012345678\"}")
        });
        using var sender = CreateSender(handler);
        var message = new DiscordMessageRequest {
            Content = "Reply",
            ReplyToMessageId = " 723456789012345678 ",
            FailIfReplyMissing = false
        };

        var result = await sender.SendAsync(
            message,
            DiscordMessageTarget.ForThread("323456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("323456789012345678", result.Reference?.ThreadId);
        Assert.Contains("\"message_id\":\"723456789012345678\"", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Contains("\"fail_if_not_exists\":false", handler.Requests[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DirectMessageOpensChannelThenCreatesMessage() {
        using var handler = new QueueHandler(new[] {
            Response(HttpStatusCode.OK, "{\"id\":\"823456789012345678\",\"type\":1}"),
            Response(HttpStatusCode.OK, "{\"id\":\"923456789012345678\",\"channel_id\":\"823456789012345678\"}")
        });
        using var sender = CreateSender(handler);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Hello" },
            DiscordMessageTarget.ForDirectMessage("423456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://discord.com/api/v10/users/@me/channels", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Contains("423456789012345678", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Equal("https://discord.com/api/v10/channels/823456789012345678/messages", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("823456789012345678", result.Reference?.ConversationId);
    }

    [Fact]
    public async Task MalformedDirectMessageChannelStopsBeforeSend() {
        using var handler = new QueueHandler(new[] {
            Response(HttpStatusCode.OK, "{\"id\":\"not-a-snowflake\"}")
        });
        using var sender = CreateSender(handler);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Hello" },
            DiscordMessageTarget.ForDirectMessage("423456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Single(handler.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, MessageErrorKind.Authentication)]
    [InlineData(HttpStatusCode.Forbidden, MessageErrorKind.Authorization)]
    [InlineData(HttpStatusCode.NotFound, MessageErrorKind.NotFound)]
    [InlineData(HttpStatusCode.BadGateway, MessageErrorKind.Transient)]
    public async Task HttpErrorsHaveProviderNeutralClassification(HttpStatusCode statusCode, MessageErrorKind expected) {
        using var handler = new QueueHandler(new[] {
            Response(statusCode, "{\"code\":50001,\"message\":\"Missing access\"}")
        });
        using var sender = CreateSender(handler);

        var result = await sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForChannel("123456789012345678"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.ErrorKind);
        Assert.Equal("50001", result.ProviderCode);
        Assert.DoesNotContain("Missing access", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NetworkExceptionDoesNotRetainBotToken() {
        using var sender = new DiscordBotMessageSender(
            DiscordConnection.ForBotToken("discord-super-secret-token-value"),
            new HttpClient(new ThrowingHandler()),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForChannel("123456789012345678"),
            TestContext.Current.CancellationToken));

        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task ResponseBodyIoFailureReturnsSanitizedTransientFailure() {
        using var sender = new DiscordBotMessageSender(
            DiscordConnection.ForBotToken("discord-super-secret-token-value"),
            new HttpClient(new ThrowingResponseStreamHandler()),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForChannel("123456789012345678"),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task CallerCancellationWinsWhenResponseStreamReportsIoFailure() {
        using var sender = new DiscordBotMessageSender(
            DiscordConnection.ForBotToken("discord-super-secret-token-value"),
            new HttpClient(new ThrowingResponseStreamHandler(throwAfterCancellation: true)),
            disposeHttpClient: true);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.SendAsync(
            new DiscordMessageRequest { Content = "Build completed" },
            DiscordMessageTarget.ForChannel("123456789012345678"),
            cancellation.Token));

        Assert.IsNotType<MessageDeliveryException>(exception);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static DiscordBotMessageSender CreateSender(HttpMessageHandler handler) {
        return new DiscordBotMessageSender(
            DiscordConnection.ForBotToken("discord-super-secret-token-value"),
            new HttpClient(handler),
            disposeHttpClient: true);
    }

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body) {
        return new HttpResponseMessage(statusCode) {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private sealed class QueueHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;

        public QueueHandler(IEnumerable<HttpResponseMessage> responses) {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<RecordedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Requests.Add(new RecordedRequest(
                request.RequestUri!,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                await request.Content!.ReadAsStringAsync(cancellationToken)));
            return _responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(
        Uri Uri,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        string Body);

    private sealed class ThrowingHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            throw new HttpRequestException($"Connection failed with {request.Headers.Authorization}");
        }
    }
}
