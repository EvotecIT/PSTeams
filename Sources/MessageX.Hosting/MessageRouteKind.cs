namespace MessageX.Hosting;

/// <summary>Application-level route selected from a verified provider event.</summary>
public enum MessageRouteKind {
    /// <summary>Route by provider-neutral event kind.</summary>
    Event = 0,
    /// <summary>Route a named command.</summary>
    Command = 1,
    /// <summary>Route an application mention.</summary>
    Mention = 2,
    /// <summary>Route a direct message.</summary>
    DirectMessage = 3,
    /// <summary>Route a named interactive action.</summary>
    Action = 4,
    /// <summary>Route a named modal or dialog submission.</summary>
    Submission = 5,
    /// <summary>Route a named provider-native autocomplete request.</summary>
    Autocomplete = 6
}
