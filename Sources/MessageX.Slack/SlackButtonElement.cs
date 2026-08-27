namespace MessageX.Slack;

/// <summary>A Slack Block Kit button.</summary>
public sealed class SlackButtonElement : SlackBlockElement {
    /// <inheritdoc />
    public override string Type => "button";

    /// <summary>Plain-text button label.</summary>
    public SlackTextObject Text { get; set; } = SlackTextObject.Plain(string.Empty);

    /// <summary>Application-defined action identifier.</summary>
    public string ActionId { get; set; } = string.Empty;

    /// <summary>Optional application-defined value returned with the interaction.</summary>
    public string? Value { get; set; }

    /// <summary>Optional external HTTPS URL opened by the button.</summary>
    public Uri? Url { get; set; }

    /// <summary>Visual style.</summary>
    public SlackButtonStyle Style { get; set; }

    /// <summary>Optional accessibility label.</summary>
    public string? AccessibilityLabel { get; set; }
}
