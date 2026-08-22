namespace MessageX.Hosting;

/// <summary>Indicates that durable payload bytes cannot be safely decoded by their owning codec.</summary>
public sealed class MessageDurablePayloadException : Exception {
    /// <summary>Creates a payload decoding failure without provider data.</summary>
    public MessageDurablePayloadException(string message)
        : base(message) {
    }

    /// <summary>Creates a payload decoding failure without provider data.</summary>
    public MessageDurablePayloadException(string message, Exception innerException)
        : base(message, innerException) {
    }
}
