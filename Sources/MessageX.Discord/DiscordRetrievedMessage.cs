namespace MessageX.Discord;

/// <summary>A safely parsed Discord message retrieved through an owned webhook.</summary>
public sealed class DiscordRetrievedMessage {
    /// <summary>Durable provider reference.</summary>
    public MessageReference Reference { get; set; } = new(MessageProviders.Discord);

    /// <summary>Message text, when present.</summary>
    public string? Content { get; set; }

    /// <summary>Provider message timestamp, when present.</summary>
    public DateTimeOffset? Timestamp { get; set; }
}
