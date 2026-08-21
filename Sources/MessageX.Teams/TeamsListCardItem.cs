namespace MessageX.Teams;

/// <summary>
/// Represents one Teams list-card item.
/// </summary>
public sealed class TeamsListCardItem {
    public TeamsListCardItemKind Kind { get; set; }
    public string? Icon { get; set; }
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? TapAction { get; set; }
    public TeamsCardButtonActionType? TapType { get; set; }
    public string? TapValue { get; set; }
}
