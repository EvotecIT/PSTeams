namespace MessageX.Hosting;

/// <summary>Installation-scoped idempotent acceptance outcome.</summary>
public enum MessageDurableAcceptanceStatus {
    /// <summary>A new durable work item was committed.</summary>
    Accepted = 0,
    /// <summary>The same installation and deduplication key are already pending or leased.</summary>
    AlreadyPending = 1,
    /// <summary>The same installation and deduplication key already completed.</summary>
    AlreadyCompleted = 2,
    /// <summary>The same installation and deduplication key was dead-lettered.</summary>
    DeadLettered = 3
}
