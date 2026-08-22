using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Safe typed projection of the supported fields in a verified Slack Events API event.</summary>
public sealed class SlackEventPayload {
    /// <summary>Creates a safe Slack event projection.</summary>
    [JsonConstructor]
    public SlackEventPayload(
        string type,
        string? subtype,
        string? userId,
        string? channelId,
        string? channelType,
        string? messageTimestamp,
        string? eventTimestamp,
        string? threadTimestamp,
        string? text,
        string? reaction,
        string? itemType) {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Subtype = subtype;
        UserId = userId;
        ChannelId = channelId;
        ChannelType = channelType;
        MessageTimestamp = messageTimestamp;
        EventTimestamp = eventTimestamp;
        ThreadTimestamp = threadTimestamp;
        Text = text;
        Reaction = reaction;
        ItemType = itemType;
    }

    /// <summary>Slack event type.</summary>
    public string Type { get; }

    /// <summary>Slack event subtype, when present.</summary>
    public string? Subtype { get; }

    /// <summary>Slack user coordinate associated with the event.</summary>
    public string? UserId { get; }

    /// <summary>Slack conversation coordinate associated with the affected message.</summary>
    public string? ChannelId { get; }

    /// <summary>Slack channel type when supplied by the provider.</summary>
    public string? ChannelType { get; }

    /// <summary>Timestamp coordinate of the affected message.</summary>
    public string? MessageTimestamp { get; }

    /// <summary>Timestamp coordinate of the callback occurrence.</summary>
    public string? EventTimestamp { get; }

    /// <summary>Root message timestamp when the affected message belongs to a thread.</summary>
    public string? ThreadTimestamp { get; }

    /// <summary>Current message text when supplied by the provider.</summary>
    public string? Text { get; }

    /// <summary>Slack reaction name for reaction callbacks.</summary>
    public string? Reaction { get; }

    /// <summary>Slack reaction item type when supplied by the provider.</summary>
    public string? ItemType { get; }
}
