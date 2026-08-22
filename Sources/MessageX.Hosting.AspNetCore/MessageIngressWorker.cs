using Microsoft.Extensions.Hosting;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageIngressWorker : BackgroundService {
    private readonly IMessageIngressQueue _queue;
    private readonly MessageRouter _router;
    private readonly TimeProvider _timeProvider;

    public MessageIngressWorker(
        IMessageIngressQueue queue,
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
        var execution = ExecuteTask;
        if (execution is null) {
            return;
        }
        try {
            await execution.WaitAsync(cancellationToken).ConfigureAwait(false);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
