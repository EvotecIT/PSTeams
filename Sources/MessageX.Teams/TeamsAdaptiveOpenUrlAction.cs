namespace MessageX.Teams;

public sealed class TeamsAdaptiveOpenUrlAction : TeamsAdaptiveAction {
    public override string Type => "Action.OpenUrl";

    public string Url { get; set; } = string.Empty;
}
