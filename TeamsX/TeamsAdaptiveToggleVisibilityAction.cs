namespace TeamsX;

public sealed class TeamsAdaptiveToggleVisibilityAction : TeamsAdaptiveAction {
    public override string Type => "Action.ToggleVisibility";

    public List<string> TargetElements { get; } = new();
}
