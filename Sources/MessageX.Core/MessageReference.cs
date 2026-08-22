namespace MessageX.Core;

/// <summary>
/// Safe-to-persist coordinates for a provider message and its conversation.
/// </summary>
public sealed class MessageReference : IProviderCapabilities {
    private string? _correlationId;

    /// <summary>Creates a provider message reference.</summary>
    /// <param name="provider">Stable provider identifier.</param>
    /// <param name="messageId">Provider message or activity identifier, when available.</param>
    public MessageReference(string provider, string? messageId = null) {
        if (string.IsNullOrWhiteSpace(provider)) {
            throw new ArgumentException("A provider identifier is required.", nameof(provider));
        }

        Provider = provider.Trim();
        MessageId = string.IsNullOrWhiteSpace(messageId) ? null : messageId;
    }

    /// <summary>Stable provider identifier.</summary>
    public string Provider { get; }

    /// <summary>Non-secret provider installation identifier.</summary>
    public string? InstallationId { get; set; }

    /// <summary>Tenant, workspace, guild, or equivalent provider scope identifier.</summary>
    public string? ScopeId { get; set; }

    /// <summary>Chat, channel, direct-message, or equivalent conversation identifier.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Provider-neutral shape of the referenced conversation.</summary>
    public MessageConversationKind ConversationKind { get; set; }

    /// <summary>Thread or reply-chain identifier.</summary>
    public string? ThreadId { get; set; }

    /// <summary>Provider message or activity identifier.</summary>
    public string? MessageId { get; set; }

    /// <summary>Provider or transport correlation identifier.</summary>
    public string? CorrelationId {
        get => _correlationId;
        set => _correlationId = MessageDiagnosticToken.Normalize(value);
    }

    /// <summary>Provider message timestamp when it is part of the message identity.</summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>Follow-up operations available for this reference.</summary>
    public MessageCapabilities Capabilities { get; set; }
}
