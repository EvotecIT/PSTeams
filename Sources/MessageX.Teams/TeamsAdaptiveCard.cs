namespace MessageX.Teams;

public sealed class TeamsAdaptiveCard {
    public string Schema { get; set; } = "http://adaptivecards.io/schemas/adaptive-card.json";
    public string Type { get; set; } = "AdaptiveCard";
    public string Version { get; set; } = "1.2";
    public string? FallbackText { get; set; }
    public string? MinimumHeight { get; set; }
    public string? Speak { get; set; }
    public string? Language { get; set; }
    public string? VerticalContentAlignment { get; set; }
    public TeamsAdaptiveBackgroundImage? BackgroundImage { get; set; }
    public TeamsAdaptiveAction? SelectAction { get; set; }
    public TeamsAdaptiveRefresh? Refresh { get; set; }
    public bool? AllowImageExpand { get; set; }
    public bool FullWidth { get; set; }
    public List<TeamsAdaptiveCardElement> Body { get; } = new();
    public List<TeamsAdaptiveAction> Actions { get; } = new();
    public List<TeamsAdaptiveMention> Mentions { get; } = new();
}
