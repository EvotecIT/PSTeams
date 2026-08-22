namespace MessageX.Discord;

/// <summary>A name/value field inside a Discord embed.</summary>
public sealed class DiscordEmbedField {
    /// <summary>Field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Field value.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Whether the field may share a row with adjacent fields.</summary>
    public bool Inline { get; set; }
}
