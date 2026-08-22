using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>
/// Short-lived Slack interaction capabilities. These values must not be written to durable event,
/// conversation, or message-reference storage.
/// </summary>
public sealed class SlackTransientInteractionContext {
    internal SlackTransientInteractionContext(string? triggerId, string? responseUrl) {
        TriggerId = triggerId;
        ResponseUrl = responseUrl;
    }

    /// <summary>Short-lived trigger identifier used for immediate interactive operations.</summary>
    [JsonIgnore]
    public string? TriggerId { get; }

    /// <summary>Short-lived response webhook URL. Never persist or log this value.</summary>
    [JsonIgnore]
    public string? ResponseUrl { get; }
}
