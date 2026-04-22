namespace TeamsX;

public sealed class TeamsAdaptiveColumn : TeamsAdaptiveCardElement {
    public override string Type => "Column";

    public string? Width { get; set; }
    public List<TeamsAdaptiveCardElement> Items { get; } = new();
}
