namespace MessageX.Hosting;

/// <summary>Reports whether a failed outbound delivery is known not to have reached the provider.</summary>
public sealed class MessageOutboxDeliveryException : Exception {
    /// <summary>Creates an outbound delivery failure with an explicit provider outcome.</summary>
    public MessageOutboxDeliveryException(
        string message,
        MessageOutboxDeliveryOutcome outcome,
        Exception? innerException = null,
        TimeSpan? retryAfter = null)
        : base(message, innerException) {
        if (!Enum.IsDefined(typeof(MessageOutboxDeliveryOutcome), outcome)) {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }
        if (retryAfter is { } delay && (delay < TimeSpan.Zero || delay > TimeSpan.FromDays(7))) {
            throw new ArgumentOutOfRangeException(nameof(retryAfter), "Retry delay must be between zero and seven days.");
        }
        Outcome = outcome;
        RetryAfter = retryAfter;
    }

    /// <summary>What is known about provider acceptance after the failure.</summary>
    public MessageOutboxDeliveryOutcome Outcome { get; }

    /// <summary>Minimum provider-directed delay before a known-unsent operation may be retried.</summary>
    /// <remarks>Ignored for ambiguous failures, which remain terminal. The host also applies its configured minimum delay.</remarks>
    public TimeSpan? RetryAfter { get; }
}

/// <summary>Delivery knowledge used to prevent unsafe automatic retries.</summary>
public enum MessageOutboxDeliveryOutcome {
    /// <summary>The provider definitely did not accept the operation, so retry is safe.</summary>
    DefinitelyNotSent = 0,

    /// <summary>The provider may have accepted the operation, so automatic retry could duplicate it.</summary>
    Ambiguous = 1
}
