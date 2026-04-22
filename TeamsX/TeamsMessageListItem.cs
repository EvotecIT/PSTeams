namespace TeamsX;

/// <summary>
/// Represents a legacy list item that is eventually rendered as connector-card fact text.
/// </summary>
public sealed class TeamsMessageListItem {
    public string? Text { get; set; }

    public int Level { get; set; }

    public bool Numbered { get; set; }
}
