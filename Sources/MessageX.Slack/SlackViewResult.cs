namespace MessageX.Slack;

/// <summary>Result of a Slack view operation.</summary>
public sealed class SlackViewResult : MessageDeliveryResult {
    /// <summary>Creates an empty Slack view result.</summary>
    public SlackViewResult()
        : base(MessageProviders.Slack) {
    }

    /// <summary>Slack view identifier returned by the provider.</summary>
    public string? ViewId { get; set; }
}
