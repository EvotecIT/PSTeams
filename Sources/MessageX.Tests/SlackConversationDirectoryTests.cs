using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackConversationDirectoryTests {
    [Fact]
    public async Task OpensExplicitDirectMessageAndReturnsSafeConversationCoordinates() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":true,\"channel\":{\"id\":\"D0123456789\"}}");
        using var directory = CreateDirectory(handler);

        var result = await directory.OpenDirectMessageAsync(
            new[] { "U0123456789" },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://slack.com/api/conversations.open", handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("U0123456789", payload.RootElement.GetProperty("users").GetString());
        Assert.False(payload.RootElement.GetProperty("prevent_creation").GetBoolean());
        Assert.Equal("D0123456789", result.Reference?.ConversationId);
        Assert.Equal("T0123", result.Reference?.ScopeId);
        Assert.Equal(MessageConversationKind.DirectMessage, result.Reference?.ConversationKind);
        Assert.Equal(MessageCapabilities.Send | MessageCapabilities.Reply, result.Reference?.Capabilities);
        Assert.DoesNotContain("secret", result.Target, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExistingConversationLookupUsesPreventCreationForExplicitUserSet() {
        using var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"ok\":true,\"channel\":{\"id\":\"G0123456789\"}}");
        using var directory = CreateDirectory(handler);

        var result = await directory.OpenDirectMessageAsync(
            new[] { "U0123456789", "W9876543210" },
            preventCreation: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("U0123456789,W9876543210", payload.RootElement.GetProperty("users").GetString());
        Assert.True(payload.RootElement.GetProperty("prevent_creation").GetBoolean());
        Assert.Equal("G0123456789", result.Reference?.ConversationId);
    }

    [Fact]
    public async Task InvalidSuccessEnvelopeDoesNotCreateConversationReference() {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true,\"channel\":{}}");
        using var directory = CreateDirectory(handler);

        var result = await directory.OpenDirectMessageAsync(
            new[] { "U0123456789" },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Null(result.Reference);
    }

    [Theory]
    [MemberData(nameof(InvalidUserSets))]
    public async Task RejectsNonUserBulkOrAmbiguousAddresses(string[] userIds) {
        using var directory = CreateDirectory(new RecordingHandler(HttpStatusCode.OK, "{\"ok\":true}"));

        await Assert.ThrowsAsync<ArgumentException>(() => directory.OpenDirectMessageAsync(
            userIds,
            cancellationToken: TestContext.Current.CancellationToken));
    }

    public static TheoryData<string[]> InvalidUserSets => new() {
        Array.Empty<string>(),
        new[] { "general" },
        new[] { "C0123456789" },
        new[] { "U0123456789", "U0123456789" },
        new[] {
            "U0000000001", "U0000000002", "U0000000003", "U0000000004", "U0000000005",
            "U0000000006", "U0000000007", "U0000000008", "U0000000009"
        }
    };

    private static SlackConversationDirectory CreateDirectory(HttpMessageHandler handler) {
        return new SlackConversationDirectory(
            SlackConnection.ForBotToken("xoxb-secret-token", workspaceId: "T0123"),
            new HttpClient(handler),
            disposeHttpClient: true);
    }

    private sealed class RecordingHandler : HttpMessageHandler {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody) {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public string? AuthorizationScheme { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
