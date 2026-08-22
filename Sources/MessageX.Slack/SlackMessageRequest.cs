namespace MessageX.Slack;

/// <summary>A provider-native Slack message.</summary>
public sealed class SlackMessageRequest {
    /// <summary>Top-level message text and Block Kit accessibility fallback.</summary>
    public string? Text { get; set; }

    /// <summary>Block Kit blocks.</summary>
    public List<SlackBlock> Blocks { get; } = new();

    /// <summary>Parent Slack message timestamp for a thread reply.</summary>
    public string? ThreadTimestamp { get; set; }

    /// <summary>Whether a thread reply should also be broadcast to the conversation.</summary>
    public bool ReplyBroadcast { get; set; }

    /// <summary>Whether Slack should unfurl links.</summary>
    public bool? UnfurlLinks { get; set; }

    /// <summary>Whether Slack should unfurl media.</summary>
    public bool? UnfurlMedia { get; set; }
}
