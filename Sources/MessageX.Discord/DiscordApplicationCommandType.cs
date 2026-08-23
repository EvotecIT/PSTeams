namespace MessageX.Discord;

/// <summary>Discord application-command type used as part of exact command routing.</summary>
public enum DiscordApplicationCommandType {
    /// <summary>Chat-input slash command.</summary>
    ChatInput = 1,

    /// <summary>User context-menu command.</summary>
    User = 2,

    /// <summary>Message context-menu command.</summary>
    Message = 3
}
