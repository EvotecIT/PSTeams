namespace MessageX.Discord;

/// <summary>A text input used inside a Discord modal action row.</summary>
public sealed class DiscordTextInput : DiscordInteractiveComponent {
    /// <inheritdoc />
    public override int Type => 4;

    /// <summary>Application-defined identifier.</summary>
    public string CustomId { get; set; } = string.Empty;

    /// <summary>User-visible field label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Input layout style.</summary>
    public DiscordTextInputStyle Style { get; set; } = DiscordTextInputStyle.Short;

    /// <summary>Minimum accepted length.</summary>
    public int? MinimumLength { get; set; }

    /// <summary>Maximum accepted length.</summary>
    public int? MaximumLength { get; set; }

    /// <summary>Whether the user must provide a value.</summary>
    public bool Required { get; set; } = true;

    /// <summary>Prepopulated value.</summary>
    public string? Value { get; set; }

    /// <summary>Placeholder shown for an empty input.</summary>
    public string? Placeholder { get; set; }
}
