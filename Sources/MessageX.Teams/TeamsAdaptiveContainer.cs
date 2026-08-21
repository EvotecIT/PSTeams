namespace MessageX.Teams;

public sealed class TeamsAdaptiveContainer : TeamsAdaptiveCardElement {
    public override string Type => "Container";

    public string? Id { get; set; }
    public string? Spacing { get; set; }
    public bool? Separator { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Height { get; set; }
    public string? Style { get; set; }
    public string? MinimumHeight { get; set; }
    public bool? Bleed { get; set; }
    public string? VerticalContentAlignment { get; set; }
    public bool? IsVisible { get; set; }
    public TeamsAdaptiveBackgroundImage? BackgroundImage { get; set; }
    public TeamsAdaptiveAction? SelectAction { get; set; }
    public List<TeamsAdaptiveCardElement> Items { get; } = new();
}
