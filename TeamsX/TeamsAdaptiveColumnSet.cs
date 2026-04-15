namespace TeamsX;

public sealed class TeamsAdaptiveColumnSet : TeamsAdaptiveCardElement {
    public override string Type => "ColumnSet";

    public List<TeamsAdaptiveColumn> Columns { get; } = new();
}
