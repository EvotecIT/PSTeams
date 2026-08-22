namespace MessageX.Discord;

/// <summary>An image or thumbnail URL used by a Discord embed.</summary>
public sealed class DiscordEmbedMedia {
    /// <summary>HTTPS media URL.</summary>
    public Uri Url { get; set; } = null!;
}
