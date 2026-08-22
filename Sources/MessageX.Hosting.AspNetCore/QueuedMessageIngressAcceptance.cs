namespace MessageX.Hosting.AspNetCore;

internal sealed class QueuedMessageIngressAcceptance : IMessageIngressAcceptance {
    private readonly IMessageIngressQueue _queue;
    private readonly MessageReplayGuard _replayGuard;
    private readonly TimeProvider _timeProvider;

    public QueuedMessageIngressAcceptance(
        IMessageIngressQueue queue,
        MessageReplayGuard replayGuard,
        TimeProvider timeProvider) {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var acceptance = _replayGuard.TryAccept(
            result,
            _timeProvider.GetUtcNow(),
            result.RequiresSynchronousDispatch
                ? static () => MessageIngressEnqueueStatus.Accepted
                : () => _queue.TryEnqueue(result));
        return ValueTask.FromResult(acceptance switch {
            MessageReplayAcceptance.Accepted => MessageIngressEnqueueStatus.Accepted,
            MessageReplayAcceptance.Duplicate => MessageIngressEnqueueStatus.Duplicate,
            MessageReplayAcceptance.Full => MessageIngressEnqueueStatus.Full,
            MessageReplayAcceptance.Stopping => MessageIngressEnqueueStatus.Stopping,
            _ => throw new InvalidOperationException("The replay guard returned an unsupported state.")
        });
    }
}
