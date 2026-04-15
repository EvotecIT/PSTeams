namespace TeamsX;

public sealed class TeamsAdaptiveCard {
    public string Schema { get; set; } = "http://adaptivecards.io/schemas/adaptive-card.json";
    public string Type { get; set; } = "AdaptiveCard";
    public string Version { get; set; } = "1.2";
    public List<TeamsAdaptiveCardElement> Body { get; } = new();
    public List<TeamsAdaptiveAction> Actions { get; } = new();
    public List<TeamsAdaptiveMention> Mentions { get; } = new();
}
