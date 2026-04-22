namespace TeamsX;

public sealed class TeamsMessageRequest {
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? Summary { get; set; }
    public TeamsAdaptiveCard? AdaptiveCard { get; set; }

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
