namespace TeamsX;

public sealed class TeamsMessageTarget {
    public TeamsDeliveryMethod DeliveryMethod { get; set; }
    public Uri TargetUri { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? AccessToken { get; set; }
    internal Func<CancellationToken, Task<string>>? AccessTokenProvider { get; set; }
    public bool HasDynamicAccessToken => AccessTokenProvider is not null;

    public static TeamsMessageTarget ForIncomingWebhook(Uri uri, string? displayName = null) {
        ValidateUri(uri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.IncomingWebhook,
            TargetUri = uri,
            DisplayName = displayName
        };
    }

    public static TeamsMessageTarget ForWorkflowWebhook(Uri uri, string? displayName = null) {
        ValidateUri(uri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.WorkflowWebhook,
            TargetUri = uri,
            DisplayName = displayName
        };
    }

    public static TeamsMessageTarget ForGraphChannelMessage(
        string teamId,
        string channelId,
        string accessToken,
        string? displayName = null,
        Uri? graphBaseUri = null) {
        ValidateIdentifier(teamId, nameof(teamId));
        ValidateIdentifier(channelId, nameof(channelId));
        ValidateAccessToken(accessToken);

        var baseUri = graphBaseUri ?? new Uri("https://graph.microsoft.com/");
        ValidateUri(baseUri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.GraphChannelMessage,
            TargetUri = new Uri(baseUri, $"v1.0/teams/{Uri.EscapeDataString(teamId)}/channels/{Uri.EscapeDataString(channelId)}/messages"),
            DisplayName = displayName,
            AccessToken = accessToken
        };
    }

    public static TeamsMessageTarget ForGraphChannelMessage(
        string teamId,
        string channelId,
        Func<CancellationToken, Task<string>> accessTokenProvider,
        string? displayName = null,
        Uri? graphBaseUri = null) {
        ValidateIdentifier(teamId, nameof(teamId));
        ValidateIdentifier(channelId, nameof(channelId));
        ValidateAccessTokenProvider(accessTokenProvider);

        var baseUri = graphBaseUri ?? new Uri("https://graph.microsoft.com/");
        ValidateUri(baseUri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.GraphChannelMessage,
            TargetUri = new Uri(baseUri, $"v1.0/teams/{Uri.EscapeDataString(teamId)}/channels/{Uri.EscapeDataString(channelId)}/messages"),
            DisplayName = displayName,
            AccessTokenProvider = accessTokenProvider
        };
    }

    public static TeamsMessageTarget ForGraphChatMessage(
        string chatId,
        string accessToken,
        string? displayName = null,
        Uri? graphBaseUri = null) {
        ValidateIdentifier(chatId, nameof(chatId));
        ValidateAccessToken(accessToken);

        var baseUri = graphBaseUri ?? new Uri("https://graph.microsoft.com/");
        ValidateUri(baseUri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.GraphChatMessage,
            TargetUri = new Uri(baseUri, $"v1.0/chats/{Uri.EscapeDataString(chatId)}/messages"),
            DisplayName = displayName,
            AccessToken = accessToken
        };
    }

    public static TeamsMessageTarget ForGraphChatMessage(
        string chatId,
        Func<CancellationToken, Task<string>> accessTokenProvider,
        string? displayName = null,
        Uri? graphBaseUri = null) {
        ValidateIdentifier(chatId, nameof(chatId));
        ValidateAccessTokenProvider(accessTokenProvider);

        var baseUri = graphBaseUri ?? new Uri("https://graph.microsoft.com/");
        ValidateUri(baseUri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.GraphChatMessage,
            TargetUri = new Uri(baseUri, $"v1.0/chats/{Uri.EscapeDataString(chatId)}/messages"),
            DisplayName = displayName,
            AccessTokenProvider = accessTokenProvider
        };
    }

    private static void ValidateUri(Uri uri) {
        if (uri is null) {
            throw new ArgumentNullException(nameof(uri));
        }

        if (!uri.IsAbsoluteUri) {
            throw new ArgumentException("Target URI must be absolute.", nameof(uri));
        }
    }

    private static void ValidateIdentifier(string value, string parameterName) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }
    }

    private static void ValidateAccessToken(string accessToken) {
        if (string.IsNullOrWhiteSpace(accessToken)) {
            throw new ArgumentException("Access token cannot be null or whitespace.", nameof(accessToken));
        }
    }

    private static void ValidateAccessTokenProvider(Func<CancellationToken, Task<string>> accessTokenProvider) {
        if (accessTokenProvider is null) {
            throw new ArgumentNullException(nameof(accessTokenProvider));
        }
    }
}
