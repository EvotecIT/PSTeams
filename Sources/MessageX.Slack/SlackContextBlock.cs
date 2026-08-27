namespace MessageX.Slack;

/// <summary>A Slack Block Kit context row containing compact text.</summary>
public sealed class SlackContextBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "context";

    /// <summary>Plain-text or mrkdwn context elements.</summary>
    public IList<SlackTextObject> Elements { get; } = new List<SlackTextObject>();
}
