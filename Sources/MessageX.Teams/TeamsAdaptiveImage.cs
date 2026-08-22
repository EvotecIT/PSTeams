namespace MessageX.Teams;

public sealed class TeamsAdaptiveImage : TeamsAdaptiveCardElement {
    public override string Type => "Image";

    public string? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Size { get; set; }
    public string? Style { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Height { get; set; }
    public string? Width { get; set; }
    public string? Spacing { get; set; }
    public string? BackgroundColor { get; set; }
    public bool? Separator { get; set; }
    public bool? IsVisible { get; set; }
    public TeamsAdaptiveAction? SelectAction { get; set; }
}
