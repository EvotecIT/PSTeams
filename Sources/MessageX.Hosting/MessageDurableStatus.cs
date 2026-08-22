namespace MessageX.Hosting;

/// <summary>Durable inbox or outbox processing state.</summary>
public enum MessageDurableStatus {
    /// <summary>Available for processing.</summary>
    Pending = 0,
    /// <summary>Owned by one unexpired processing lease.</summary>
    Leased = 1,
    /// <summary>Processing completed successfully.</summary>
    Completed = 2,
    /// <summary>Processing permanently stopped after policy or attempt exhaustion.</summary>
    DeadLettered = 3
}
