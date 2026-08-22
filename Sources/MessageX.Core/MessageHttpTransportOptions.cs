namespace MessageX.Core;

/// <summary>
/// Reusable HTTP transport settings for provider clients.
/// </summary>
public sealed class MessageHttpTransportOptions {
    /// <summary>Default request timeout used by MessageX HTTP transports.</summary>
    public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(100);

    /// <summary>Gets or sets the request timeout.</summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>Gets or sets an HTTP or HTTPS proxy endpoint.</summary>
    public Uri? ProxyUri { get; set; }

    /// <summary>Gets or sets a product user-agent value sent with requests.</summary>
    public string? UserAgent { get; set; }
}
