namespace MessageX.Hosting;

/// <summary>Bounded outbound work committed atomically with inbox completion.</summary>
public sealed class MessageOutboxRecord {
    private const int MaximumPayloadBytes = 1024 * 1024;
    private readonly byte[] _payload;

    /// <summary>Creates one transport-neutral outbound operation.</summary>
    public MessageOutboxRecord(
        string provider,
        string installationId,
        string deduplicationKey,
        string operation,
        string payloadType,
        byte[] safePayload,
        DateTimeOffset availableAt) {
        Provider = MessageDurableValidation.Required(provider, nameof(provider));
        InstallationId = MessageDurableValidation.Required(installationId, nameof(installationId));
        DeduplicationKey = MessageDurableValidation.Required(deduplicationKey, nameof(deduplicationKey));
        Operation = MessageDurableValidation.Required(operation, nameof(operation), 128);
        PayloadType = MessageDurableValidation.Required(payloadType, nameof(payloadType));
        if (safePayload is null) {
            throw new ArgumentNullException(nameof(safePayload));
        }
        if (safePayload.Length > MaximumPayloadBytes) {
            throw new ArgumentException("Outbox payload projections cannot exceed 1 MiB.", nameof(safePayload));
        }
        _payload = (byte[])safePayload.Clone();
        AvailableAt = availableAt;
    }

    /// <summary>Stable provider identifier.</summary>
    public string Provider { get; }

    /// <summary>Trusted non-secret installation identifier.</summary>
    public string InstallationId { get; }

    /// <summary>Provider-and-installation-scoped idempotency coordinate.</summary>
    public string DeduplicationKey { get; }

    /// <summary>Codec-owned outbound operation name.</summary>
    public string Operation { get; }

    /// <summary>Stable codec-owned payload discriminator.</summary>
    public string PayloadType { get; }

    /// <summary>Earliest eligible delivery time.</summary>
    public DateTimeOffset AvailableAt { get; }

    /// <summary>Bounded safe-payload length.</summary>
    public int PayloadLength => _payload.Length;

    /// <summary>Returns an independent copy of the outbound projection.</summary>
    public byte[] CopyPayload() => (byte[])_payload.Clone();
}
