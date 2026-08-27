using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackExternalFileUploadClientTests {
    [Fact]
    public async Task UploadUsesThreePhaseExternalWorkflowWithoutLeakingAuthorization() {
        var responses = new[] {
            Json(HttpStatusCode.OK, "{\"ok\":true,\"upload_url\":\"https://files.slack.com/upload/v1/ticket-42\",\"file_id\":\"F123ABC456\"}"),
            Text(HttpStatusCode.OK, "OK"),
            Json(HttpStatusCode.OK, "{\"ok\":true,\"files\":[{\"id\":\"F123ABC456\",\"title\":\"Build log\"}]}")
        };
        using var handler = new SequenceHandler(responses);
        using var client = CreateClient(handler);
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("build output"));

        var result = await client.UploadAsync(new SlackFileUploadRequest {
            Content = content,
            Length = content.Length,
            FileName = "build.log",
            Title = "Build log",
            AlternativeText = "Build output",
            ConversationId = "C123ABC456",
            ThreadTimestamp = "1720000000.000100",
            InitialComment = "Attached build output"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("F123ABC456", result.FileId);
        Assert.Equal("C123ABC456", result.ConversationId);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal("Bearer", handler.Requests[0].Authorization?.Scheme);
        Assert.Null(handler.Requests[1].Authorization);
        Assert.Equal("Bearer", handler.Requests[2].Authorization?.Scheme);
        Assert.Equal("https://files.slack.com/upload/v1/ticket-42", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("build output", handler.Requests[1].Body);
        Assert.Contains("\"filename\":\"build.log\"", handler.Requests[0].Body);
        Assert.Contains("\"length\":12", handler.Requests[0].Body);
        Assert.Contains("\"channel_id\":\"C123ABC456\"", handler.Requests[2].Body);
        Assert.Contains("\"thread_ts\":\"1720000000.000100\"", handler.Requests[2].Body);
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task UploadRejectsUntrustedProviderUploadUrlBeforeSendingContent() {
        using var handler = new SequenceHandler(new[] {
            Json(HttpStatusCode.OK, "{\"ok\":true,\"upload_url\":\"https://attacker.example/upload\",\"file_id\":\"F123ABC456\"}")
        });
        using var client = CreateClient(handler);
        using var content = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await client.UploadAsync(new SlackFileUploadRequest {
            Content = content,
            Length = content.Length,
            FileName = "evidence.bin"
        }, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task UploadStopsWhenBinaryTransferFails() {
        using var handler = new SequenceHandler(new[] {
            Json(HttpStatusCode.OK, "{\"ok\":true,\"upload_url\":\"https://files.slack.com/upload/v1/ticket-42\",\"file_id\":\"F123ABC456\"}"),
            Text(HttpStatusCode.ServiceUnavailable, "unavailable")
        });
        using var client = CreateClient(handler);
        using var content = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await client.UploadAsync(new SlackFileUploadRequest {
            Content = content,
            Length = content.Length,
            FileName = "evidence.bin"
        }, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("upload_failed", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task UploadRequiresLengthToMatchSeekableContent() {
        using var client = CreateClient(new SequenceHandler(Array.Empty<HttpResponseMessage>()));
        using var content = new MemoryStream(new byte[] { 1, 2, 3 });

        await Assert.ThrowsAsync<ArgumentException>(() => client.UploadAsync(new SlackFileUploadRequest {
            Content = content,
            Length = 2,
            FileName = "evidence.bin"
        }, TestContext.Current.CancellationToken));
    }

    private static SlackExternalFileUploadClient CreateClient(HttpMessageHandler handler) => new(
        SlackConnection.ForBotToken("xoxb-test-token"),
        new HttpClient(handler),
        disposeHttpClient: true);

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string body) => new(statusCode) {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private static HttpResponseMessage Text(HttpStatusCode statusCode, string body) => new(statusCode) {
        Content = new StringContent(body, Encoding.UTF8, "text/plain")
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
                request.RequestUri!,
                request.Headers.Authorization,
                body));
            return _responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(Uri Uri, AuthenticationHeaderValue? Authorization, string Body);
}
