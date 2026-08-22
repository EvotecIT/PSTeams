namespace MessageX.Teams;

public abstract class TeamsAdaptiveAction {
    public abstract string Type { get; }

    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
