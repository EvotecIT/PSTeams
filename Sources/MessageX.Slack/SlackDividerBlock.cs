namespace MessageX.Slack;

/// <summary>A Slack Block Kit divider block.</summary>
public sealed class SlackDividerBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "divider";
}
