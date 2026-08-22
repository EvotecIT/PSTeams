namespace MessageX.Discord;

/// <summary>Controls which Discord mention forms can notify recipients.</summary>
public sealed class DiscordAllowedMentions {
    /// <summary>A safe mention policy that parses no mentions and does not ping a replied user.</summary>
    public static DiscordAllowedMentions None => new();

    /// <summary>Parses user mention syntax from message content.</summary>
    public bool ParseUsers { get; set; }

    /// <summary>Parses role mention syntax from message content.</summary>
    public bool ParseRoles { get; set; }

    /// <summary>Parses <c>@everyone</c> and <c>@here</c> from message content.</summary>
    public bool ParseEveryone { get; set; }

    /// <summary>Explicit user identifiers allowed to receive mentions.</summary>
    public IList<string> UserIds { get; } = new List<string>();

    /// <summary>Explicit role identifiers allowed to receive mentions.</summary>
    public IList<string> RoleIds { get; } = new List<string>();

    /// <summary>Whether a reply should mention the author of the referenced message.</summary>
    public bool RepliedUser { get; set; }
}
