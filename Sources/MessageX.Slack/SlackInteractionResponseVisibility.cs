namespace MessageX.Slack;

/// <summary>Visibility of a Slack interaction response.</summary>
public enum SlackInteractionResponseVisibility {
    /// <summary>Only the user who initiated the interaction sees the response.</summary>
    Ephemeral = 0,

    /// <summary>The response is visible in the conversation.</summary>
    InChannel = 1
}
