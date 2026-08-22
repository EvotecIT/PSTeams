namespace MessageX.Core;

/// <summary>
/// Provider-neutral delivery status carried by provider-specific result types.
/// </summary>
public abstract class MessageDeliveryResult {
    private string? _correlationId;
    private string? _providerCode;

    /// <summary>Initializes a result for a provider.</summary>
    /// <param name="provider">Stable provider identifier.</param>
    protected MessageDeliveryResult(string provider) {
        if (string.IsNullOrWhiteSpace(provider)) {
            throw new ArgumentException("A provider identifier is required.", nameof(provider));
        }

        Provider = provider.Trim();
    }

    /// <summary>Stable provider identifier.</summary>
    public string Provider { get; }

    /// <summary>Whether the provider accepted the operation.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>HTTP status code when the operation used HTTP.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Safe-to-persist coordinates returned by the provider.</summary>
    public MessageReference? Reference { get; set; }

    /// <summary>Provider-specific error or status code, normalized as a safe diagnostic token.</summary>
    public string? ProviderCode {
        get => _providerCode;
        set => _providerCode = MessageDiagnosticToken.Normalize(value);
    }

    /// <summary>
    /// Provider or transport correlation identifier. Values that are too long or contain characters
    /// outside a conservative diagnostic-token alphabet are discarded.
    /// </summary>
    public string? CorrelationId {
        get => _correlationId;
        set => _correlationId = MessageDiagnosticToken.Normalize(value);
    }

    /// <summary>Sanitized error text suitable for diagnostics.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Provider-neutral failure category when the operation was rejected.</summary>
    public MessageErrorKind ErrorKind { get; set; }

    /// <summary>Provider-supplied delay before a retry should be attempted.</summary>
    public TimeSpan? RetryAfter { get; set; }

}
