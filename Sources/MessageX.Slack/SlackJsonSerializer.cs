namespace MessageX.Slack;

/// <summary>Serializes typed Slack messages without including credentials.</summary>
public static class SlackJsonSerializer {
    /// <summary>Serializes a message for the specified Slack target.</summary>
    public static string Serialize(SlackMessageRequest message, SlackMessageTarget target) {
        return SlackMessageRenderer.Render(message, target);
    }
}
