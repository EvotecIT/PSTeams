namespace MessageX.Hosting;

/// <summary>Owned bounded lease over one durable outbox operation.</summary>
public sealed class MessageOutboxLease {
    /// <summary>Creates an outbox lease returned by a store.</summary>
    public MessageOutboxLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageOutboxRecord record) {
        RecordId = MessageDurableValidation.RequiredOpaque(recordId, nameof(recordId));
        LeaseToken = MessageDurableValidation.RequiredOpaque(leaseToken, nameof(leaseToken));
        if (attemptCount < 1) {
            throw new ArgumentOutOfRangeException(nameof(attemptCount));
        }
        LeaseExpiresAt = leaseExpiresAt;
        AttemptCount = attemptCount;
        Record = record ?? throw new ArgumentNullException(nameof(record));
    }

    /// <summary>Stable storage record identifier.</summary>
    public string RecordId { get; }

    /// <summary>Opaque ownership token required for completion or failure.</summary>
    public string LeaseToken { get; }

    /// <summary>Lease expiration in UTC.</summary>
    public DateTimeOffset LeaseExpiresAt { get; }

    /// <summary>One-based delivery attempt count.</summary>
    public int AttemptCount { get; }

    /// <summary>Safe outbound operation.</summary>
    public MessageOutboxRecord Record { get; }
}
