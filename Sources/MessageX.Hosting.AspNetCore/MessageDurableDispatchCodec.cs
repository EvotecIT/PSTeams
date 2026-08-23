namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableDispatchCodec<TProviderPayload> : IMessageDurableDispatchCodec {
    private readonly IMessageDurableCodec<TProviderPayload> _codec;

    public MessageDurableDispatchCodec(IMessageDurableCodec<TProviderPayload> codec) =>
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));

    public string PayloadType => _codec.PayloadType;

    public Task<MessageDispatchResult> DispatchAsync(
        MessageDurableRecord record,
        MessageRouter router,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(router);
        if (!string.Equals(record.PayloadType, PayloadType, StringComparison.Ordinal)) {
            throw new InvalidOperationException("The durable payload type does not match its codec.");
        }
        var envelope = _codec.Decode(record);
        MessageDurableCodecGuard.ValidateDecoded(record, envelope);
        return router.DispatchAsync(record.Route, envelope, cancellationToken);
    }
}
