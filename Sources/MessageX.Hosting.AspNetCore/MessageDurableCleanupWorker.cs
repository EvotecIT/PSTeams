using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableCleanupWorker : BackgroundService {
    private readonly IMessageDurableStore _store;
    private readonly MessageDurableStoreInitializer _initializer;
    private readonly MessageXDurableIngressOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly MessageDurableIngressHealth _health;

    public MessageDurableCleanupWorker(
        IMessageDurableStore store,
        MessageDurableStoreInitializer initializer,
        IOptions<MessageXDurableIngressOptions> options,
        TimeProvider timeProvider,
        MessageDurableIngressHealth health) {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _health = health ?? throw new ArgumentNullException(nameof(health));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await _initializer.EnsureInitializedAsync(stoppingToken).ConfigureAwait(false);
                int purged;
                do {
                    purged = await _store.PurgeTerminalAsync(
                        _options.TerminalRetention,
                        _options.CleanupBatchSize,
                        stoppingToken).ConfigureAwait(false);
                    if (purged >= _options.CleanupBatchSize) {
                        await Task.Yield();
                    }
                } while (purged >= _options.CleanupBatchSize);
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                return;
            } catch {
                _health.Unavailable(_timeProvider.GetUtcNow());
            }
            await Task.Delay(_options.CleanupInterval, _timeProvider, stoppingToken).ConfigureAwait(false);
        }
    }
}
