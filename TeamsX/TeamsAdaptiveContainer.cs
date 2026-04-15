namespace TeamsX;

public sealed class TeamsAdaptiveContainer : TeamsAdaptiveCardElement {
    public override string Type => "Container";

    public List<TeamsAdaptiveCardElement> Items { get; } = new();
}
