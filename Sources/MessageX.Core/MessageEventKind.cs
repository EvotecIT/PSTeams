namespace MessageX.Core;

/// <summary>Small provider-neutral classification for inbound messaging events.</summary>
public enum MessageEventKind {
    /// <summary>The provider event has no shared classification.</summary>
    Unknown = 0,
    /// <summary>A message was received.</summary>
    MessageReceived = 1,
    /// <summary>The application or bot was mentioned.</summary>
    AppMentioned = 2,
    /// <summary>A command was invoked.</summary>
    CommandInvoked = 3,
    /// <summary>An interactive action was invoked.</summary>
    ActionInvoked = 4,
    /// <summary>A modal or dialog was submitted.</summary>
    ModalSubmitted = 5,
    /// <summary>A reaction was added, removed, or changed.</summary>
    ReactionChanged = 6,
    /// <summary>An existing message was changed.</summary>
    MessageChanged = 7,
    /// <summary>An existing message was deleted.</summary>
    MessageDeleted = 8,
    /// <summary>The application was installed in a provider scope.</summary>
    Installed = 9,
    /// <summary>The application was removed from a provider scope.</summary>
    Removed = 10,
    /// <summary>Provider-native command autocomplete choices were requested.</summary>
    AutocompleteRequested = 11
}
