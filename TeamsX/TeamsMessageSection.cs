namespace TeamsX;

/// <summary>
/// Represents a connector-card section.
/// </summary>
public sealed class TeamsMessageSection {
    public string? Title { get; set; }
    public string? ActivityTitle { get; set; }
    public string? ActivitySubtitle { get; set; }
    public string? ActivityImage { get; set; }
    public string? ActivityText { get; set; }
    public string? Text { get; set; }
    public bool StartGroup { get; set; }

    public IList<TeamsMessageFact> Facts { get; } = new List<TeamsMessageFact>();
    public IList<TeamsMessageButton> Buttons { get; } = new List<TeamsMessageButton>();
    public IList<string> Images { get; } = new List<string>();
    public IList<string> HeroImages { get; } = new List<string>();
}
