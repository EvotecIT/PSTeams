using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>
/// Short-lived Slack interaction capabilities. These values must not be written to durable event,
/// conversation, or message-reference storage.
/// </summary>
public sealed class SlackTransientInteractionContext {
    private static readonly TimeSpan TriggerLifetime = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ResponseLifetime = TimeSpan.FromMinutes(30);
    internal static SlackTransientInteractionContext Unavailable { get; } =
        new(null, null, DateTimeOffset.MinValue);

    internal SlackTransientInteractionContext(string? triggerId, string? responseUrl)
        : this(triggerId, responseUrl, DateTimeOffset.UtcNow) {
    }

    internal SlackTransientInteractionContext(
        string? triggerId,
        string? responseUrl,
        DateTimeOffset receivedAt) {
        TriggerId = triggerId;
        ResponseUrl = responseUrl;
        TriggerExpiresAt = triggerId is null ? null : receivedAt.Add(TriggerLifetime);
        ResponseExpiresAt = responseUrl is null ? null : receivedAt.Add(ResponseLifetime);
    }

    /// <summary>Short-lived trigger identifier used for immediate interactive operations.</summary>
    [JsonIgnore]
    public string? TriggerId { get; }

    /// <summary>Whether the provider-issued modal trigger is still inside its supported lifetime.</summary>
    [JsonIgnore]
    public bool CanOpenModal => TriggerId is not null && TriggerExpiresAt is not null &&
        DateTimeOffset.UtcNow < TriggerExpiresAt.Value;

    /// <summary>Short-lived response webhook URL. Never persist or log this value.</summary>
    [JsonIgnore]
    public string? ResponseUrl { get; }

    /// <summary>Whether the provider-issued response URL is still inside its supported lifetime.</summary>
    [JsonIgnore]
    public bool CanRespond => ResponseUrl is not null && ResponseExpiresAt is not null &&
        DateTimeOffset.UtcNow < ResponseExpiresAt.Value;

    [JsonIgnore]
    internal DateTimeOffset? TriggerExpiresAt { get; }

    [JsonIgnore]
    internal DateTimeOffset? ResponseExpiresAt { get; }
}
