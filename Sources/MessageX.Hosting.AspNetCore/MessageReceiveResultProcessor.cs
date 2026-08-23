using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Completes verified HTTP receives through bounded dispatch and exact acknowledgement.</summary>
public sealed class MessageReceiveResultProcessor {
    private readonly IMessageIngressAcceptance _acceptance;
    private readonly MessageAcknowledgementWriter _writer;
    private readonly MessageRouter _router;
    private readonly MessageReplayGuard _replayGuard;
    private readonly MessageSynchronousDispatchGate _synchronousDispatchGate;

    /// <summary>Creates a receive-result processor.</summary>
    public MessageReceiveResultProcessor(
        IMessageIngressAcceptance acceptance,
        MessageAcknowledgementWriter writer,
        MessageRouter router,
        MessageReplayGuard replayGuard) : this(
            acceptance,
            writer,
            router,
            replayGuard,
            new MessageSynchronousDispatchGate(
                MessageXHostingAspNetCoreOptions.DefaultSynchronousDispatchCapacity)) {
    }

    /// <summary>Creates a receive-result processor with an explicit host-wide synchronous dispatch gate.</summary>
    [ActivatorUtilitiesConstructor]
    public MessageReceiveResultProcessor(
        IMessageIngressAcceptance acceptance,
        MessageAcknowledgementWriter writer,
        MessageRouter router,
        MessageReplayGuard replayGuard,
        MessageSynchronousDispatchGate synchronousDispatchGate) {
        _acceptance = acceptance ?? throw new ArgumentNullException(nameof(acceptance));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
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
            var acceptance = await _acceptance.AcceptAsync(result, cancellationToken).ConfigureAwait(false);
            if (acceptance is not (MessageIngressEnqueueStatus.Accepted or MessageIngressEnqueueStatus.Duplicate)) {
                response.Headers.RetryAfter = "1";
                await _writer.WriteAsync(
                    response,
                    MessageAcknowledgement.Empty(StatusCodes.Status503ServiceUnavailable),
                    cancellationToken).ConfigureAwait(false);
                return;
            }
            if (acceptance == MessageIngressEnqueueStatus.Duplicate) {
                var acknowledgement = result.Acknowledgement;
                if (result.RequiresSynchronousDispatch &&
                    _acceptance is IMessageSynchronousAcknowledgementReplay replay) {
                    acknowledgement = await replay.WaitForAcknowledgementAsync(result, cancellationToken)
                        .ConfigureAwait(false);
                }
                await _writer.WriteAsync(response, acknowledgement, cancellationToken).ConfigureAwait(false);
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
        MessageAcknowledgement acknowledgement;
        try {
            var dispatch = await _router.DispatchAsync(result.Route, result.Envelope, cancellationToken)
                .ConfigureAwait(false);
            acknowledgement = dispatch.HandlerResult?.Acknowledgement ?? result.Acknowledgement;
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            _replayGuard.Release(result);
            throw;
        } catch {
            _replayGuard.Release(result);
            await _writer.WriteAsync(
                response,
                MessageAcknowledgement.Empty(StatusCodes.Status500InternalServerError),
                cancellationToken).ConfigureAwait(false);
            return;
        }
        if (_acceptance is IMessageSynchronousAcknowledgementReplay replay) {
            replay.Complete(result, acknowledgement);
        }
        await _writer.WriteAsync(response, acknowledgement, cancellationToken).ConfigureAwait(false);
    }
}
