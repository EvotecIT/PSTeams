using System.Net;
using System.Net.Http;
using System.Text;
using TeamsX;

namespace TeamsX.Tests;

public class GraphTeamsMessageSenderTests {
    [Fact]
    public async Task SendAsyncPostsGraphJsonWithBearerToken() {
        var handler = new RecordingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        using var sender = new GraphTeamsMessageSender(httpClient);

        var target = TeamsMessageTarget.ForGraphChatMessage(
            "19:testchat@thread.v2",
            "token-value",
            "Ops Chat",
            new Uri("https://graph.example.test/"));
        var request = new TeamsMessageRequest {
            Text = "Build failed"
        };

        var result = await sender.SendAsync(request, target);

        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("token-value", handler.AuthorizationToken);
        Assert.Equal("https://graph.example.test/v1.0/chats/19%3Atestchat%40thread.v2/messages", handler.RequestUri);
        Assert.Contains("\"contentType\":\"html\"", handler.Body);
        Assert.Contains("Build failed", handler.Body);
    }

    [Fact]
    public async Task SendJsonAsyncRequiresAccessToken() {
        var handler = new RecordingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        using var sender = new GraphTeamsMessageSender(httpClient);

        var target = new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.GraphChatMessage,
            TargetUri = new Uri("https://graph.example.test/v1.0/chats/abc/messages")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendJsonAsync("{}", target));
    }

    [Fact]
    public async Task SendJsonAsyncUsesDynamicAccessTokenProviderWhenPresent() {
        var handler = new RecordingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        using var sender = new GraphTeamsMessageSender(httpClient);

        var target = TeamsMessageTarget.ForGraphChatMessage(
            "19:testchat@thread.v2",
            _ => Task.FromResult("dynamic-token"),
            "Ops Chat",
            new Uri("https://graph.example.test/"));

        var result = await sender.SendJsonAsync("{\"body\":{\"content\":\"hello\"}}", target);

        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("dynamic-token", handler.AuthorizationToken);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler {
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationToken { get; private set; }
        public string? RequestUri { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationToken = request.Headers.Authorization?.Parameter;
            RequestUri = request.RequestUri?.ToString();
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.Created) {
                Content = new StringContent("{\"id\":\"message-123\"}", Encoding.UTF8, "application/json")
            };
        }
    }
}
