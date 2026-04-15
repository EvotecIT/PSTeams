namespace TeamsX;

public sealed class TeamsAdaptiveTextBlock : TeamsAdaptiveCardElement {
    public override string Type => "TextBlock";

    public string Text { get; set; } = string.Empty;
    public bool Wrap { get; set; } = true;
    public string? Size { get; set; }
    public string? Weight { get; set; }
    public string? Color { get; set; }
}
