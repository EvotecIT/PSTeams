namespace MessageX.Hosting;

/// <summary>Safe provider-neutral metadata retained beside a codec-owned durable payload.</summary>
public sealed class MessageDurableEnvelopeMetadata {
    /// <summary>Provider event identifier.</summary>
    public string? EventId { get; set; }

    /// <summary>Tenant, workspace, guild, or equivalent provider scope.</summary>
    public string? ScopeId { get; set; }

    /// <summary>Provider-scoped sender identifier.</summary>
    public string? SenderId { get; set; }

    /// <summary>Conversation coordinates associated with the event.</summary>
    public MessageDurableReference? Conversation { get; set; }

    /// <summary>Message coordinates associated with the event.</summary>
    public MessageDurableReference? Message { get; set; }

    /// <summary>Provider event time.</summary>
    public DateTimeOffset? EventTime { get; set; }

    /// <summary>Safe provider or transport correlation identifier.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Captures the provider-neutral portion of a verified envelope.</summary>
    public static MessageDurableEnvelopeMetadata Capture<TProviderPayload>(
        MessageEventEnvelope<TProviderPayload> envelope) {
        if (envelope is null) {
            throw new ArgumentNullException(nameof(envelope));
        }
        return new MessageDurableEnvelopeMetadata {
            EventId = envelope.EventId,
            ScopeId = envelope.ScopeId,
            SenderId = envelope.SenderId,
            Conversation = MessageDurableReference.Capture(
                envelope.Conversation,
                envelope.Provider,
                envelope.InstallationId),
            Message = MessageDurableReference.Capture(
                envelope.Message,
                envelope.Provider,
                envelope.InstallationId),
            EventTime = envelope.EventTime,
            CorrelationId = envelope.CorrelationId
        };
    }

    /// <summary>Reconstructs a typed envelope from record-owned coordinates and safe metadata.</summary>
    public MessageEventEnvelope<TProviderPayload> Restore<TProviderPayload>(
        MessageDurableRecord record,
        TProviderPayload payload) {
        if (record is null) {
            throw new ArgumentNullException(nameof(record));
        }
        try {
            var envelope = new MessageEventEnvelope<TProviderPayload>(
                record.Provider,
                record.InstallationId,
                record.DeduplicationKey,
                record.Route.EventKind,
                record.ReceivedAt,
                payload) {
                EventId = EventId,
                ScopeId = ScopeId,
                SenderId = SenderId,
                Conversation = Conversation?.Restore(record.Provider, record.InstallationId),
                Message = Message?.Restore(record.Provider, record.InstallationId),
                EventTime = EventTime,
                CorrelationId = CorrelationId
            };
            return envelope;
        }
        catch (ArgumentException exception) {
            throw new MessageDurablePayloadException(
                "Durable envelope metadata contains unsafe or inconsistent coordinates.",
                exception);
        }
    }
}

/// <summary>Serializable safe projection of one message reference.</summary>
public sealed class MessageDurableReference {
    private const int MaximumCoordinateLength = 256;

    /// <summary>Stable provider identifier.</summary>
    public string? Provider { get; set; }

    /// <summary>Non-secret installation identifier.</summary>
    public string? InstallationId { get; set; }

    /// <summary>Provider scope identifier.</summary>
    public string? ScopeId { get; set; }

    /// <summary>Conversation identifier.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Conversation shape.</summary>
    public MessageConversationKind ConversationKind { get; set; }

    /// <summary>Thread identifier.</summary>
    public string? ThreadId { get; set; }

    /// <summary>Message identifier.</summary>
    public string? MessageId { get; set; }

    /// <summary>Safe correlation identifier.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Provider timestamp used as part of identity.</summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>Follow-up capabilities retained with the reference.</summary>
    public MessageCapabilities Capabilities { get; set; }

    internal static MessageDurableReference? Capture(
        MessageReference? reference,
        string provider,
        string installationId) {
        if (reference is null) {
            return null;
        }
        var projection = new MessageDurableReference {
            Provider = reference.Provider,
            InstallationId = reference.InstallationId,
            ScopeId = reference.ScopeId,
            ConversationId = reference.ConversationId,
            ConversationKind = reference.ConversationKind,
            ThreadId = reference.ThreadId,
            MessageId = reference.MessageId,
            CorrelationId = reference.CorrelationId,
            Timestamp = reference.Timestamp,
            Capabilities = reference.Capabilities
        };
        projection.Restore(provider, installationId);
        return projection;
    }

    internal MessageReference Restore(string provider, string installationId) {
        var normalizedProvider = Required(Provider, nameof(Provider));
        if (!string.Equals(normalizedProvider, provider, StringComparison.Ordinal) ||
            (InstallationId is not null &&
             !string.Equals(Required(InstallationId, nameof(InstallationId)), installationId, StringComparison.Ordinal)) ||
            !Enum.IsDefined(typeof(MessageConversationKind), ConversationKind) ||
            (Capabilities & ~AllCapabilities) != 0) {
            throw new ArgumentException("Durable references must match their record and contain known enum values.");
        }

        return new MessageReference(normalizedProvider, Optional(MessageId, nameof(MessageId))) {
            InstallationId = installationId,
            ScopeId = Optional(ScopeId, nameof(ScopeId)),
            ConversationId = Optional(ConversationId, nameof(ConversationId)),
            ConversationKind = ConversationKind,
            ThreadId = Optional(ThreadId, nameof(ThreadId)),
            CorrelationId = Optional(CorrelationId, nameof(CorrelationId)),
            Timestamp = Timestamp,
            Capabilities = Capabilities
        };
    }

    private static readonly MessageCapabilities AllCapabilities =
        Enum.GetValues(typeof(MessageCapabilities))
            .Cast<MessageCapabilities>()
            .Aggregate(MessageCapabilities.None, (current, value) => current | value);

    private static string Required(string? value, string name) =>
        Optional(value, name) ?? throw new ArgumentException("A durable reference coordinate is required.", name);

    private static string? Optional(string? value, string name) {
        if (value is not null && (value.Length > MaximumCoordinateLength || value.Any(char.IsControl))) {
            throw new ArgumentException("Durable reference coordinates must be bounded text without control characters.", name);
        }
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
