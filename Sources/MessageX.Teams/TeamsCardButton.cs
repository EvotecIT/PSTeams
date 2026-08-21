namespace MessageX.Teams;

/// <summary>
/// Represents a wrapper-card button.
/// </summary>
public sealed class TeamsCardButton {
    public TeamsCardButtonActionType Type { get; set; }
    public string? Title { get; set; }
    public string? Value { get; set; }
    public string? Image { get; set; }
}
