namespace MessageX.Teams;

public sealed class TeamsAdaptiveShowCardAction : TeamsAdaptiveAction {
    public override string Type => "Action.ShowCard";

    public TeamsAdaptiveCard? Card { get; set; }
}
