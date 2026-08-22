namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded handoff for verified typed envelopes.</summary>
public interface IMessageIngressQueue {
    /// <summary>Attempts to enqueue one dispatch-ready receive result.</summary>
    MessageIngressEnqueueStatus TryEnqueue<TProviderPayload>(MessageReceiveResult<TProviderPayload> result);

    /// <summary>Returns bounded payload-free host health.</summary>
    MessageIngressHealthSnapshot GetHealthSnapshot();
}
