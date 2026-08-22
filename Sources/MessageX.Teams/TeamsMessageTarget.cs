namespace MessageX.Teams;

public sealed class TeamsMessageTarget : IProviderCapabilities {
    public TeamsDeliveryMethod DeliveryMethod { get; internal set; }
    internal Uri TargetUri { get; set; } = null!;
    public string? DisplayName { get; internal set; }
    public TeamsWorkflowDestinationKind WorkflowDestination { get; internal set; }
    public MessageCapabilities Capabilities =>
        DeliveryMethod is TeamsDeliveryMethod.IncomingWebhook or TeamsDeliveryMethod.WorkflowWebhook
            ? MessageCapabilities.Send
            : MessageCapabilities.None;

    public static TeamsMessageTarget ForIncomingWebhook(Uri uri, string? displayName = null) {
        ValidateUri(uri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.IncomingWebhook,
            TargetUri = uri,
            DisplayName = displayName
        };
    }

    public static TeamsMessageTarget ForWorkflowWebhook(
        Uri uri,
        string? displayName = null,
        TeamsWorkflowDestinationKind destination = TeamsWorkflowDestinationKind.Unknown) {
        ValidateUri(uri);

        return new TeamsMessageTarget {
            DeliveryMethod = TeamsDeliveryMethod.WorkflowWebhook,
            TargetUri = uri,
            DisplayName = displayName,
            WorkflowDestination = destination
        };
    }

    /// <summary>
    /// Returns the webhook endpoint for a sender implementation. The URI contains a credential and
    /// must not be written to logs, diagnostic output, or delivery results.
    /// </summary>
    public Uri GetWebhookUri() {
        return TargetUri;
    }

    internal static void ValidateUri(Uri uri) {
        if (uri is null) {
            throw new ArgumentNullException(nameof(uri));
        }

        if (!uri.IsAbsoluteUri) {
            throw new ArgumentException("Target URI must be absolute.", nameof(uri));
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException("Teams webhook targets must use HTTPS.", nameof(uri));
        }
    }

    public override string ToString() {
        return string.IsNullOrWhiteSpace(DisplayName)
            ? $"{DeliveryMethod} target at {TargetUri?.Host ?? "unknown host"}"
            : DisplayName!;
    }
}
