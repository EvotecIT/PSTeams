namespace MessageX.Hosting.AspNetCore;

/// <summary>Payload-free durable inbox health suitable for diagnostics.</summary>
public sealed record MessageDurableIngressHealthSnapshot(
    long Accepted,
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
