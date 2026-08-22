namespace MessageX.Hosting;

/// <summary>Bounded safe projection of a verified envelope ready for durable acceptance.</summary>
public sealed class MessageDurableRecord {
    private const int MaximumPayloadBytes = 1024 * 1024;
    private readonly byte[] _payload;

    /// <summary>Creates one installation-scoped durable work item.</summary>
    public MessageDurableRecord(
        string provider,
        string installationId,
        string deduplicationKey,
        MessageRoute route,
        DateTimeOffset receivedAt,
        string payloadType,
        byte[] safePayload) {
        Provider = MessageDurableValidation.Required(provider, nameof(provider));
        InstallationId = MessageDurableValidation.Required(installationId, nameof(installationId));
        DeduplicationKey = MessageDurableValidation.Required(deduplicationKey, nameof(deduplicationKey));
        Route = route ?? throw new ArgumentNullException(nameof(route));
        ReceivedAt = receivedAt;
        PayloadType = MessageDurableValidation.Required(payloadType, nameof(payloadType));
        if (safePayload is null) {
            throw new ArgumentNullException(nameof(safePayload));
        }
        if (safePayload.Length > MaximumPayloadBytes) {
            throw new ArgumentException("Durable payload projections cannot exceed 1 MiB.", nameof(safePayload));
        }
        _payload = (byte[])safePayload.Clone();
    }

    /// <summary>Stable provider identifier.</summary>
    public string Provider { get; }

    /// <summary>Trusted non-secret installation identifier.</summary>
    public string InstallationId { get; }

    /// <summary>Provider-derived idempotency coordinate, scoped by installation.</summary>
    public string DeduplicationKey { get; }

    /// <summary>Validated application route.</summary>
    public MessageRoute Route { get; }

    /// <summary>Verified receive time.</summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>Stable codec-owned payload discriminator.</summary>
    public string PayloadType { get; }

    /// <summary>Bounded safe-payload length.</summary>
    public int PayloadLength => _payload.Length;

    /// <summary>Returns an independent copy of the safe provider projection.</summary>
    public byte[] CopyPayload() => (byte[])_payload.Clone();
}
