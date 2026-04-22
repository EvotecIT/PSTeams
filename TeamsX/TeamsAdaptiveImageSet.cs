namespace TeamsX;

public sealed class TeamsAdaptiveImageSet : TeamsAdaptiveCardElement {
    public override string Type => "ImageSet";

    public string? ImageSize { get; set; }
    public List<TeamsAdaptiveImage> Images { get; } = new();
}
