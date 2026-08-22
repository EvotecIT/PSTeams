namespace MessageX.Slack;

/// <summary>A typed Slack webhook or conversation target.</summary>
public sealed class SlackMessageTarget : IProviderCapabilities {
    /// <summary>Delivery transport.</summary>
    public SlackDeliveryMethod DeliveryMethod { get; private set; }

    /// <summary>Secret incoming-webhook URI used only inside the Slack transport assembly.</summary>
    internal Uri? WebhookUri { get; private set; }

    /// <summary>Slack channel, private-channel, DM, multiparty-DM, or user identifier.</summary>
    public string? ConversationId { get; private set; }

    /// <summary>Optional safe display label.</summary>
    public string? DisplayName { get; private set; }

    /// <inheritdoc />
    public MessageCapabilities Capabilities => DeliveryMethod switch {
        SlackDeliveryMethod.IncomingWebhook => MessageCapabilities.Send | MessageCapabilities.Reply,
        SlackDeliveryMethod.WebApi => MessageCapabilities.Send | MessageCapabilities.Reply,
        _ => MessageCapabilities.None
    };

    /// <summary>Creates a fixed-destination incoming-webhook target.</summary>
    public static SlackMessageTarget ForIncomingWebhook(Uri uri, string? displayName = null) {
        ValidateWebhookUri(uri);
        return new SlackMessageTarget {
            DeliveryMethod = SlackDeliveryMethod.IncomingWebhook,
            WebhookUri = uri,
            DisplayName = displayName
        };
    }

    /// <summary>Creates an authenticated Web API conversation target.</summary>
    public static SlackMessageTarget ForConversation(string conversationId, string? displayName = null) {
        var normalized = ValidateConversationId(conversationId);
        return new SlackMessageTarget {
            DeliveryMethod = SlackDeliveryMethod.WebApi,
            ConversationId = normalized,
            DisplayName = displayName
        };
    }

    internal static void ValidateWebhookUri(Uri? uri) {
        if (uri is null) {
            throw new ArgumentNullException(nameof(uri));
        }
        if (!uri.IsAbsoluteUri) {
            throw new ArgumentException("Slack webhook targets must use an official HTTPS Slack webhook URI.", nameof(uri));
        }

        var isOfficialHost = uri.Host == "hooks.slack.com" || uri.Host == "hooks.slack-gov.com";
        var pathSegments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !uri.IsDefaultPort ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !isOfficialHost ||
            pathSegments.Length < 4 ||
            pathSegments[0] != "services") {
            throw new ArgumentException("Slack webhook targets must use an official HTTPS Slack webhook URI.", nameof(uri));
        }
    }

    internal static string ValidateConversationId(string? conversationId) {
        if (!TryNormalizeProviderIdentifier(conversationId, out var normalized)) {
            throw new ArgumentException("A Slack conversation or user identifier is required.", nameof(conversationId));
        }

        var prefix = normalized[0];
        if (prefix is not ('C' or 'G' or 'D' or 'U' or 'W') || normalized.Skip(1).Any(char.IsWhiteSpace)) {
            throw new ArgumentException("Slack targets must use provider identifiers rather than display names.", nameof(conversationId));
        }

        return normalized;
    }

    internal static bool TryNormalizeProviderIdentifier(string? identifier, out string normalized) {
        normalized = identifier?.Trim() ?? string.Empty;
        if (normalized.Length < 2 || normalized.Length > 255 || normalized.Any(char.IsControl)) {
            normalized = string.Empty;
            return false;
        }
        return true;
    }

    internal string SafeLabel() {
        if (!string.IsNullOrWhiteSpace(DisplayName)) {
            return DisplayName!;
        }
        return DeliveryMethod == SlackDeliveryMethod.WebApi
            ? ConversationId ?? "Slack conversation"
            : WebhookUri?.Host ?? "Slack incoming webhook";
    }

    /// <inheritdoc />
    public override string ToString() => SafeLabel();
}
