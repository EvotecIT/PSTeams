namespace MessageX.Hosting.AspNetCore;

/// <summary>Accepts one verified dispatch-ready result before provider success is returned.</summary>
public interface IMessageIngressAcceptance {
    /// <summary>Commits or enqueues one verified result.</summary>
    ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken);
}
