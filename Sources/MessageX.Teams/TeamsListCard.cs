namespace MessageX.Teams;

/// <summary>
/// Represents a Teams list card attachment body.
/// </summary>
public sealed class TeamsListCard {
    public string? Title { get; set; }
    public IList<TeamsListCardItem> Items { get; } = new List<TeamsListCardItem>();
    public IList<TeamsCardButton> Buttons { get; } = new List<TeamsCardButton>();
}
