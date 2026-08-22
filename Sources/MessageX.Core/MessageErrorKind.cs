namespace MessageX.Core;

/// <summary>
/// Provider-neutral failure categories suitable for retry and diagnostics decisions.
/// </summary>
public enum MessageErrorKind {
    /// <summary>The failure has not been classified.</summary>
    Unknown,
    /// <summary>Authentication credentials are missing or invalid.</summary>
    Authentication,
    /// <summary>The caller or installation lacks permission.</summary>
    Authorization,
    /// <summary>The request or target is invalid.</summary>
    Validation,
    /// <summary>The provider rejected the request because of throttling.</summary>
    RateLimited,
    /// <summary>The requested resource no longer exists.</summary>
    NotFound,
    /// <summary>The provider or network failed temporarily.</summary>
    Transient,
    /// <summary>The operation is unavailable for the selected provider capability.</summary>
    Unsupported
}
