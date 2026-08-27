namespace MessageX.Slack;

/// <summary>A Slack Block Kit header.</summary>
public sealed class SlackHeaderBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "header";

    /// <summary>Plain-text header.</summary>
    public SlackTextObject Text { get; set; } = SlackTextObject.Plain(string.Empty);
}
