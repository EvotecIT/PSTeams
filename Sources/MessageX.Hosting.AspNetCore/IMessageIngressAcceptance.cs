namespace MessageX.Hosting.AspNetCore;

/// <summary>
/// Accepts one verified dispatch-ready result before provider success is returned. Results requiring synchronous
/// dispatch remain inline with process-local replay protection even when the configured acceptance owner is durable.
/// </summary>
public interface IMessageIngressAcceptance {
    /// <summary>Commits or enqueues one verified result.</summary>
    ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken);
}
