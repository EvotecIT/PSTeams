namespace TeamsX;

/// <summary>
/// Represents a connector-card action button.
/// </summary>
public sealed class TeamsMessageButton {
    public string? Name { get; set; }
    public string? Link { get; set; }
    public TeamsMessageButtonType ButtonType { get; set; } = TeamsMessageButtonType.ViewAction;
}
