namespace TeamsX;

public sealed class TeamsAdaptiveRichTextBlock : TeamsAdaptiveCardElement {
    public override string Type => "RichTextBlock";

    public string? Id { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Height { get; set; }
    public string? Spacing { get; set; }
    public bool? Separator { get; set; }
    public bool? IsVisible { get; set; }
    public List<TeamsAdaptiveTextRun> Inlines { get; } = new();
}
