namespace MessageX.Hosting;

/// <summary>Application-level route selected from a verified provider event.</summary>
public enum MessageRouteKind {
    /// <summary>Route by provider-neutral event kind.</summary>
    Event = 0,
    /// <summary>Route a named command.</summary>
    Command,
    /// <summary>Route an application mention.</summary>
    Mention,
    /// <summary>Route a direct message.</summary>
    DirectMessage,
    /// <summary>Route a named interactive action.</summary>
    Action
}
