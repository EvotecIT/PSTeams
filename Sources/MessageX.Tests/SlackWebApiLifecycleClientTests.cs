using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackWebApiLifecycleClientTests {
    [Fact]
    public async Task UpdateUsesPersistedCoordinatesAndReturnsManagedReference() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":true,\"channel\":\"C0123456789\",\"ts\":\"1712345678.123456\"}");
        using var client = CreateClient(handler);

        var result = await client.UpdateAsync(
            new SlackMessageRequest { Text = "Deployment completed" },
            CreateReference(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://slack.com/api/chat.update", handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("xoxb-secret-token", handler.AuthorizationParameter);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("C0123456789", payload.RootElement.GetProperty("channel").GetString());
        Assert.Equal("1712345678.123456", payload.RootElement.GetProperty("ts").GetString());
        Assert.Equal("Deployment completed", payload.RootElement.GetProperty("text").GetString());
        Assert.Empty(payload.RootElement.GetProperty("blocks").EnumerateArray());
        Assert.Equal(ManagedCapabilities, result.Reference?.Capabilities);
    }

    [Fact]
    public async Task UpdateEmitsReplacementFieldsAndPreservesCapabilities() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":true,\"channel\":\"C0123456789\",\"ts\":\"1712345678.123456\"}");
        using var client = CreateClient(handler);
        var reference = CreateReference();
        reference.Capabilities = MessageCapabilities.Update | MessageCapabilities.React;
        var message = new SlackMessageRequest();
        message.Blocks.Add(new SlackDividerBlock());

        var result = await client.UpdateAsync(message, reference, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(reference.Capabilities, result.Reference?.Capabilities);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(string.Empty, payload.RootElement.GetProperty("text").GetString());
        Assert.Single(payload.RootElement.GetProperty("blocks").EnumerateArray());
    }

    [Fact]
    public async Task DeleteClearsLifecycleCapabilitiesAfterProviderAcceptance() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = CreateClient(handler);

        var result = await client.DeleteAsync(
            CreateReference(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://slack.com/api/chat.delete", handler.RequestUri?.AbsoluteUri);
        Assert.Equal(MessageCapabilities.None, result.Reference?.Capabilities);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("C0123456789", payload.RootElement.GetProperty("channel").GetString());
        Assert.Equal("1712345678.123456", payload.RootElement.GetProperty("ts").GetString());
    }

    [Theory]
    [InlineData(true, "reactions.add")]
    [InlineData(false, "reactions.remove")]
    public async Task ReactionOperationsUseSlackReactionNames(bool add, string method) {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = CreateClient(handler);

        var result = add
            ? await client.AddReactionAsync(CreateReference(), "white_check_mark", TestContext.Current.CancellationToken)
            : await client.RemoveReactionAsync(CreateReference(), "white_check_mark", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal($"https://slack.com/api/{method}", handler.RequestUri?.AbsoluteUri);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("white_check_mark", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("1712345678.123456", payload.RootElement.GetProperty("timestamp").GetString());
    }

    [Theory]
    [InlineData("thumbsup::skin-tone-2")]
    [InlineData("wave::skin-tone-6")]
    public async Task ReactionOperationsAcceptSlackSkinToneIdentifiers(string reaction) {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = CreateClient(handler);

        var result = await client.AddReactionAsync(
            CreateReference(),
            reaction,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(reaction, payload.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ReactionOperationsPreserveSourceCapabilities() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = CreateClient(handler);
        var reference = CreateReference();
        reference.Capabilities = MessageCapabilities.React;
        reference.ConversationKind = MessageConversationKind.DirectMessage;

        var result = await client.AddReactionAsync(
            reference,
            "eyes",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(MessageCapabilities.React, result.Reference?.Capabilities);
        Assert.Equal(reference.ConversationKind, result.Reference?.ConversationKind);
    }

    [Fact]
    public async Task LifecycleOperationsFailClosedForWrongProviderOrMissingCapability() {
        using var client = CreateClient(new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}"));
        var wrongProvider = new MessageReference(MessageProviders.Discord, "1712345678.123456") {
            ConversationId = "C0123456789",
            Capabilities = MessageCapabilities.Delete
        };
        var sendOnly = CreateReference();
        sendOnly.Capabilities = MessageCapabilities.Reply;

        await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteAsync(
            wrongProvider,
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.DeleteAsync(
            sendOnly,
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.AddReactionAsync(
            CreateReference(),
            ":eyes:",
            TestContext.Current.CancellationToken));
        foreach (var reaction in new[] {
            "eyes::skin-tone-1",
            "eyes::skin-tone-7",
            "eyes::skin-tone-3:extra",
            "eyes::skin-tone-3::skin-tone-4"
        }) {
            await Assert.ThrowsAsync<ArgumentException>(() => client.AddReactionAsync(
                CreateReference(),
                reaction,
                TestContext.Current.CancellationToken));
        }

        var wrongWorkspace = CreateReference();
        wrongWorkspace.ScopeId = "T9999";
        await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteAsync(
            wrongWorkspace,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateRejectsSendOnlyPlacementAndUnfurlOptions() {
        using var client = CreateClient(new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}"));

        await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateAsync(
            new SlackMessageRequest { Text = "Updated", ThreadTimestamp = "1712345678.000001" },
            CreateReference(),
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateAsync(
            new SlackMessageRequest { Text = "Updated", UnfurlLinks = false },
            CreateReference(),
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateAsync(
            new SlackMessageRequest { Text = "Updated", UnfurlMedia = true },
            CreateReference(),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task InvalidUpdateSuccessEnvelopeIsTransientAndHasNoReference() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}");
        using var client = CreateClient(handler);

        var result = await client.UpdateAsync(
            new SlackMessageRequest { Text = "Deployment completed" },
            CreateReference(),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Null(result.Reference);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, MessageErrorKind.RateLimited)]
    [InlineData(HttpStatusCode.Unauthorized, MessageErrorKind.Authentication)]
    public async Task MalformedFailureBodiesRetainHttpClassification(
        HttpStatusCode statusCode,
        MessageErrorKind expectedKind) {
        using var handler = new RecordingHandler(statusCode, "<html>not-json</html>");
        using var client = CreateClient(handler);

        var result = await client.DeleteAsync(
            CreateReference(),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(expectedKind, result.ErrorKind);
    }

    [Fact]
    public async Task NetworkFailuresDoNotRetainBotTokens() {
        using var client = CreateClient(new ThrowingHandler());

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => client.DeleteAsync(
            CreateReference(),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.Transient, exception.Kind);
        Assert.DoesNotContain("secret-token", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    private static SlackWebApiLifecycleClient CreateClient(HttpMessageHandler handler) {
        return new SlackWebApiLifecycleClient(
            SlackConnection.ForBotToken("xoxb-secret-token", workspaceId: "T0123"),
            new HttpClient(handler),
            disposeHttpClient: true);
    }

    private static MessageReference CreateReference() {
        return new MessageReference(MessageProviders.Slack, "1712345678.123456") {
            ScopeId = "T0123",
            ConversationId = "C0123456789",
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(1712345678).AddTicks(1_234_560),
            Capabilities = ManagedCapabilities
        };
    }

    private const MessageCapabilities ManagedCapabilities = MessageCapabilities.Reply |
        MessageCapabilities.Update |
        MessageCapabilities.Delete |
        MessageCapabilities.React;

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody) {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
            response.Headers.TryAddWithoutValidation("x-slack-req-id", "slack-request-42");
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
