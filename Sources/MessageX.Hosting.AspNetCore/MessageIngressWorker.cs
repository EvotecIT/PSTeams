using Microsoft.Extensions.Hosting;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageIngressWorker : BackgroundService {
    private readonly MessageIngressQueue _queue;
    private readonly MessageRouter _router;
    private readonly TimeProvider _timeProvider;

    public MessageIngressWorker(
        MessageIngressQueue queue,
        MessageRouter router,
        TimeProvider timeProvider) {
        _queue = queue;
        _router = router;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            await foreach (var item in _queue.ReadAllAsync(stoppingToken).ConfigureAwait(false)) {
                try {
                    await item.DispatchAsync(_router, stoppingToken).ConfigureAwait(false);
                    _queue.Completed(_timeProvider.GetUtcNow());
                } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                    throw;
                } catch {
                    _queue.Failed(_timeProvider.GetUtcNow());
                }
            }
        } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _queue.Complete();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
