namespace MessageX.Slack;

/// <summary>An input block used inside a Slack modal view.</summary>
public sealed class SlackInputBlock : SlackBlock {
    /// <inheritdoc />
    public override string Type => "input";

    /// <summary>Plain-text field label.</summary>
    public SlackTextObject Label { get; set; } = SlackTextObject.Plain(string.Empty);

    /// <summary>Input element.</summary>
    public SlackBlockElement Element { get; set; } = new SlackPlainTextInputElement();

    /// <summary>Whether the input can be omitted.</summary>
    public bool Optional { get; set; }

    /// <summary>Optional plain-text hint.</summary>
    public SlackTextObject? Hint { get; set; }
}
