using System.Net.Http;

namespace MessageX.Slack;

internal static class SlackHttpClientPool {
    internal static HttpClient Shared { get; } = MessageHttpClientFactory.CreateClient();
}
