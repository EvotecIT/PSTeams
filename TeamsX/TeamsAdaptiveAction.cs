namespace TeamsX;

public abstract class TeamsAdaptiveAction {
    public abstract string Type { get; }

    public string Title { get; set; } = string.Empty;
}
