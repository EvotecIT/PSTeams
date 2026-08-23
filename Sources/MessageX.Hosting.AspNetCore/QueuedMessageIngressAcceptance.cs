namespace MessageX.Hosting.AspNetCore;

internal sealed class QueuedMessageIngressAcceptance :
    IMessageIngressAcceptance,
    IMessageIngressReservationRelease,
    IMessageSynchronousAcknowledgementReplay,
    IMessageSynchronousDispatchGate {
    private readonly IMessageIngressQueue _queue;
    private readonly MessageReplayGuard _replayGuard;
    private readonly TimeProvider _timeProvider;
    private readonly MessageSynchronousDispatchGate _synchronousDispatchGate;

    public QueuedMessageIngressAcceptance(
        IMessageIngressQueue queue,
        MessageReplayGuard replayGuard,
        TimeProvider timeProvider,
        MessageSynchronousDispatchGate synchronousDispatchGate) {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _synchronousDispatchGate = synchronousDispatchGate ??
            throw new ArgumentNullException(nameof(synchronousDispatchGate));
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
            MessageReplayAcceptance.Unavailable => MessageIngressEnqueueStatus.Unavailable,
            _ => throw new InvalidOperationException("The replay guard returned an unsupported state.")
        });
    }

    public void Release<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) =>
        _replayGuard.Release(result);

    public ValueTask<MessageAcknowledgement> WaitForAcknowledgementAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken) =>
        _replayGuard.WaitForAcknowledgementAsync(result, cancellationToken);

    public void Complete<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        MessageAcknowledgement acknowledgement) =>
        _replayGuard.Complete(result, acknowledgement);

    public IDisposable? TryEnterSynchronousDispatch() => _synchronousDispatchGate.TryEnter();
}
