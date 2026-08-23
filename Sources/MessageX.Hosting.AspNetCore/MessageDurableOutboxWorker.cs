using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableOutboxWorker : BackgroundService {
    private const int MaximumPayloadTypesPerClaim = 64;
    private readonly IMessageDurableStore _store;
    private readonly MessageDurableStoreInitializer _initializer;
    private readonly IReadOnlyDictionary<string, IMessageOutboxHandler> _handlers;
    private readonly string[] _payloadTypes;
    private readonly MessageXDurableIngressOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly string _ownerId = Guid.NewGuid().ToString("N");
    private int _claimPayloadOffset;

    public MessageDurableOutboxWorker(
        IMessageDurableStore store,
        MessageDurableStoreInitializer initializer,
        IEnumerable<IMessageOutboxHandler> handlers,
        IOptions<MessageXDurableIngressOptions> options,
        TimeProvider timeProvider) {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = handlers.ToDictionary(static handler => handler.PayloadType, StringComparer.Ordinal);
        _payloadTypes = _handlers.Keys.OrderBy(static value => value, StringComparer.Ordinal).ToArray();
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await _initializer.EnsureInitializedAsync(stoppingToken).ConfigureAwait(false);
                if (_payloadTypes.Length == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                var leases = await ClaimSupportedOutboxAsync(stoppingToken).ConfigureAwait(false);
                if (leases.Count == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                await Task.WhenAll(leases.Select(lease => ProcessAsync(lease, stoppingToken)))
                    .ConfigureAwait(false);
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                break;
            } catch {
                await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessAsync(MessageOutboxLease lease, CancellationToken stoppingToken) {
        if (!_handlers.TryGetValue(lease.Record.PayloadType, out var handler)) {
            return;
        }
        using var deliveryCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var renewal = RenewUntilCanceledAsync(lease, deliveryCancellation.Token);
        await Task.Yield();
        Task delivery;
        try {
            delivery = handler.DeliverAsync(lease.Record, deliveryCancellation.Token) ??
                throw new InvalidOperationException("An outbox handler returned no delivery task.");
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            deliveryCancellation.Cancel();
            throw;
        } catch (Exception exception) {
            deliveryCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailDeliveryAsync(lease, exception, stoppingToken)
                    .ConfigureAwait(false);
            }
            return;
        }
        var first = await Task.WhenAny(delivery, renewal).ConfigureAwait(false);
        if (ReferenceEquals(first, renewal) && !await renewal.ConfigureAwait(false)) {
            deliveryCancellation.Cancel();
            ObserveAfterLeaseLoss(delivery);
            return;
        }
        try {
            await delivery.ConfigureAwait(false);
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            deliveryCancellation.Cancel();
            throw;
        } catch (Exception exception) {
            deliveryCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailDeliveryAsync(lease, exception, stoppingToken)
                    .ConfigureAwait(false);
            }
            return;
        }
        deliveryCancellation.Cancel();
        if (!await renewal.ConfigureAwait(false)) {
            return;
        }
        await _store.CompleteOutboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            stoppingToken).ConfigureAwait(false);
    }

    private async Task<bool> RenewUntilCanceledAsync(
        MessageOutboxLease lease,
        CancellationToken cancellationToken) {
        var leaseExpiresAt = lease.LeaseExpiresAt;
        try {
            while (true) {
                var delay = GetRenewalDelay(leaseExpiresAt);
                if (delay > TimeSpan.Zero) {
                    await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
                }
                var renewed = await _store.RenewOutboxLeaseAsync(
                    lease.RecordId,
                    lease.LeaseToken,
                    _options.LeaseDuration,
                    cancellationToken).ConfigureAwait(false);
                if (renewed is null) {
                    return false;
                }
                leaseExpiresAt = renewed.LeaseExpiresAt;
            }
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return true;
        } catch {
            return false;
        }
    }

    private async Task<IReadOnlyList<MessageOutboxLease>> ClaimSupportedOutboxAsync(
        CancellationToken cancellationToken) {
        var leases = new List<MessageOutboxLease>(_options.ClaimBatchSize);
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
            var claimed = await _store.ClaimOutboxAsync(
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

    private Task<MessageDurableFailureResult> FailAsync(
        MessageOutboxLease lease,
        MessageDurableFailureKind kind,
        TimeSpan retryDelay,
        CancellationToken cancellationToken) =>
        _store.FailOutboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            kind,
            retryDelay,
            _options.MaximumAttempts,
            cancellationToken);

    private Task<MessageDurableFailureResult> FailDeliveryAsync(
        MessageOutboxLease lease,
        Exception exception,
        CancellationToken cancellationToken) {
        var definitelyNotSent = exception is MessageOutboxDeliveryException {
            Outcome: MessageOutboxDeliveryOutcome.DefinitelyNotSent
        };
        return FailAsync(
            lease,
            definitelyNotSent ? MessageDurableFailureKind.Handler : MessageDurableFailureKind.Permanent,
            definitelyNotSent ? _options.RetryDelay : TimeSpan.Zero,
            cancellationToken);
    }

    private static void ObserveAfterLeaseLoss(Task delivery) {
        _ = delivery.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }
}
