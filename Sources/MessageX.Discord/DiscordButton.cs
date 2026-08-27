namespace MessageX.Discord;

/// <summary>An interactive or link Discord button.</summary>
public sealed class DiscordButton : DiscordInteractiveComponent {
    /// <inheritdoc />
    public override int Type => 2;

    /// <summary>Button style.</summary>
    public DiscordButtonStyle Style { get; set; } = DiscordButtonStyle.Primary;

    /// <summary>Application-defined identifier for non-link buttons.</summary>
    public string? CustomId { get; set; }

    /// <summary>Visible button label.</summary>
    public string? Label { get; set; }

    /// <summary>External target for link buttons.</summary>
    public Uri? Url { get; set; }

    /// <summary>Whether the button is disabled.</summary>
    public bool Disabled { get; set; }
}
