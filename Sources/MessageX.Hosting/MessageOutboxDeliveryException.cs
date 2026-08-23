namespace MessageX.Hosting;

/// <summary>Reports whether a failed outbound delivery is known not to have reached the provider.</summary>
public sealed class MessageOutboxDeliveryException : Exception {
    /// <summary>Creates an outbound delivery failure with an explicit provider outcome.</summary>
    public MessageOutboxDeliveryException(
        string message,
        MessageOutboxDeliveryOutcome outcome,
        Exception? innerException = null)
        : base(message, innerException) {
        if (!Enum.IsDefined(typeof(MessageOutboxDeliveryOutcome), outcome)) {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }
        Outcome = outcome;
    }

    /// <summary>What is known about provider acceptance after the failure.</summary>
    public MessageOutboxDeliveryOutcome Outcome { get; }
}

/// <summary>Delivery knowledge used to prevent unsafe automatic retries.</summary>
public enum MessageOutboxDeliveryOutcome {
    /// <summary>The provider definitely did not accept the operation, so retry is safe.</summary>
    DefinitelyNotSent = 0,

    /// <summary>The provider may have accepted the operation, so automatic retry could duplicate it.</summary>
    Ambiguous = 1
}
