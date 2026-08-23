namespace MessageX.Slack;

/// <summary>Supported Slack HTTP interaction shape.</summary>
public enum SlackInteractionKind {
    /// <summary>Unknown or unsupported interaction.</summary>
    Unknown = 0,
    /// <summary>Slash command request.</summary>
    SlashCommand,
    /// <summary>Global or message shortcut.</summary>
    Shortcut,
    /// <summary>Block action, button, or selection.</summary>
    BlockAction,
    /// <summary>Modal view submission.</summary>
    ViewSubmission
}
