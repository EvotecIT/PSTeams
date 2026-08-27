namespace MessageX.Slack;

/// <summary>Result of Slack's external file upload workflow.</summary>
public sealed class SlackFileUploadResult : MessageDeliveryResult {
    /// <summary>Creates an empty Slack file upload result.</summary>
    public SlackFileUploadResult()
        : base(MessageProviders.Slack) {
    }

    /// <summary>Slack file identifier when the upload was finalized.</summary>
    public string? FileId { get; set; }

    /// <summary>Provider-visible file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Conversation where the file was shared, or null when it remains private.</summary>
    public string? ConversationId { get; set; }
}
