namespace MessageX.Teams;

/// <summary>
/// Represents a Teams hero card attachment body.
/// </summary>
public sealed class TeamsHeroCard {
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Text { get; set; }
    public IList<TeamsCardImage> Images { get; } = new List<TeamsCardImage>();
    public IList<TeamsCardButton> Buttons { get; } = new List<TeamsCardButton>();
}
