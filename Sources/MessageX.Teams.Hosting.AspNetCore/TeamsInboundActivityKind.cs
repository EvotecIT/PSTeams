namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Microsoft Teams activity shapes adapted into MessageX.</summary>
public enum TeamsInboundActivityKind {
    /// <summary>A new message.</summary>
    Message,
    /// <summary>An existing message was updated.</summary>
    MessageUpdated,
    /// <summary>An existing message was deleted.</summary>
    MessageDeleted,
    /// <summary>A message reaction changed.</summary>
    ReactionChanged,
    /// <summary>An Adaptive Card action was submitted.</summary>
    AdaptiveCardAction
}
