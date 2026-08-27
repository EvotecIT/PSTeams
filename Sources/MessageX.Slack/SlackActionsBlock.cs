namespace MessageX.Slack;

/// <summary>A Slack Block Kit actions block.</summary>
public sealed class SlackActionsBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "actions";

    /// <summary>Interactive elements.</summary>
    public IList<SlackBlockElement> Elements { get; } = new List<SlackBlockElement>();
}
