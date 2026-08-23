using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Completes verified HTTP receives through bounded dispatch and exact acknowledgement.</summary>
public sealed class MessageReceiveResultProcessor {
    private readonly IMessageIngressQueue _queue;
    private readonly MessageAcknowledgementWriter _writer;
    private readonly MessageReplayGuard _replayGuard;
    private readonly MessageRouter _router;
    private readonly TimeProvider _timeProvider;
    private readonly MessageSynchronousDispatchGate _synchronousDispatchGate;

    /// <summary>Creates a receive-result processor.</summary>
    public MessageReceiveResultProcessor(
        IMessageIngressQueue queue,
        MessageAcknowledgementWriter writer,
        MessageReplayGuard replayGuard,
        MessageRouter router,
        TimeProvider timeProvider) : this(
            queue,
            writer,
            replayGuard,
            router,
            timeProvider,
            new MessageSynchronousDispatchGate(
                MessageXHostingAspNetCoreOptions.DefaultSynchronousDispatchCapacity)) {
    }

    /// <summary>Creates a receive-result processor with an explicit host-wide synchronous dispatch gate.</summary>
    [ActivatorUtilitiesConstructor]
    public MessageReceiveResultProcessor(
        IMessageIngressQueue queue,
        MessageAcknowledgementWriter writer,
        MessageReplayGuard replayGuard,
        MessageRouter router,
        TimeProvider timeProvider,
        MessageSynchronousDispatchGate synchronousDispatchGate) {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _synchronousDispatchGate = synchronousDispatchGate ??
            throw new ArgumentNullException(nameof(synchronousDispatchGate));
    }

    /// <summary>Writes an acknowledgement after any dispatch-ready envelope is accepted.</summary>
    public async Task ProcessAsync<TProviderPayload>(
        HttpResponse response,
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status == MessageReceiveStatus.DispatchReady) {
            var acceptance = _replayGuard.TryAccept(
                result,
                _timeProvider.GetUtcNow(),
                result.RequiresSynchronousDispatch
                    ? static () => MessageIngressEnqueueStatus.Accepted
                    : () => _queue.TryEnqueue(result));
            if (acceptance == MessageReplayAcceptance.Duplicate) {
                await _writer.WriteAsync(response, result.Acknowledgement, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (acceptance != MessageReplayAcceptance.Accepted) {
                response.Headers.RetryAfter = "1";
                await _writer.WriteAsync(
                    response,
                    MessageAcknowledgement.Empty(StatusCodes.Status503ServiceUnavailable),
                    cancellationToken).ConfigureAwait(false);
                return;
            }
            if (result.RequiresSynchronousDispatch) {
                using var slot = _synchronousDispatchGate.TryEnter();
                if (slot is null) {
                    _replayGuard.Release(result);
                    response.Headers.RetryAfter = "1";
                    await _writer.WriteAsync(
                        response,
                        MessageAcknowledgement.Empty(StatusCodes.Status503ServiceUnavailable),
                        cancellationToken).ConfigureAwait(false);
                    return;
                }
                await ProcessSynchronousAsync(response, result, cancellationToken).ConfigureAwait(false);
                return;
            }
        }
        await _writer.WriteAsync(response, result.Acknowledgement, cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessSynchronousAsync<TProviderPayload>(
        HttpResponse response,
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken) {
        if (result.Route is null || result.Envelope is null) {
            throw new InvalidOperationException("Synchronous dispatch requires a verified route and envelope.");
        }
        try {
            var dispatch = await _router.DispatchAsync(
                result.Route,
                result.Envelope,
                cancellationToken).ConfigureAwait(false);
            var acknowledgement = dispatch.HandlerResult?.Acknowledgement ?? result.Acknowledgement;
            await _writer.WriteAsync(response, acknowledgement, cancellationToken).ConfigureAwait(false);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        } catch {
            await _writer.WriteAsync(
                response,
                MessageAcknowledgement.Empty(StatusCodes.Status500InternalServerError),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
