namespace MessageX.Discord;

/// <summary>Discord message delivery status.</summary>
public sealed class DiscordDeliveryResult : MessageDeliveryResult {
    /// <summary>Creates a Discord delivery result.</summary>
    public DiscordDeliveryResult()
        : base(MessageProviders.Discord) {
    }

    /// <summary>Delivery transport.</summary>
    public DiscordDeliveryMethod DeliveryMethod { get; set; }

    /// <summary>Safe target label or identifier.</summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>Raw provider response for explicit C# diagnostics. PowerShell does not emit it by default.</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Discord rate-limit bucket identifier.</summary>
    public string? RateLimitBucket { get; set; }

    /// <summary>Discord rate-limit scope.</summary>
    public string? RateLimitScope { get; set; }

    /// <summary>Whether Discord reported a global rate limit.</summary>
    public bool IsGlobalRateLimit { get; set; }
}
