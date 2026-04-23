namespace TeamsX;

public sealed class TeamsAdaptiveFactSet : TeamsAdaptiveCardElement {
    public override string Type => "FactSet";

    public string? Height { get; set; }
    public string? Spacing { get; set; }
    public bool? Separator { get; set; }
    public List<TeamsAdaptiveFact> Facts { get; } = new();
}
