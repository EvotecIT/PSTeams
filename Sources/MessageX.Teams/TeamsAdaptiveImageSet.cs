namespace MessageX.Teams;

public sealed class TeamsAdaptiveImageSet : TeamsAdaptiveCardElement {
    public override string Type => "ImageSet";

    public string? Id { get; set; }
    public string? ImageSize { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Height { get; set; }
    public string? Spacing { get; set; }
    public bool? Separator { get; set; }
    public bool? IsVisible { get; set; }
    public List<TeamsAdaptiveImage> Images { get; } = new();
}
