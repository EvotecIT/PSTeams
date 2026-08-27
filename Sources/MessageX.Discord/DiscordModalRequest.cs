namespace MessageX.Discord;

/// <summary>A modal returned as the immediate response to a Discord interaction.</summary>
public sealed class DiscordModalRequest {
    /// <summary>Application-defined modal identifier.</summary>
    public string CustomId { get; set; } = string.Empty;

    /// <summary>User-visible modal title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>One to five action rows containing one text input each.</summary>
    public IList<DiscordActionRow> Components { get; } = new List<DiscordActionRow>();
}
