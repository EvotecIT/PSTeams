namespace MessageX.Discord;

/// <summary>One option in a Discord string select menu.</summary>
public sealed class DiscordSelectOption {
    /// <summary>User-visible option label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Application-defined option value.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Optional user-visible description.</summary>
    public string? Description { get; set; }

    /// <summary>Whether the option is selected by default.</summary>
    public bool Default { get; set; }
}
