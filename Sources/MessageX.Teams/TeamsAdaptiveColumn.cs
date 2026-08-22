namespace MessageX.Teams;

public sealed class TeamsAdaptiveColumn : TeamsAdaptiveCardElement {
    public override string Type => "Column";

    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? MinimumHeight { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? VerticalContentAlignment { get; set; }
    public string? Spacing { get; set; }
    public string? Style { get; set; }
    public bool? IsVisible { get; set; }
    public bool? Separator { get; set; }
    public TeamsAdaptiveAction? SelectAction { get; set; }
    public List<TeamsAdaptiveCardElement> Items { get; } = new();
}
