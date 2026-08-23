using MessageX.Hosting;

namespace MessageX.Tests;

internal sealed class CoordinatedReleaseDurableStore : DelegatingMessageDurableStore {
    private readonly TaskCompletionSource<bool> _released =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> _resume =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _coordinated;

    public CoordinatedReleaseDurableStore(IMessageDurableStore inner) : base(inner) {
    }

    public Task Released => _released.Task;

    public void Resume() => _resume.TrySetResult(true);

    public override async Task<bool> ReleaseInboxAsync(
        string recordId,
        string leaseToken,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default) {
        var released = await base.ReleaseInboxAsync(
            recordId,
            leaseToken,
            retryDelay,
            cancellationToken).ConfigureAwait(false);
        if (released && Interlocked.CompareExchange(ref _coordinated, 1, 0) == 0) {
            _released.TrySetResult(true);
            await _resume.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        return released;
    }
}

internal abstract class DelegatingMessageDurableStore : IMessageDurableStore {
    private readonly IMessageDurableStore _inner;

    protected DelegatingMessageDurableStore(IMessageDurableStore inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public virtual Task InitializeAsync(CancellationToken cancellationToken = default) =>
        _inner.InitializeAsync(cancellationToken);

    public virtual Task<MessageDurableAcceptance> AcceptInboxAsync(
        MessageDurableRecord record,
        CancellationToken cancellationToken = default) =>
        _inner.AcceptInboxAsync(record, cancellationToken);

    public virtual Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default) =>
        _inner.ClaimInboxAsync(ownerId, maximumCount, leaseDuration, payloadTypes, cancellationToken);

    public virtual Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default) =>
        _inner.RenewInboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);

    public virtual Task<bool> ReleaseInboxAsync(
        string recordId,
        string leaseToken,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default) =>
        _inner.ReleaseInboxAsync(recordId, leaseToken, retryDelay, cancellationToken);

    public virtual Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        MessageOutboxBatch? outbox = null,
        CancellationToken cancellationToken = default) =>
        _inner.CompleteInboxAsync(recordId, leaseToken, outbox, cancellationToken);

    public virtual Task<MessageDurableFailureResult> FailInboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default) =>
        _inner.FailInboxAsync(
            recordId,
            leaseToken,
            failureKind,
            retryDelay,
            maximumAttempts,
            cancellationToken);

    public virtual Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default) =>
        _inner.ClaimOutboxAsync(ownerId, maximumCount, leaseDuration, payloadTypes, cancellationToken);

    public virtual Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default) =>
        _inner.RenewOutboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);

    public virtual Task<bool> CompleteOutboxAsync(
        string recordId,
        string leaseToken,
        CancellationToken cancellationToken = default) =>
        _inner.CompleteOutboxAsync(recordId, leaseToken, cancellationToken);

    public virtual Task<MessageDurableFailureResult> FailOutboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default) =>
        _inner.FailOutboxAsync(
            recordId,
            leaseToken,
            failureKind,
            retryDelay,
            maximumAttempts,
            cancellationToken);

    public virtual Task<int> PurgeTerminalAsync(
        DateTimeOffset completedBefore,
        int maximumCount,
        CancellationToken cancellationToken = default) =>
        _inner.PurgeTerminalAsync(completedBefore, maximumCount, cancellationToken);
}
