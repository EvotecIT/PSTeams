namespace MessageX.Hosting.AspNetCore;

internal interface IMessageDurableDispatchCodec {
    string PayloadType { get; }

    Task<MessageDispatchResult> DispatchAsync(
        MessageDurableRecord record,
        MessageRouter router,
        CancellationToken cancellationToken);
}
