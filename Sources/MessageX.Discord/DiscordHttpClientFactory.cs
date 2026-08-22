using System.Net.Http;

namespace MessageX.Discord;

internal static class DiscordHttpClientFactory {
    private const string ProjectUri = "https://github.com/EvotecIT/PSTeams";

    internal static string DefaultUserAgent {
        get {
            var version = typeof(DiscordClient).Assembly.GetName().Version?.ToString(3) ?? "0.1.0";
            return $"DiscordBot ({ProjectUri}, {version})";
        }
    }

    internal static HttpClient CreateClient(MessageHttpTransportOptions? options = null) {
        options ??= new MessageHttpTransportOptions();
        return MessageHttpClientFactory.CreateClient(new MessageHttpTransportOptions {
            ProxyUri = options.ProxyUri,
            Timeout = options.Timeout,
            UserAgent = string.IsNullOrWhiteSpace(options.UserAgent)
                ? DefaultUserAgent
                : options.UserAgent
        });
    }
}
