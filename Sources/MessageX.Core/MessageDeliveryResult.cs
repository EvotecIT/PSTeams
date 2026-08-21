namespace MessageX.Core;

/// <summary>
/// Provider-neutral delivery status carried by provider-specific result types.
/// </summary>
public abstract class MessageDeliveryResult {
    private const int MaximumCorrelationIdLength = 128;
    private string? _correlationId;

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

    /// <summary>
    /// Provider or transport correlation identifier. Values that are too long or contain characters
    /// outside a conservative diagnostic-token alphabet are discarded.
    /// </summary>
    public string? CorrelationId {
        get => _correlationId;
        set => _correlationId = SanitizeCorrelationId(value);
    }

    /// <summary>Sanitized error text suitable for diagnostics.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Provider-neutral failure category when the operation was rejected.</summary>
    public MessageErrorKind ErrorKind { get; set; }

    /// <summary>Provider-supplied delay before a retry should be attempted.</summary>
    public TimeSpan? RetryAfter { get; set; }

    private static string? SanitizeCorrelationId(string? value) {
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate!.Length > MaximumCorrelationIdLength) {
            return null;
        }

        foreach (var character in candidate) {
            var isAsciiLetterOrDigit = character is >= 'a' and <= 'z' or
                >= 'A' and <= 'Z' or
                >= '0' and <= '9';
            if (!isAsciiLetterOrDigit && character is not '-' and not '_' and not '.') {
                return null;
            }
        }

        return candidate;
    }
}
