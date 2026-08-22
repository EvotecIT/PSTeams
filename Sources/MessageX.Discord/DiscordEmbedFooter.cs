namespace MessageX.Discord;

/// <summary>Footer text and icon displayed below a Discord embed.</summary>
public sealed class DiscordEmbedFooter {
    /// <summary>Footer text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional footer icon.</summary>
    public Uri? IconUrl { get; set; }
}
