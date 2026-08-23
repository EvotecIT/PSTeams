using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableIngressWorker : BackgroundService {
    private const int MaximumPayloadTypesPerClaim = 64;
    private readonly IMessageDurableStore _store;
    private readonly MessageDurableStoreInitializer _initializer;
    private readonly IReadOnlyDictionary<string, IMessageDurableDispatchCodec> _codecs;
    private readonly string[] _payloadTypes;
    private readonly MessageRouter _router;
    private readonly MessageXDurableIngressOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly MessageDurableIngressHealth _health;
    private readonly string _ownerId = Guid.NewGuid().ToString("N");
    private int _claimPayloadOffset;

    public MessageDurableIngressWorker(
        IMessageDurableStore store,
        MessageDurableStoreInitializer initializer,
        IEnumerable<IMessageDurableDispatchCodec> codecs,
        MessageRouter router,
        IOptions<MessageXDurableIngressOptions> options,
        TimeProvider timeProvider,
        MessageDurableIngressHealth health) {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        ArgumentNullException.ThrowIfNull(codecs);
        _codecs = codecs.ToDictionary(static codec => codec.PayloadType, StringComparer.Ordinal);
        _payloadTypes = _codecs.Keys.OrderBy(static value => value, StringComparer.Ordinal).ToArray();
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _health = health ?? throw new ArgumentNullException(nameof(health));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await _initializer.EnsureInitializedAsync(stoppingToken).ConfigureAwait(false);
                if (_payloadTypes.Length == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                var leases = await ClaimSupportedInboxAsync(stoppingToken).ConfigureAwait(false);
                _health.Claimed(leases.Count);
                if (leases.Count == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                await Task.WhenAll(leases.Select(lease => ProcessAsync(lease, stoppingToken)))
                    .ConfigureAwait(false);
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                return;
            } catch {
                _health.Unavailable(_timeProvider.GetUtcNow());
                var delay = _options.RetryDelay > TimeSpan.Zero
                    ? _options.RetryDelay
                    : _options.PollInterval;
                await Task.Delay(delay, _timeProvider, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessAsync(MessageDurableLease lease, CancellationToken stoppingToken) {
        if (!_codecs.TryGetValue(lease.Record.PayloadType, out var codec)) {
            _health.Unavailable(_timeProvider.GetUtcNow());
            return;
        }
        using var dispatchCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var renewal = RenewUntilCanceledAsync(lease, dispatchCancellation.Token);
        Task<MessageDispatchResult> dispatch;
        try {
            dispatch = codec.DispatchAsync(lease.Record, _router, dispatchCancellation.Token) ??
                throw new InvalidOperationException("A durable codec returned no dispatch task.");
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            dispatchCancellation.Cancel();
            throw;
        } catch (Exception exception) {
            dispatchCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailAndRecordAsync(
                    lease,
                    exception is MessageDurablePayloadException
                        ? MessageDurableFailureKind.Permanent
                        : MessageDurableFailureKind.Handler,
                    exception is MessageDurablePayloadException ? TimeSpan.Zero : _options.RetryDelay,
                    stoppingToken).ConfigureAwait(false);
            }
            return;
        }

        var first = await Task.WhenAny(dispatch, renewal).ConfigureAwait(false);
        if (ReferenceEquals(first, renewal) && !await renewal.ConfigureAwait(false)) {
            dispatchCancellation.Cancel();
            ObserveAfterLeaseLoss(dispatch);
            return;
        }
        MessageDispatchResult result;
        try {
            result = await dispatch.ConfigureAwait(false);
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            dispatchCancellation.Cancel();
            throw;
        } catch (Exception exception) {
            dispatchCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailAndRecordAsync(
                    lease,
                    exception is MessageDurablePayloadException
                        ? MessageDurableFailureKind.Permanent
                        : MessageDurableFailureKind.Handler,
                    exception is MessageDurablePayloadException ? TimeSpan.Zero : _options.RetryDelay,
                    stoppingToken).ConfigureAwait(false);
            }
            return;
        }
        dispatchCancellation.Cancel();
        if (!await renewal.ConfigureAwait(false)) {
            return;
        }
        var completedAt = _timeProvider.GetUtcNow();
        var completed = await _store.CompleteInboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            result.HandlerResult?.Outbox,
            stoppingToken).ConfigureAwait(false);
        if (completed) {
            _health.Completed(completedAt);
        } else {
            _health.LeaseLost(completedAt);
        }
    }

    private async Task<bool> RenewUntilCanceledAsync(
        MessageDurableLease lease,
        CancellationToken cancellationToken) {
        var leaseExpiresAt = lease.LeaseExpiresAt;
        try {
            while (true) {
                var delay = GetRenewalDelay(leaseExpiresAt);
                if (delay > TimeSpan.Zero) {
                    await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
                }
                var renewed = await _store.RenewInboxLeaseAsync(
                    lease.RecordId,
                    lease.LeaseToken,
                    _options.LeaseDuration,
                    cancellationToken).ConfigureAwait(false);
                if (renewed is null) {
                    _health.LeaseLost(_timeProvider.GetUtcNow());
                    return false;
                }
                leaseExpiresAt = renewed.LeaseExpiresAt;
                _health.LeaseRenewed();
            }
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return true;
        } catch {
            _health.Unavailable(_timeProvider.GetUtcNow());
            return false;
        }
    }

    private async Task<IReadOnlyList<MessageDurableLease>> ClaimSupportedInboxAsync(
        CancellationToken cancellationToken) {
        var leases = new List<MessageDurableLease>(_options.ClaimBatchSize);
        var start = _claimPayloadOffset;
        _claimPayloadOffset = (_claimPayloadOffset + MaximumPayloadTypesPerClaim) % _payloadTypes.Length;
        var orderedPayloadTypes = _payloadTypes
            .Skip(start)
            .Concat(_payloadTypes.Take(start))
            .ToArray();
        for (var offset = 0;
             offset < orderedPayloadTypes.Length && leases.Count < _options.ClaimBatchSize;
             offset += MaximumPayloadTypesPerClaim) {
            var payloadTypes = orderedPayloadTypes
                .Skip(offset)
                .Take(MaximumPayloadTypesPerClaim)
                .ToArray();
            var claimed = await _store.ClaimInboxAsync(
                _ownerId,
                _options.ClaimBatchSize - leases.Count,
                _options.LeaseDuration,
                payloadTypes,
                cancellationToken).ConfigureAwait(false);
            leases.AddRange(claimed);
        }
        return leases;
    }

    private TimeSpan GetRenewalDelay(DateTimeOffset leaseExpiresAt) {
        var remaining = leaseExpiresAt - _timeProvider.GetUtcNow();
        if (remaining <= TimeSpan.FromMilliseconds(100)) {
            return TimeSpan.Zero;
        }
        var configuredInterval = TimeSpan.FromTicks(_options.LeaseDuration.Ticks / 3);
        var expiryInterval = TimeSpan.FromTicks(remaining.Ticks / 3);
        return configuredInterval <= expiryInterval ? configuredInterval : expiryInterval;
    }

    private async Task FailAndRecordAsync(
        MessageDurableLease lease,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        CancellationToken cancellationToken) {
        var at = _timeProvider.GetUtcNow();
        var result = await _store.FailInboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            failureKind,
            retryDelay,
            _options.MaximumAttempts,
            cancellationToken).ConfigureAwait(false);
        switch (result.Status) {
            case MessageDurableFailureStatus.RetryScheduled:
                _health.Retried(at);
                break;
            case MessageDurableFailureStatus.DeadLettered:
                _health.DeadLettered(at);
                break;
            case MessageDurableFailureStatus.LeaseLost:
                _health.LeaseLost(at);
                break;
            default:
                throw new InvalidOperationException("The durable store returned an unsupported failure state.");
        }
    }

    private static void ObserveAfterLeaseLoss(Task dispatch) {
        _ = dispatch.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _health.Stopping();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
