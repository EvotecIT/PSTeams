namespace TeamsX;

/// <summary>
/// Represents a typed instruction that sets one section activity property.
/// </summary>
public sealed class TeamsMessageSectionDirective {
    public TeamsMessageSectionDirectiveType DirectiveType { get; set; }
    public string? Value { get; set; }
}
