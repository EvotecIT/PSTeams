namespace MessageX.Slack;

/// <summary>Slack message delivery status.</summary>
public sealed class SlackDeliveryResult : MessageDeliveryResult {
    /// <summary>Creates a Slack delivery result.</summary>
    public SlackDeliveryResult()
        : base(MessageProviders.Slack) {
    }

    /// <summary>Delivery transport.</summary>
    public SlackDeliveryMethod DeliveryMethod { get; set; }

    /// <summary>Safe target label or conversation identifier.</summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>Raw provider response for explicit C# diagnostics. PowerShell does not emit it by default.</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Returned Slack conversation identifier.</summary>
    public string? ConversationId => Reference?.ConversationId;

    /// <summary>Returned Slack message timestamp identifier.</summary>
    public string? TimestampId => Reference?.MessageId;
}
