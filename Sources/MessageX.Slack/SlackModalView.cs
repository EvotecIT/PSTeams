namespace MessageX.Slack;

/// <summary>A typed Slack modal view opened from a verified interaction trigger.</summary>
public sealed class SlackModalView {
    /// <summary>Application-defined callback identifier.</summary>
    public string CallbackId { get; set; } = string.Empty;

    /// <summary>Plain-text modal title.</summary>
    public SlackTextObject Title { get; set; } = SlackTextObject.Plain(string.Empty);

    /// <summary>Optional plain-text submit label.</summary>
    public SlackTextObject? Submit { get; set; }

    /// <summary>Optional plain-text close label.</summary>
    public SlackTextObject? Close { get; set; }

    /// <summary>Whether closing the view produces a view_closed interaction.</summary>
    public bool NotifyOnClose { get; set; }

    /// <summary>Modal Block Kit blocks.</summary>
    public IList<SlackBlock> Blocks { get; } = new List<SlackBlock>();
}
