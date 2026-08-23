using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Verified provider-native Slack Events API callback payload.</summary>
public sealed class SlackInboundEvent {
    /// <summary>Creates a verified Slack inbound-event value or rehydrates its safe persisted projection.</summary>
    [JsonConstructor]
    public SlackInboundEvent(
        string eventType,
        SlackEventPayload providerEvent,
        string? text,
        string? retryReason,
        int? retryNumber) {
        EventType = eventType;
        ProviderEvent = providerEvent;
        Text = text;
        RetryReason = retryReason;
        RetryNumber = retryNumber;
    }

    /// <summary>Slack event type.</summary>
    public string EventType { get; }

    /// <summary>Safe typed projection of the supported provider event fields.</summary>
    public SlackEventPayload ProviderEvent { get; }

    /// <summary>Message text when supplied by the event.</summary>
    public string? Text { get; }

    /// <summary>Slack delivery retry number, when present.</summary>
    public int? RetryNumber { get; }

    /// <summary>Safe Slack retry reason, when present.</summary>
    public string? RetryReason { get; }
}
