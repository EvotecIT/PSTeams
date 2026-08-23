namespace MessageX.Hosting.AspNetCore;

/// <summary>Configures bounded provider-neutral ASP.NET Core ingress.</summary>
public sealed class MessageXHostingAspNetCoreOptions {
    /// <summary>Default maximum exact request-body size.</summary>
    public const int DefaultMaximumRequestBodyBytes = 1024 * 1024;

    /// <summary>Default number of verified envelopes accepted for background dispatch.</summary>
    public const int DefaultQueueCapacity = 256;

    /// <summary>Default number of provider requests allowed to wait for synchronous handler responses.</summary>
    public const int DefaultSynchronousDispatchCapacity = 64;

    /// <summary>Largest configurable synchronous dispatch set.</summary>
    public const int MaximumSynchronousDispatchCapacity = 65536;

    /// <summary>Default number of accepted deduplication coordinates retained in memory.</summary>
    public const int DefaultReplayCapacity = 65536;

    /// <summary>Largest configurable in-memory replay set.</summary>
    public const int MaximumReplayCapacity = 10_000_000;

    /// <summary>Default aggregate acknowledgement-body bytes retained for synchronous replay.</summary>
    public const int DefaultReplayAcknowledgementBodyBytes = 16 * 1024 * 1024;

    /// <summary>Largest configurable aggregate acknowledgement-body replay budget.</summary>
    public const int MaximumReplayAcknowledgementBodyBytes = 256 * 1024 * 1024;

    /// <summary>Minimum retention that covers every supported provider signature replay window.</summary>
    public static readonly TimeSpan MinimumReplayRetention = TimeSpan.FromHours(1);

    /// <summary>Default retention for accepted in-memory deduplication coordinates.</summary>
    public static readonly TimeSpan DefaultReplayRetention = MinimumReplayRetention;

    /// <summary>Maximum exact request-body size accepted before provider parsing.</summary>
    public int MaximumRequestBodyBytes { get; set; } = DefaultMaximumRequestBodyBytes;

    /// <summary>Maximum number of verified envelopes waiting for background dispatch.</summary>
    public int QueueCapacity { get; set; } = DefaultQueueCapacity;

    /// <summary>Maximum concurrent verified requests whose acknowledgement requires synchronous dispatch.</summary>
    public int SynchronousDispatchCapacity { get; set; } = DefaultSynchronousDispatchCapacity;

    /// <summary>
    /// Maximum accepted deduplication coordinates retained by in-memory ingress. Size this for the
    /// peak verified request rate across all installations multiplied by <see cref="ReplayRetention"/>.
    /// </summary>
    public int ReplayCapacity { get; set; } = DefaultReplayCapacity;

    /// <summary>
    /// Aggregate response-body bytes retained for exact replay of completed synchronous dispatches.
    /// When this budget is full, the original response still succeeds while duplicates receive a
    /// retryable response and a later request may dispatch again.
    /// </summary>
    public int ReplayAcknowledgementBodyBytes { get; set; } = DefaultReplayAcknowledgementBodyBytes;

    /// <summary>Retention for accepted in-memory deduplication coordinates.</summary>
    public TimeSpan ReplayRetention { get; set; } = DefaultReplayRetention;
}
