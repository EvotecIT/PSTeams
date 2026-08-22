namespace MessageX.Core;

/// <summary>Small provider-neutral classification for inbound messaging events.</summary>
public enum MessageEventKind {
    /// <summary>The provider event has no shared classification.</summary>
    Unknown = 0,
    /// <summary>A message was received.</summary>
    MessageReceived,
    /// <summary>The application or bot was mentioned.</summary>
    AppMentioned,
    /// <summary>A command was invoked.</summary>
    CommandInvoked,
    /// <summary>An interactive action was invoked.</summary>
    ActionInvoked,
    /// <summary>A modal or dialog was submitted.</summary>
    ModalSubmitted,
    /// <summary>A reaction was added, removed, or changed.</summary>
    ReactionChanged,
    /// <summary>An existing message was changed.</summary>
    MessageChanged,
    /// <summary>An existing message was deleted.</summary>
    MessageDeleted,
    /// <summary>The application was installed in a provider scope.</summary>
    Installed,
    /// <summary>The application was removed from a provider scope.</summary>
    Removed
}
