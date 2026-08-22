namespace MessageX.Hosting.AspNetCore;

/// <summary>Configures durable inbox leasing and bounded retry behavior.</summary>
public sealed class MessageXDurableIngressOptions {
    /// <summary>Maximum inbox items claimed in one storage transaction.</summary>
    public int ClaimBatchSize { get; set; } = 16;

    /// <summary>Time after which an unfinished claim can be recovered by another worker.</summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Delay used when no durable inbox item is available.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Delay before retrying a failed handler.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum handler attempts before an inbox item is dead-lettered.</summary>
    public int MaximumAttempts { get; set; } = 5;
}
