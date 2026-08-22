namespace MessageX.Slack;

/// <summary>Typed Slack Block Kit text content.</summary>
public sealed class SlackTextObject {
    /// <summary>Text formatting style.</summary>
    public SlackTextStyle Style { get; set; }

    /// <summary>Text content.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Whether plain-text emoji should be escaped to colon form.</summary>
    public bool? Emoji { get; set; }

    /// <summary>Whether Slack should avoid automatic link and mention conversion for mrkdwn text.</summary>
    public bool? Verbatim { get; set; }

    /// <summary>Creates plain-text content.</summary>
    public static SlackTextObject Plain(string text, bool? emoji = null) {
        return new SlackTextObject { Style = SlackTextStyle.PlainText, Text = text, Emoji = emoji };
    }

    /// <summary>Creates Slack mrkdwn content.</summary>
    public static SlackTextObject Markdown(string text, bool? verbatim = null) {
        return new SlackTextObject { Style = SlackTextStyle.Markdown, Text = text, Verbatim = verbatim };
    }
}
