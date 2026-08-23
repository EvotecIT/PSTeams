namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded handoff for verified typed envelopes.</summary>
public interface IMessageIngressQueue {
    /// <summary>Attempts to enqueue one dispatch-ready receive result.</summary>
    MessageIngressEnqueueStatus TryEnqueue<TProviderPayload>(MessageReceiveResult<TProviderPayload> result);

    /// <summary>Reads accepted work until the queue is completed.</summary>
    IAsyncEnumerable<IMessageIngressWorkItem> ReadAllAsync(CancellationToken cancellationToken);

    /// <summary>Records one successfully dispatched item.</summary>
    void Completed(DateTimeOffset at);

    /// <summary>Records one failed dispatch.</summary>
    void Failed(DateTimeOffset at);

    /// <summary>Stops writers and lets the worker drain already accepted work.</summary>
    void Complete();

    /// <summary>Returns bounded payload-free host health.</summary>
    MessageIngressHealthSnapshot GetHealthSnapshot();
}
