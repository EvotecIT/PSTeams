namespace TeamsX;

public sealed class TeamsAdaptiveShowCardAction : TeamsAdaptiveAction {
    public override string Type => "Action.ShowCard";

    public Dictionary<string, object?>? Card { get; set; }
}
