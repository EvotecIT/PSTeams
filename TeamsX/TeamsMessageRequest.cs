namespace TeamsX;

public sealed class TeamsMessageRequest {
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? Summary { get; set; }
    public string? ThemeColor { get; set; }
    public bool HideOriginalBody { get; set; }
    public bool UseConnectorCardFormat { get; set; }
    public TeamsAdaptiveCard? AdaptiveCard { get; set; }
    public IList<TeamsMessageSection> Sections { get; } = new List<TeamsMessageSection>();

    public string EffectiveSummary {
        get {
            if (!string.IsNullOrWhiteSpace(Summary)) {
                return Summary!;
            }

            if (!string.IsNullOrWhiteSpace(Title)) {
                return Title!;
            }

            return Text ?? string.Empty;
        }
    }
}
