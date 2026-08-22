namespace MessageX.Discord;

/// <summary>Author metadata displayed at the top of a Discord embed.</summary>
public sealed class DiscordEmbedAuthor {
    /// <summary>Author name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional author link.</summary>
    public Uri? Url { get; set; }

    /// <summary>Optional author icon.</summary>
    public Uri? IconUrl { get; set; }
}
