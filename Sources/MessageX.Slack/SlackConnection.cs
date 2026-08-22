namespace MessageX.Slack;

/// <summary>An authenticated Slack bot Web API connection.</summary>
public sealed class SlackConnection : IProviderCapabilities {
    private readonly string _botToken;

    private SlackConnection(string botToken, Uri apiBaseUri, string? workspaceId) {
        _botToken = botToken;
        ApiBaseUri = apiBaseUri;
        WorkspaceId = string.IsNullOrWhiteSpace(workspaceId) ? null : workspaceId!.Trim();
    }

    /// <summary>Default Slack Web API base URI.</summary>
    public static Uri DefaultApiBaseUri { get; } = new("https://slack.com/api/");

    /// <summary>Slack or GovSlack Web API base URI.</summary>
    public Uri ApiBaseUri { get; }

    /// <summary>Optional non-secret Slack workspace identifier.</summary>
    public string? WorkspaceId { get; }

    /// <inheritdoc />
    public MessageCapabilities Capabilities => MessageCapabilities.Send |
        MessageCapabilities.Reply |
        MessageCapabilities.Update |
        MessageCapabilities.Delete |
        MessageCapabilities.React |
        MessageCapabilities.ResolveConversation;

    internal string BotToken => _botToken;

    /// <summary>Creates a Slack connection from a bot token.</summary>
    public static SlackConnection ForBotToken(
        string botToken,
        Uri? apiBaseUri = null,
        string? workspaceId = null) {
        var normalizedToken = botToken?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedToken) ||
            (!normalizedToken!.StartsWith("xoxb-", StringComparison.Ordinal) &&
             !normalizedToken.StartsWith("xoxe.xoxb-", StringComparison.Ordinal))) {
            throw new ArgumentException("A Slack bot token is required.", nameof(botToken));
        }

        var normalizedBaseUri = NormalizeApiBaseUri(apiBaseUri ?? DefaultApiBaseUri);
        return new SlackConnection(normalizedToken, normalizedBaseUri, workspaceId);
    }

    /// <inheritdoc />
    public override string ToString() {
        return string.IsNullOrWhiteSpace(WorkspaceId)
            ? $"Slack bot connection at {ApiBaseUri.Host}"
            : $"Slack bot connection for workspace {WorkspaceId}";
    }

    private static Uri NormalizeApiBaseUri(Uri apiBaseUri) {
        if (!apiBaseUri.IsAbsoluteUri ||
            !string.Equals(apiBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !apiBaseUri.IsDefaultPort ||
            !string.IsNullOrEmpty(apiBaseUri.UserInfo) ||
            !string.IsNullOrEmpty(apiBaseUri.Query) ||
            !string.IsNullOrEmpty(apiBaseUri.Fragment) ||
            (apiBaseUri.Host != "slack.com" && apiBaseUri.Host != "slack-gov.com") ||
            apiBaseUri.AbsolutePath.TrimEnd('/') != "/api") {
            throw new ArgumentException(
                "Slack API base URI must be the official HTTPS Slack or GovSlack API endpoint.",
                nameof(apiBaseUri));
        }

        var absolute = apiBaseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? apiBaseUri.AbsoluteUri
            : apiBaseUri.AbsoluteUri + "/";
        return new Uri(absolute, UriKind.Absolute);
    }
}
