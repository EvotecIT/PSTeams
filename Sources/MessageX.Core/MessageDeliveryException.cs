namespace MessageX.Core;

/// <summary>
/// Represents a classified messaging failure without embedding credentials or request bodies.
/// </summary>
public class MessageDeliveryException : Exception {
    /// <summary>Creates a classified messaging failure.</summary>
    public MessageDeliveryException(
        string message,
        MessageErrorKind kind = MessageErrorKind.Unknown,
        int? statusCode = null,
        string? providerCode = null,
        Exception? innerException = null)
        : base(message, innerException) {
        Kind = kind;
        StatusCode = statusCode;
        ProviderCode = providerCode;
    }

    /// <summary>Provider-neutral failure category.</summary>
    public MessageErrorKind Kind { get; }

    /// <summary>HTTP status code when available.</summary>
    public int? StatusCode { get; }

    /// <summary>Provider-specific error code when available.</summary>
    public string? ProviderCode { get; }
}
