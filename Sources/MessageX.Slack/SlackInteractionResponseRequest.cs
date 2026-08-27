namespace MessageX.Slack;

/// <summary>A response sent through a verified Slack interaction's short-lived response URL.</summary>
public sealed class SlackInteractionResponseRequest {
    /// <summary>Message content. Omit only when deleting the original response.</summary>
    public SlackMessageRequest? Message { get; set; }

    /// <summary>Replaces the original interaction message instead of posting a new response.</summary>
    public bool ReplaceOriginal { get; set; }

    /// <summary>Deletes the original interaction message.</summary>
    public bool DeleteOriginal { get; set; }

    /// <summary>Visibility used when posting a response.</summary>
    public SlackInteractionResponseVisibility Visibility { get; set; } = SlackInteractionResponseVisibility.Ephemeral;
}
