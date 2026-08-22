namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded payload-free snapshot of the in-memory dispatch pipeline.</summary>
public sealed record MessageIngressHealthSnapshot(
    int Capacity,
    int Queued,
    long Accepted,
    long Completed,
    long Failed,
    bool IsStopping,
    DateTimeOffset? LastCompletedAt,
    DateTimeOffset? LastFailureAt);
