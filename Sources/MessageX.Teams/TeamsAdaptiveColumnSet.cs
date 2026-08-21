namespace MessageX.Teams;

public sealed class TeamsAdaptiveColumnSet : TeamsAdaptiveCardElement {
    public override string Type => "ColumnSet";

    public string? Style { get; set; }
    public string? MinimumHeight { get; set; }
    public bool? Bleed { get; set; }
    public string? Spacing { get; set; }
    public bool? Separator { get; set; }
    public string? HorizontalAlignment { get; set; }
    public string? Height { get; set; }
    public List<TeamsAdaptiveColumn> Columns { get; } = new();
}
