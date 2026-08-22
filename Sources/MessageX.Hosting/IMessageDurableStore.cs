namespace MessageX.Hosting;

/// <summary>Provider-neutral durable inbox and transactional outbox boundary.</summary>
public interface IMessageDurableStore {
    /// <summary>Creates or upgrades the store schema idempotently.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically accepts a new installation-scoped work item or returns its existing state.</summary>
    Task<MessageDurableAcceptance> AcceptInboxAsync(
        MessageDurableRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>Claims eligible inbox work, including leases that expired before <paramref name="now"/>.</summary>
    Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>Completes one owned inbox item and commits any outbound work in the same transaction.</summary>
    Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        DateTimeOffset completedAt,
        IReadOnlyList<MessageOutboxRecord>? outbox = null,
        CancellationToken cancellationToken = default);

    /// <summary>Schedules another inbox attempt or atomically dead-letters the item.</summary>
    Task<MessageDurableFailureResult> FailInboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        DateTimeOffset now,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);

    /// <summary>Claims eligible outbound work, including leases that expired before <paramref name="now"/>.</summary>
    Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>Completes one owned outbound operation.</summary>
    Task<bool> CompleteOutboxAsync(
        string recordId,
        string leaseToken,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default);

    /// <summary>Schedules another outbound attempt or atomically dead-letters the operation.</summary>
    Task<MessageDurableFailureResult> FailOutboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        DateTimeOffset now,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);
}
