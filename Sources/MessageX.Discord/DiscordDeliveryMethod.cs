namespace MessageX.Discord;

/// <summary>Discord message delivery transport.</summary>
public enum DiscordDeliveryMethod {
    /// <summary>Execute a token-authenticated incoming webhook.</summary>
    IncomingWebhook,
    /// <summary>Send through a bot to an existing channel or thread channel.</summary>
    BotChannel,
    /// <summary>Send through a bot to an existing thread channel.</summary>
    BotThread,
    /// <summary>Open or reuse a one-to-one DM and send through a bot.</summary>
    BotDirectMessage
}
