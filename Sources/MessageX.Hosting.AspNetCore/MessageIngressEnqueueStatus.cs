namespace MessageX.Hosting.AspNetCore;

/// <summary>Outcome of handing a verified envelope to bounded background dispatch.</summary>
public enum MessageIngressEnqueueStatus {
    /// <summary>The envelope was accepted.</summary>
    Accepted = 0,

    /// <summary>The bounded queue has no available capacity.</summary>
    Full = 1,

    /// <summary>The host is stopping and no longer accepts work.</summary>
    Stopping = 2
}
