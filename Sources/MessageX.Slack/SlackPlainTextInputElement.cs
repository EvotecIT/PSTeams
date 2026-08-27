namespace MessageX.Slack;

/// <summary>A plain-text input used by Slack modal views.</summary>
public sealed class SlackPlainTextInputElement : SlackBlockElement {
    /// <inheritdoc />
    public override string Type => "plain_text_input";

    /// <summary>Application-defined action identifier.</summary>
    public string ActionId { get; set; } = string.Empty;

    /// <summary>Initial value.</summary>
    public string? InitialValue { get; set; }

    /// <summary>Whether the input accepts multiple lines.</summary>
    public bool Multiline { get; set; }

    /// <summary>Minimum accepted length.</summary>
    public int? MinimumLength { get; set; }

    /// <summary>Maximum accepted length.</summary>
    public int? MaximumLength { get; set; }

    /// <summary>Optional plain-text placeholder.</summary>
    public SlackTextObject? Placeholder { get; set; }
}
