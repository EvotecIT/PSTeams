namespace MessageX.Teams;

/// <summary>
/// Describes the destination configured behind a Power Automate Workflow URL.
/// The URL remains send-only and does not expose Teams conversation coordinates.
/// </summary>
public enum TeamsWorkflowDestinationKind {
    /// <summary>The configured Workflow destination is not known to MessageX.</summary>
    Unknown,
    /// <summary>The Workflow delivers to a Teams channel.</summary>
    Channel,
    /// <summary>The Workflow delivers to a group chat.</summary>
    GroupChat,
    /// <summary>The Workflow delivers to a one-to-one chat.</summary>
    Chat
}
