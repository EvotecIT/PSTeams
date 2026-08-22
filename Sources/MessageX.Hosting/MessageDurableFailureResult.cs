namespace MessageX.Hosting;

/// <summary>Result of atomically recording one leased failure.</summary>
public sealed class MessageDurableFailureResult {
    /// <summary>Creates one failure result.</summary>
    public MessageDurableFailureResult(MessageDurableFailureStatus status) => Status = status;

    /// <summary>Recorded failure outcome.</summary>
    public MessageDurableFailureStatus Status { get; }
}
