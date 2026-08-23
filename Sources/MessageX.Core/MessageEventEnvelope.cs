namespace MessageX.Core;

/// <summary>
/// Verified, safe-to-persist routing metadata and a typed provider payload for one inbound event.
/// </summary>
/// <typeparam name="TProviderPayload">Provider-native event payload type.</typeparam>
public sealed class MessageEventEnvelope<TProviderPayload> {
    private const int MaximumCoordinateLength = 256;
    private string? _eventId;
    private string? _scopeId;
    private string? _senderId;
    private string? _correlationId;

    /// <summary>Creates a verified inbound event envelope.</summary>
    /// <param name="provider">Stable provider identifier.</param>
    /// <param name="installationId">Non-secret installation selected by the trusted host route.</param>
    /// <param name="deduplicationKey">Stable provider-derived idempotency key.</param>
    /// <param name="kind">Provider-neutral event classification.</param>
    /// <param name="receivedAt">Time at which the verified host received the event.</param>
    /// <param name="payload">Typed provider payload.</param>
    public MessageEventEnvelope(
        string provider,
        string installationId,
        string deduplicationKey,
        MessageEventKind kind,
        DateTimeOffset receivedAt,
        TProviderPayload payload) {
        Provider = NormalizeRequired(provider, nameof(provider));
        InstallationId = NormalizeRequired(installationId, nameof(installationId));
        DeduplicationKey = NormalizeRequired(deduplicationKey, nameof(deduplicationKey));
        Kind = kind;
        ReceivedAt = receivedAt;
        Payload = payload is null ? throw new ArgumentNullException(nameof(payload)) : payload;
    }

    /// <summary>Stable provider identifier.</summary>
    public string Provider { get; }

    /// <summary>Non-secret installation identifier selected outside the untrusted payload.</summary>
    public string InstallationId { get; }

    /// <summary>Stable idempotency key used for duplicate suppression.</summary>
    public string DeduplicationKey { get; }

    /// <summary>Provider-neutral event classification.</summary>
    public MessageEventKind Kind { get; }

    /// <summary>Time at which the verified host received the event.</summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>Typed provider-native payload.</summary>
    public TProviderPayload Payload { get; }

    /// <summary>Provider event identifier, when distinct from the deduplication key.</summary>
    public string? EventId {
        get => _eventId;
        set => _eventId = NormalizeOptional(value, nameof(EventId));
    }

    /// <summary>Tenant, workspace, guild, or equivalent verified provider scope.</summary>
    public string? ScopeId {
        get => _scopeId;
        set => _scopeId = NormalizeOptional(value, nameof(ScopeId));
    }

    /// <summary>Provider-scoped sender identifier.</summary>
    public string? SenderId {
        get => _senderId;
        set => _senderId = NormalizeOptional(value, nameof(SenderId));
    }

    /// <summary>Conversation coordinates associated with the event.</summary>
    public MessageReference? Conversation { get; set; }

    /// <summary>Message coordinates associated with the event.</summary>
    public MessageReference? Message { get; set; }

    /// <summary>Provider event time, when supplied and verified.</summary>
    public DateTimeOffset? EventTime { get; set; }

    /// <summary>Safe provider or transport correlation identifier.</summary>
    public string? CorrelationId {
        get => _correlationId;
        set => _correlationId = MessageDiagnosticToken.Normalize(value);
    }

    private static string NormalizeRequired(string? value, string parameterName) {
        var normalized = NormalizeOptional(value, parameterName);
        return normalized ?? throw new ArgumentException("A non-empty event coordinate is required.", parameterName);
    }

    private static string? NormalizeOptional(string? value, string parameterName) {
        if (value is not null &&
            (value.Length > MaximumCoordinateLength || value.Any(char.IsControl))) {
            throw new ArgumentException("Event coordinates must be bounded text without control characters.", parameterName);
        }
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized)) {
            return null;
        }
        return normalized;
    }
}
