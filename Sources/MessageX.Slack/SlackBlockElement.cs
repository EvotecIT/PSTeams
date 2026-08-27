namespace MessageX.Slack;

/// <summary>A provider-native Slack Block Kit element.</summary>
public abstract class SlackBlockElement {
    /// <summary>Slack element type token.</summary>
    public abstract string Type { get; }
}
