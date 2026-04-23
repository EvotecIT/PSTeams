namespace TeamsX;

public sealed class TeamsAdaptiveTextBlock : TeamsAdaptiveCardElement {
    public override string Type => "TextBlock";

    public string Text { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string? Spacing { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Size { get; set; }
    public string? Weight { get; set; }
    public string? Color { get; set; }
    public string? Height { get; set; }
    public string? FontType { get; set; }
    public bool? Highlight { get; set; }
    public bool? Italic { get; set; }
    public bool? StrikeThrough { get; set; }
    public int? MaximumLines { get; set; }
    public bool? Separator { get; set; }
    public bool? Wrap { get; set; }
    public bool? Subtle { get; set; }
    public bool? IsVisible { get; set; }
}
