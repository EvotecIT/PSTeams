using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableOutboxWorker : BackgroundService {
    private readonly IMessageDurableStore _store;
    private readonly MessageDurableStoreInitializer _initializer;
    private readonly IReadOnlyDictionary<string, IMessageOutboxHandler> _handlers;
    private readonly string[] _payloadTypes;
    private readonly MessageXDurableIngressOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly string _ownerId = Guid.NewGuid().ToString("N");

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
        await _initializer.EnsureInitializedAsync(stoppingToken).ConfigureAwait(false);
        try {
            while (!stoppingToken.IsCancellationRequested) {
                if (_payloadTypes.Length == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                var leases = await _store.ClaimOutboxAsync(
                    _ownerId,
                    _options.ClaimBatchSize,
                    _options.LeaseDuration,
                    _payloadTypes,
                    stoppingToken).ConfigureAwait(false);
                if (leases.Count == 0) {
                    await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                await Task.WhenAll(leases.Select(lease => ProcessAsync(lease, stoppingToken)))
                    .ConfigureAwait(false);
            }
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
        }
    }

    private async Task ProcessAsync(MessageOutboxLease lease, CancellationToken stoppingToken) {
        if (!_handlers.TryGetValue(lease.Record.PayloadType, out var handler)) {
            return;
        }
        using var deliveryCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var renewal = RenewUntilCanceledAsync(lease, deliveryCancellation.Token);
        Task delivery;
        try {
            delivery = handler.DeliverAsync(lease.Record, deliveryCancellation.Token) ??
                throw new InvalidOperationException("An outbox handler returned no delivery task.");
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            deliveryCancellation.Cancel();
            throw;
        } catch {
            deliveryCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailAsync(lease, MessageDurableFailureKind.Handler, _options.RetryDelay, stoppingToken)
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
        } catch {
            deliveryCancellation.Cancel();
            if (await renewal.ConfigureAwait(false)) {
                await FailAsync(lease, MessageDurableFailureKind.Handler, _options.RetryDelay, stoppingToken)
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
        var interval = TimeSpan.FromTicks(Math.Max(
            TimeSpan.FromMilliseconds(100).Ticks,
            _options.LeaseDuration.Ticks / 3));
        try {
            while (true) {
                await Task.Delay(interval, _timeProvider, cancellationToken).ConfigureAwait(false);
                var renewed = await _store.RenewOutboxLeaseAsync(
                    lease.RecordId,
                    lease.LeaseToken,
                    _options.LeaseDuration,
                    cancellationToken).ConfigureAwait(false);
                if (renewed is null) {
                    return false;
                }
            }
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return true;
        } catch {
            return false;
        }
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

    private static void ObserveAfterLeaseLoss(Task delivery) {
        _ = delivery.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }
}
