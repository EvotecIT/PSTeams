namespace MessageX.Hosting;

/// <summary>Application handler outcome.</summary>
public sealed class MessageHandlerResult {
    private MessageHandlerResult(bool handled, MessageReference? responseReference) {
        Handled = handled;
        ResponseReference = responseReference;
    }

    /// <summary>Whether the application accepted responsibility for the event.</summary>
    public bool Handled { get; }

    /// <summary>Safe reference produced by a reply or follow-up operation, when available.</summary>
    public MessageReference? ResponseReference { get; }

    /// <summary>Creates a completed handler result.</summary>
    public static MessageHandlerResult Completed(MessageReference? responseReference = null) =>
        new(true, responseReference);

    /// <summary>Creates an explicit ignored handler result.</summary>
    public static MessageHandlerResult Ignored() => new(false, null);
}
