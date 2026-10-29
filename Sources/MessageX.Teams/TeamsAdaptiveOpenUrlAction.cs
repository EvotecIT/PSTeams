namespace MessageX.Teams;

public sealed class TeamsAdaptiveOpenUrlAction {
    public string Type => "Action.OpenUrl";
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
