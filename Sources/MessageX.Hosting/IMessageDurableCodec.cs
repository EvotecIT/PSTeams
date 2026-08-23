namespace MessageX.Hosting;

/// <summary>Projects one verified provider payload into safe durable bytes and reconstructs it for dispatch.</summary>
/// <typeparam name="TProviderPayload">Provider payload owned by the codec.</typeparam>
public interface IMessageDurableCodec<TProviderPayload> {
    /// <summary>Stable versioned payload discriminator stored with durable work.</summary>
    string PayloadType { get; }

    /// <summary>Creates a bounded durable record without transient capabilities or authentication material.</summary>
    MessageDurableRecord Encode(
        MessageRoute route,
        MessageEventEnvelope<TProviderPayload> envelope);

    /// <summary>Reconstructs a verified typed envelope from a record produced by this codec.</summary>
    MessageEventEnvelope<TProviderPayload> Decode(MessageDurableRecord record);
}
