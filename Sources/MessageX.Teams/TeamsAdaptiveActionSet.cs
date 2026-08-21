namespace MessageX.Teams;

public sealed class TeamsAdaptiveActionSet : TeamsAdaptiveCardElement {
    public override string Type => "ActionSet";

    public List<TeamsAdaptiveAction> Actions { get; } = new();
}
