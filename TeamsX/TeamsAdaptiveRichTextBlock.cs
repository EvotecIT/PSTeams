namespace TeamsX;

public sealed class TeamsAdaptiveRichTextBlock : TeamsAdaptiveCardElement {
    public override string Type => "RichTextBlock";

    public List<TeamsAdaptiveTextRun> Inlines { get; } = new();
}
