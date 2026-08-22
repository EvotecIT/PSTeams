namespace MessageX.Hosting.AspNetCore;

/// <summary>Configures bounded provider-neutral ASP.NET Core ingress.</summary>
public sealed class MessageXHostingAspNetCoreOptions {
    /// <summary>Default maximum exact request-body size.</summary>
    public const int DefaultMaximumRequestBodyBytes = 1024 * 1024;

    /// <summary>Default number of verified envelopes accepted for background dispatch.</summary>
    public const int DefaultQueueCapacity = 256;

    /// <summary>Maximum exact request-body size accepted before provider parsing.</summary>
    public int MaximumRequestBodyBytes { get; set; } = DefaultMaximumRequestBodyBytes;

    /// <summary>Maximum number of verified envelopes waiting for background dispatch.</summary>
    public int QueueCapacity { get; set; } = DefaultQueueCapacity;
}
