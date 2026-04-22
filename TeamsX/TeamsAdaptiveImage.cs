namespace TeamsX;

public sealed class TeamsAdaptiveImage : TeamsAdaptiveCardElement {
    public override string Type => "Image";

    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Size { get; set; }
}
