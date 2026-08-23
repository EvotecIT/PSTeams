namespace MessageX.Hosting;

/// <summary>Owned bounded lease over one durable inbox work item.</summary>
public sealed class MessageDurableLease {
    /// <summary>Creates a durable inbox lease returned by a store.</summary>
    public MessageDurableLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageDurableRecord record) : this(
            recordId,
            leaseToken,
            leaseExpiresAt,
            attemptCount,
            record,
            null) {
    }

    /// <summary>Creates a durable inbox lease with a store-authoritative relative duration.</summary>
    public MessageDurableLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageDurableRecord record,
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

    /// <summary>One-based processing attempt count.</summary>
    public int AttemptCount { get; }

    /// <summary>Safe durable work item.</summary>
    public MessageDurableRecord Record { get; }
}
