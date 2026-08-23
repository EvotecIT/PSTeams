namespace MessageX.Hosting;

/// <summary>Provider-neutral durable inbox and transactional outbox boundary.</summary>
public interface IMessageDurableStore {
    /// <summary>Creates or upgrades the store schema idempotently.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically accepts a new provider-and-installation-scoped work item or returns its existing state.
    /// Provider, installation, and deduplication coordinates use ordinal, case-sensitive equality.
    /// </summary>
    Task<MessageDurableAcceptance> AcceptInboxAsync(
        MessageDurableRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically and exclusively claims eligible inbox work. Concurrent claimers cannot receive the same active
    /// record. Payload types use ordinal, case-sensitive equality and the lease duration must be strictly positive.
    /// Stores should return the effective relative duration on each lease so workers can schedule renewal without
    /// comparing the store's authoritative clock with the host clock.
    /// </summary>
    Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews one active inbox lease using the store's authoritative clock and a strictly positive duration. Stores
    /// should return the effective relative duration so workers can schedule the next renewal on the host clock.
    /// </summary>
    Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases one owned inbox item without consuming a handler attempt and defers it for another worker.
    /// Use this when the claiming worker cannot route the record. The delay cannot be negative.
    /// </summary>
    Task<bool> ReleaseInboxAsync(
        string recordId,
        string leaseToken,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes one owned inbox item and commits any outbound work in the same transaction. Outbox deduplication
    /// coordinates are unique across the store, not only within one completion batch.
    /// </summary>
    Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        MessageOutboxBatch? outbox = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules another inbox attempt or atomically dead-letters the item. Permanent failures dead-letter
    /// immediately. Handler and transient failures retry only while the current one-based lease attempt is less than
    /// <paramref name="maximumAttempts"/>; equality dead-letters the item.
    /// <see cref="MessageDurableFailureKind.None"/> and undefined values are invalid. The retry delay cannot be negative.
    /// </summary>
    Task<MessageDurableFailureResult> FailInboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically and exclusively claims eligible outbound work. Concurrent claimers cannot receive the same active
    /// record. Payload types use ordinal, case-sensitive equality and the lease duration must be strictly positive.
    /// Stores should return the effective relative duration on each lease so workers can schedule renewal without
    /// comparing the store's authoritative clock with the host clock.
    /// </summary>
    Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews one active outbox lease using the store's authoritative clock and a strictly positive duration. Stores
    /// should return the effective relative duration so workers can schedule the next renewal on the host clock.
    /// </summary>
    Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>Completes one owned outbound operation.</summary>
    Task<bool> CompleteOutboxAsync(
        string recordId,
        string leaseToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules another outbound attempt or atomically dead-letters the operation. Permanent failures dead-letter
    /// immediately. Handler and transient failures retry only while the current one-based lease attempt is less than
    /// <paramref name="maximumAttempts"/>; equality dead-letters the operation.
    /// <see cref="MessageDurableFailureKind.None"/> and undefined values are invalid. The retry delay cannot be negative.
    /// </summary>
    Task<MessageDurableFailureResult> FailOutboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes bounded batches of completed or dead-lettered inbox and outbox records older than the supplied
    /// retention boundary. Pending, leased, and inbox records with retained outbound work must remain untouched.
    /// </summary>
    Task<int> PurgeTerminalAsync(
        DateTimeOffset completedBefore,
        int maximumCount,
        CancellationToken cancellationToken = default);
}
