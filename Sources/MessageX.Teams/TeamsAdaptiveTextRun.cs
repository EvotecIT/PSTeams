namespace MessageX.Teams;

public sealed class TeamsAdaptiveTextRun {
    public string Text { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool? Subtle { get; set; }
    public string? Size { get; set; }
    public string? Weight { get; set; }
    public bool? Highlight { get; set; }
    public bool? Italic { get; set; }
    public bool? StrikeThrough { get; set; }
    public string? FontType { get; set; }
}
