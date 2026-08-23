namespace MessageX.Hosting.AspNetCore;

/// <summary>Configures bounded provider-neutral ASP.NET Core ingress.</summary>
public sealed class MessageXHostingAspNetCoreOptions {
    /// <summary>Default maximum exact request-body size.</summary>
    public const int DefaultMaximumRequestBodyBytes = 1024 * 1024;

    /// <summary>Default number of verified envelopes accepted for background dispatch.</summary>
    public const int DefaultQueueCapacity = 256;

    /// <summary>Default number of accepted deduplication coordinates retained in memory.</summary>
    public const int DefaultReplayCapacity = 65536;

    /// <summary>Minimum retention that covers every supported provider signature replay window.</summary>
    public static readonly TimeSpan MinimumReplayRetention = TimeSpan.FromHours(1);

    /// <summary>Default retention for accepted in-memory deduplication coordinates.</summary>
    public static readonly TimeSpan DefaultReplayRetention = MinimumReplayRetention;

    /// <summary>Maximum exact request-body size accepted before provider parsing.</summary>
    public int MaximumRequestBodyBytes { get; set; } = DefaultMaximumRequestBodyBytes;

    /// <summary>Maximum number of verified envelopes waiting for background dispatch.</summary>
    public int QueueCapacity { get; set; } = DefaultQueueCapacity;

    /// <summary>Maximum accepted deduplication coordinates retained by in-memory ingress.</summary>
    public int ReplayCapacity { get; set; } = DefaultReplayCapacity;

    /// <summary>Retention for accepted in-memory deduplication coordinates.</summary>
    public TimeSpan ReplayRetention { get; set; } = DefaultReplayRetention;
}
