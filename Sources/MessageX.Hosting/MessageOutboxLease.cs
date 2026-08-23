namespace MessageX.Hosting;

/// <summary>Owned bounded lease over one durable outbox operation.</summary>
public sealed class MessageOutboxLease {
    /// <summary>Creates an outbox lease returned by a store.</summary>
    public MessageOutboxLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageOutboxRecord record) : this(
            recordId,
            leaseToken,
            leaseExpiresAt,
            attemptCount,
            record,
            null) {
    }

    /// <summary>Creates an outbox lease with a store-authoritative relative duration.</summary>
    public MessageOutboxLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageOutboxRecord record,
        TimeSpan? leaseDuration) {
        RecordId = MessageDurableValidation.RequiredOpaque(recordId, nameof(recordId));
        LeaseToken = MessageDurableValidation.RequiredOpaque(leaseToken, nameof(leaseToken));
        if (attemptCount < 1) {
            throw new ArgumentOutOfRangeException(nameof(attemptCount));
        }
        if (leaseDuration.HasValue && leaseDuration.Value <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        }
        LeaseExpiresAt = leaseExpiresAt;
        LeaseDuration = leaseDuration;
        AttemptCount = attemptCount;
        Record = record ?? throw new ArgumentNullException(nameof(record));
    }

    /// <summary>Stable storage record identifier.</summary>
    public string RecordId { get; }

    /// <summary>Opaque ownership token required for completion or failure.</summary>
    public string LeaseToken { get; }

    /// <summary>Lease expiration in the store's authoritative UTC clock.</summary>
    public DateTimeOffset LeaseExpiresAt { get; }

    /// <summary>
    /// Store-authoritative relative lease duration, when supplied. Workers use this value for local renewal timing
    /// without comparing clocks from different systems.
    /// </summary>
    public TimeSpan? LeaseDuration { get; }

    /// <summary>One-based delivery attempt count.</summary>
    public int AttemptCount { get; }

    /// <summary>Safe outbound operation.</summary>
    public MessageOutboxRecord Record { get; }
}
