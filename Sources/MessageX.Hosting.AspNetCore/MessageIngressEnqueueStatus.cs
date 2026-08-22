namespace MessageX.Hosting.AspNetCore;

/// <summary>Outcome of handing a verified envelope to bounded background dispatch.</summary>
public enum MessageIngressEnqueueStatus {
    /// <summary>The envelope was accepted.</summary>
    Accepted = 0,

    /// <summary>The bounded queue has no available capacity.</summary>
    Full = 1,

    /// <summary>The host is stopping and no longer accepts work.</summary>
    Stopping = 2,

    /// <summary>Durable acceptance or the required safe payload codec is unavailable.</summary>
    Unavailable = 3,

    /// <summary>The same provider, installation, and deduplication coordinate was already accepted.</summary>
    Duplicate = 4
}
