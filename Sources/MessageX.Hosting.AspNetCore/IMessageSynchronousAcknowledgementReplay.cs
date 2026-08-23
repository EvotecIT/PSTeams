namespace MessageX.Hosting.AspNetCore;

/// <summary>Publishes and replays the acknowledgement produced by one accepted synchronous dispatch.</summary>
public interface IMessageSynchronousAcknowledgementReplay {
    /// <summary>Waits for the original accepted dispatch to publish its acknowledgement.</summary>
    ValueTask<MessageAcknowledgement> WaitForAcknowledgementAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken);

    /// <summary>Publishes the acknowledgement for later or concurrent duplicates.</summary>
    void Complete<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        MessageAcknowledgement acknowledgement);
}
