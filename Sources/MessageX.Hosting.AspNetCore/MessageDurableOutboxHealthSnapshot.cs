namespace MessageX.Hosting.AspNetCore;

/// <summary>Process-local outbox counters; these are not a persisted queue-depth measurement.</summary>
public sealed record MessageDurableOutboxHealthSnapshot(
    long Claimed,
    long Completed,
    long Retried,
    long DeadLettered,
    long LeaseRenewed,
    long LeaseLost,
    long Unavailable,
    bool IsStopping,
    DateTimeOffset? LastCompletedAt,
    DateTimeOffset? LastFailureAt);
