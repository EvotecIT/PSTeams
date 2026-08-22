using System.Net.Http;

namespace MessageX.Discord;

internal static class DiscordHttpClientPool {
    internal static HttpClient Shared { get; } = DiscordHttpClientFactory.CreateClient();
}
