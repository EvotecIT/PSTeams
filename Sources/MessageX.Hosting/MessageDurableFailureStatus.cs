namespace MessageX.Hosting;

/// <summary>Outcome of recording a leased work-item failure.</summary>
public enum MessageDurableFailureStatus {
    /// <summary>The work item was rescheduled for another attempt.</summary>
    RetryScheduled = 0,
    /// <summary>The work item reached a terminal dead-letter state.</summary>
    DeadLettered = 1,
    /// <summary>The supplied lease no longer owns the work item.</summary>
    LeaseLost = 2
}
