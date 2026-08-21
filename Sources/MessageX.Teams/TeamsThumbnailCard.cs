namespace MessageX.Teams;

/// <summary>
/// Represents a Teams thumbnail card attachment body.
/// </summary>
public sealed class TeamsThumbnailCard {
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Text { get; set; }
    public IList<TeamsCardImage> Images { get; } = new List<TeamsCardImage>();
    public IList<TeamsCardButton> Buttons { get; } = new List<TeamsCardButton>();
}
