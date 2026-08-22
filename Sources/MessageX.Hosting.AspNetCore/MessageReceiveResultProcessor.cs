using Microsoft.AspNetCore.Http;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Completes verified HTTP receives through bounded dispatch and exact acknowledgement.</summary>
public sealed class MessageReceiveResultProcessor {
    private readonly IMessageIngressQueue _queue;
    private readonly MessageAcknowledgementWriter _writer;

    /// <summary>Creates a receive-result processor.</summary>
    public MessageReceiveResultProcessor(
        IMessageIngressQueue queue,
        MessageAcknowledgementWriter writer) {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>Writes an acknowledgement after any dispatch-ready envelope is accepted.</summary>
    public async Task ProcessAsync<TProviderPayload>(
        HttpResponse response,
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status == MessageReceiveStatus.DispatchReady) {
            var enqueue = _queue.TryEnqueue(result);
            if (enqueue != MessageIngressEnqueueStatus.Accepted) {
                response.Headers.RetryAfter = "1";
                await _writer.WriteAsync(
                    response,
                    MessageAcknowledgement.Empty(StatusCodes.Status503ServiceUnavailable),
                    cancellationToken).ConfigureAwait(false);
                return;
            }
        }
        await _writer.WriteAsync(response, result.Acknowledgement, cancellationToken).ConfigureAwait(false);
    }
}
