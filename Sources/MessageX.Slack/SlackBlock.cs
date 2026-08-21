namespace MessageX.Slack;

/// <summary>Base type for Slack Block Kit message blocks.</summary>
public abstract class SlackBlock {
    /// <summary>Slack block type token.</summary>
    public abstract string Type { get; }

    /// <summary>Optional unique block identifier.</summary>
    public string? BlockId { get; set; }
}
