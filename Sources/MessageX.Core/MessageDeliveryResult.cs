namespace MessageX.Core;

/// <summary>
/// Provider-neutral delivery status carried by provider-specific result types.
/// </summary>
public abstract class MessageDeliveryResult {
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

    /// <summary>Provider-specific error or status code.</summary>
    public string? ProviderCode { get; set; }

    /// <summary>Provider or transport correlation identifier.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Sanitized error text suitable for diagnostics.</summary>
    public string? ErrorMessage { get; set; }
}
