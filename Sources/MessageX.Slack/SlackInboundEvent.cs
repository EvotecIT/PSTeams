using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Verified provider-native Slack Events API callback payload.</summary>
public sealed class SlackInboundEvent {
    internal SlackInboundEvent(
        string eventType,
        JsonElement providerEvent,
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

    /// <summary>Exact parsed provider event object, independent from the request document lifetime.</summary>
    public JsonElement ProviderEvent { get; }

    /// <summary>Message text when supplied by the event.</summary>
    public string? Text { get; }

    /// <summary>Slack delivery retry number, when present.</summary>
    public int? RetryNumber { get; }

    /// <summary>Safe Slack retry reason, when present.</summary>
    public string? RetryReason { get; }
}
