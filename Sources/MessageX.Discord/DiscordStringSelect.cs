namespace MessageX.Discord;

/// <summary>A Discord string select menu.</summary>
public sealed class DiscordStringSelect : DiscordInteractiveComponent {
    /// <inheritdoc />
    public override int Type => 3;

    /// <summary>Application-defined identifier.</summary>
    public string CustomId { get; set; } = string.Empty;

    /// <summary>Selectable options.</summary>
    public IList<DiscordSelectOption> Options { get; } = new List<DiscordSelectOption>();

    /// <summary>Placeholder shown before selection.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Minimum selections required.</summary>
    public int MinimumValues { get; set; } = 1;

    /// <summary>Maximum selections allowed.</summary>
    public int MaximumValues { get; set; } = 1;

    /// <summary>Whether the select menu is disabled.</summary>
    public bool Disabled { get; set; }
}
