namespace MessageX.Hosting;

/// <summary>Provider-neutral durable inbox and transactional outbox boundary.</summary>
public interface IMessageDurableStore {
    /// <summary>Creates or upgrades the store schema idempotently.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically accepts a new provider-and-installation-scoped work item or returns its existing state.</summary>
    Task<MessageDurableAcceptance> AcceptInboxAsync(
        MessageDurableRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>Claims eligible inbox work for explicitly supported payload types using the store's authoritative clock.</summary>
    Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default);

    /// <summary>Renews one active inbox lease using the store's authoritative clock.</summary>
    Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>Completes one owned inbox item and commits any outbound work in the same transaction.</summary>
    Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        MessageOutboxBatch? outbox = null,
        CancellationToken cancellationToken = default);

    /// <summary>Schedules another inbox attempt or atomically dead-letters the item.</summary>
    Task<MessageDurableFailureResult> FailInboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);

    /// <summary>Claims eligible outbound work for explicitly supported payload types using the store's authoritative clock.</summary>
    Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default);

    /// <summary>Renews one active outbox lease using the store's authoritative clock.</summary>
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

    /// <summary>Schedules another outbound attempt or atomically dead-letters the operation.</summary>
    Task<MessageDurableFailureResult> FailOutboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default);
}
