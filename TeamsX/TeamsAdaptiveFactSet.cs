namespace TeamsX;

public sealed class TeamsAdaptiveFactSet : TeamsAdaptiveCardElement {
    public override string Type => "FactSet";

    public List<TeamsAdaptiveFact> Facts { get; } = new();
}
