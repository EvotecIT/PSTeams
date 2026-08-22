namespace MessageX.Teams;

public sealed class TeamsAdaptiveToggleVisibilityAction : TeamsAdaptiveAction {
    public override string Type => "Action.ToggleVisibility";

    public List<string> TargetElements { get; } = new();
}
