namespace MessageX.Teams;

/// <summary>
/// Represents an image entry attached to a connector-card section.
/// </summary>
public sealed class TeamsMessageImage {
    public string? Image { get; set; }
    public bool IsHeroImage { get; set; }
}
