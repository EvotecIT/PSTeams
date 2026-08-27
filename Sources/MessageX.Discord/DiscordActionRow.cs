namespace MessageX.Discord;

/// <summary>A row of compatible Discord interactive components.</summary>
public sealed class DiscordActionRow : DiscordComponent {
    /// <inheritdoc />
    public override int Type => 1;

    /// <summary>Buttons, one select menu, or one modal text input.</summary>
    public IList<DiscordInteractiveComponent> Components { get; } = new List<DiscordInteractiveComponent>();
}
