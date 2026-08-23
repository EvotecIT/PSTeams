namespace MessageX.Hosting;

/// <summary>Typed context delivered to one application handler.</summary>
/// <typeparam name="TProviderPayload">Provider-native event payload type.</typeparam>
public sealed class MessageHandlerContext<TProviderPayload> {
    internal MessageHandlerContext(
        MessageRoute route,
        MessageEventEnvelope<TProviderPayload> envelope) {
        Route = route;
        Envelope = envelope;
    }

    /// <summary>Application route selected from the verified event.</summary>
    public MessageRoute Route { get; }

    /// <summary>Verified event metadata and typed provider payload.</summary>
    public MessageEventEnvelope<TProviderPayload> Envelope { get; }
}

/// <summary>Handles a typed verified provider event.</summary>
/// <typeparam name="TProviderPayload">Provider-native event payload type.</typeparam>
/// <param name="context">Typed verified handler context.</param>
/// <param name="cancellationToken">Cancellation requested by the host.</param>
public delegate Task<MessageHandlerResult> MessageEventHandler<TProviderPayload>(
    MessageHandlerContext<TProviderPayload> context,
    CancellationToken cancellationToken);
