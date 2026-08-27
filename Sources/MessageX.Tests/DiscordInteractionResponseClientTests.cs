using System.Net;
using System.Text;
using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordInteractionResponseClientTests {
    private const string ApplicationId = "123456789012345678";
    private const string Token = "interaction-secret-token-value-123456";

    [Fact]
    public async Task FollowUpReturnsDurableReferenceWithoutExposingTransientToken() {
        using var handler = new SequenceHandler(new[] {
            Json(HttpStatusCode.OK, "{\"id\":\"223456789012345678\",\"channel_id\":\"323456789012345678\",\"timestamp\":\"2026-08-27T12:00:00Z\"}")
        });
        using var client = new DiscordInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);

        var result = await client.FollowUpAsync(
            Context(DateTimeOffset.UtcNow.AddMinutes(10)),
            new DiscordMessageRequest {
                Content = "Approval recorded",
                Components = {
                    new DiscordActionRow {
                        Components = {
                            new DiscordButton { Label = "Details", CustomId = "details" }
                        }
                    }
                }
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("223456789012345678", result.Reference?.MessageId);
        Assert.Equal("323456789012345678", result.Reference?.ConversationId);
        Assert.Equal(MessageCapabilities.Read | MessageCapabilities.Update | MessageCapabilities.Delete,
            result.Reference?.Capabilities);
        Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.EndsWith(
            $"/webhooks/{ApplicationId}/{Token}?wait=true&with_components=true",
            handler.Requests[0].Uri.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Null(handler.Requests[0].Authorization);
        Assert.Contains("\"custom_id\":\"details\"", handler.Requests[0].Body);
        Assert.DoesNotContain(Token, result.Target, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAndDeleteOriginalUseProviderOriginalMessageEndpoint() {
        using var handler = new SequenceHandler(new[] {
            Json(HttpStatusCode.OK, "{\"id\":\"223456789012345678\",\"channel_id\":\"323456789012345678\",\"timestamp\":\"2026-08-27T12:00:00Z\"}"),
            new HttpResponseMessage(HttpStatusCode.NoContent) { Content = new StringContent(string.Empty) }
        });
        using var client = new DiscordInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);
        var context = Context(DateTimeOffset.UtcNow.AddMinutes(10));

        var updated = await client.UpdateOriginalAsync(
            context,
            new DiscordMessageRequest {
                Content = "Updated",
                Components = {
                    new DiscordActionRow {
                        Components = { new DiscordButton { Label = "Approve", CustomId = "approve" } }
                    }
                }
            },
            TestContext.Current.CancellationToken);
        var deleted = await client.DeleteOriginalAsync(
            context,
            TestContext.Current.CancellationToken);

        Assert.True(updated.IsSuccess);
        Assert.True(deleted.IsSuccess);
        Assert.Equal(new HttpMethod("PATCH"), handler.Requests[0].Method);
        Assert.Equal(HttpMethod.Delete, handler.Requests[1].Method);
        Assert.All(handler.Requests, request =>
            Assert.EndsWith("/messages/@original", request.Uri.AbsolutePath, StringComparison.Ordinal));
        Assert.Equal("?with_components=true", handler.Requests[0].Uri.Query);
        Assert.Equal(string.Empty, handler.Requests[1].Uri.Query);
        Assert.All(handler.Requests, request => Assert.Null(request.Authorization));
    }

    [Fact]
    public async Task ExpiredOrDurablyRestoredCapabilitiesAreRejectedBeforeNetworkUse() {
        using var handler = new SequenceHandler(Array.Empty<HttpResponseMessage>());
        using var client = new DiscordInteractionResponseClient(new HttpClient(handler), disposeHttpClient: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.FollowUpAsync(
            Context(DateTimeOffset.UtcNow.AddSeconds(-1)),
            new DiscordMessageRequest { Content = "Too late" },
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.DeleteOriginalAsync(
            DiscordTransientInteractionContext.Unavailable,
            TestContext.Current.CancellationToken));

        Assert.Empty(handler.Requests);
    }

    private static DiscordTransientInteractionContext Context(DateTimeOffset expiresAt) =>
        new(ApplicationId, Token, expiresAt);

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string body) => new(statusCode) {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class SequenceHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;

        public SequenceHandler(IEnumerable<HttpResponseMessage> responses) {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<RecordedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.Authorization,
                body));
            return _responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        Uri Uri,
        System.Net.Http.Headers.AuthenticationHeaderValue? Authorization,
        string Body);
}
