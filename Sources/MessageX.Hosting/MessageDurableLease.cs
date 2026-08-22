namespace MessageX.Hosting;

/// <summary>Owned bounded lease over one durable inbox work item.</summary>
public sealed class MessageDurableLease {
    /// <summary>Creates a durable inbox lease returned by a store.</summary>
    public MessageDurableLease(
        string recordId,
        string leaseToken,
        DateTimeOffset leaseExpiresAt,
        int attemptCount,
        MessageDurableRecord record) {
        RecordId = MessageDurableValidation.Required(recordId, nameof(recordId));
        LeaseToken = MessageDurableValidation.Required(leaseToken, nameof(leaseToken));
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

    /// <summary>One-based processing attempt count.</summary>
    public int AttemptCount { get; }

    /// <summary>Safe durable work item.</summary>
    public MessageDurableRecord Record { get; }
}
