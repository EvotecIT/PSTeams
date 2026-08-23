namespace MessageX.Hosting;

/// <summary>Safe failure classification for a rejected provider request.</summary>
public enum MessageReceiveFailureKind {
    /// <summary>The request was accepted.</summary>
    None = 0,
    /// <summary>Authentication or signature verification failed.</summary>
    Unauthorized,
    /// <summary>The signed request fell outside the allowed replay window.</summary>
    Stale,
    /// <summary>The provider envelope was malformed or exceeded supported bounds.</summary>
    Malformed,
    /// <summary>The verified provider event is not supported by this receiver.</summary>
    Unsupported
}
