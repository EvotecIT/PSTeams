namespace MessageX.Teams;

public sealed class TeamsAdaptiveMention {
    public string Type => "mention";
    public string Text { get; set; } = string.Empty;
    public TeamsMentionedIdentity Mentioned { get; set; } = new();
}
