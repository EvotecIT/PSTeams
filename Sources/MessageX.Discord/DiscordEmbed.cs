namespace MessageX.Discord;

/// <summary>A rich Discord embed.</summary>
public sealed class DiscordEmbed {
    /// <summary>Optional embed title.</summary>
    public string? Title { get; set; }

    /// <summary>Optional embed description.</summary>
    public string? Description { get; set; }

    /// <summary>Optional link applied to the title.</summary>
    public Uri? Url { get; set; }

    /// <summary>Optional RGB color encoded as a 24-bit integer.</summary>
    public int? Color { get; set; }

    /// <summary>Optional timestamp displayed by Discord.</summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>Optional author metadata.</summary>
    public DiscordEmbedAuthor? Author { get; set; }

    /// <summary>Optional footer metadata.</summary>
    public DiscordEmbedFooter? Footer { get; set; }

    /// <summary>Optional large embed image.</summary>
    public DiscordEmbedMedia? Image { get; set; }

    /// <summary>Optional compact embed thumbnail.</summary>
    public DiscordEmbedMedia? Thumbnail { get; set; }

    /// <summary>Name/value fields.</summary>
    public IList<DiscordEmbedField> Fields { get; } = new List<DiscordEmbedField>();
}
