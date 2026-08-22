namespace MessageX.Slack;

/// <summary>A Slack Block Kit section block.</summary>
public sealed class SlackSectionBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "section";

    /// <summary>Primary section text.</summary>
    public SlackTextObject? Text { get; set; }

    /// <summary>Compact two-column section fields.</summary>
    public List<SlackTextObject> Fields { get; } = new();

    /// <summary>Whether Slack should initially expand the section text.</summary>
    public bool? Expand { get; set; }
}
