namespace MessageX.Hosting.AspNetCore;

/// <summary>Indicates that an inbound provider body exceeded the configured safe bound.</summary>
public sealed class MessageInboundBodyTooLargeException : Exception {
    /// <summary>Creates a body-size rejection without retaining request data.</summary>
    public MessageInboundBodyTooLargeException(int maximumBodyBytes)
        : base($"The inbound request body exceeds the configured {maximumBodyBytes}-byte limit.") {
        MaximumBodyBytes = maximumBodyBytes;
    }

    /// <summary>Configured maximum exact body length.</summary>
    public int MaximumBodyBytes { get; }
}
