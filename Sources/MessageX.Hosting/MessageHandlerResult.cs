namespace MessageX.Hosting;

/// <summary>Application handler outcome.</summary>
public sealed class MessageHandlerResult {
    private MessageHandlerResult(
        bool handled,
        MessageReference? responseReference,
        MessageAcknowledgement? acknowledgement,
        MessageOutboxBatch? outbox) {
        Handled = handled;
        ResponseReference = responseReference;
        Acknowledgement = acknowledgement;
        Outbox = outbox ?? MessageOutboxBatch.Empty;
    }

    /// <summary>Whether the application accepted responsibility for the event.</summary>
    public bool Handled { get; }

    /// <summary>Safe reference produced by a reply or follow-up operation, when available.</summary>
    public MessageReference? ResponseReference { get; }

    /// <summary>Immediate provider acknowledgement produced by a synchronous handler, when required.</summary>
    public MessageAcknowledgement? Acknowledgement { get; }

    /// <summary>Safe bounded outbound work to commit atomically with durable inbox completion.</summary>
    public MessageOutboxBatch Outbox { get; }

    /// <summary>Creates a completed handler result.</summary>
    public static MessageHandlerResult Completed(MessageReference? responseReference = null) =>
        new(true, responseReference, null, null);

    /// <summary>Creates a completed result with bounded transactional outbound work.</summary>
    public static MessageHandlerResult CompletedWithOutbox(
        MessageOutboxBatch outbox,
        MessageReference? responseReference = null) =>
        new(true, responseReference, null, outbox ?? throw new ArgumentNullException(nameof(outbox)));

    /// <summary>Creates a completed synchronous handler response.</summary>
    public static MessageHandlerResult Respond(MessageAcknowledgement acknowledgement) =>
        new(true, null, acknowledgement ?? throw new ArgumentNullException(nameof(acknowledgement)), null);

    /// <summary>Creates an explicit ignored handler result.</summary>
    public static MessageHandlerResult Ignored() => new(false, null, null, null);
}
