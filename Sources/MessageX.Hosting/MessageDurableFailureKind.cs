namespace MessageX.Hosting;

/// <summary>Safe durable failure category that contains no exception or provider payload text.</summary>
public enum MessageDurableFailureKind {
    /// <summary>No failure was recorded.</summary>
    None = 0,
    /// <summary>The application handler failed unexpectedly.</summary>
    Handler = 1,
    /// <summary>A retryable dependency or provider failure occurred.</summary>
    Transient = 2,
    /// <summary>The work item cannot succeed without a product or configuration change.</summary>
    Permanent = 3
}
