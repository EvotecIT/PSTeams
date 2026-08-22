namespace MessageX.Hosting;

/// <summary>Validated application route derived from a verified provider event.</summary>
public sealed class MessageRoute {
    private const int MaximumNameLength = 128;

    private MessageRoute(MessageRouteKind kind, MessageEventKind eventKind, string? name) {
        Kind = kind;
        EventKind = eventKind;
        Name = name;
    }

    /// <summary>Application route category.</summary>
    public MessageRouteKind Kind { get; }

    /// <summary>Provider-neutral event kind for an event route.</summary>
    public MessageEventKind EventKind { get; }

    /// <summary>Normalized command or action name.</summary>
    public string? Name { get; }

    /// <summary>Creates a provider-neutral event route.</summary>
    public static MessageRoute ForEvent(MessageEventKind eventKind) {
        if (eventKind == MessageEventKind.Unknown) {
            throw new ArgumentOutOfRangeException(nameof(eventKind), "A routable event kind is required.");
        }
        return new MessageRoute(MessageRouteKind.Event, eventKind, null);
    }

    /// <summary>Creates a named command route.</summary>
    public static MessageRoute ForCommand(string name) =>
        new(MessageRouteKind.Command, MessageEventKind.CommandInvoked, NormalizeName(name, nameof(name)));

    /// <summary>Creates an application-mention route.</summary>
    public static MessageRoute ForMention() =>
        new(MessageRouteKind.Mention, MessageEventKind.AppMentioned, null);

    /// <summary>Creates a direct-message route.</summary>
    public static MessageRoute ForDirectMessage() =>
        new(MessageRouteKind.DirectMessage, MessageEventKind.MessageReceived, null);

    /// <summary>Creates a named interactive-action route.</summary>
    public static MessageRoute ForAction(string name) =>
        new(MessageRouteKind.Action, MessageEventKind.ActionInvoked, NormalizeName(name, nameof(name)));

    internal static string NormalizeName(string? value, string parameterName) {
        if (value is not null &&
            (value.Length > MaximumNameLength || value.Any(char.IsControl))) {
            throw new ArgumentException(
                "Route names must be bounded non-empty text without control characters.",
                parameterName);
        }
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized)) {
            throw new ArgumentException(
                "Route names must be bounded non-empty text without control characters.",
                parameterName);
        }
        return normalized!;
    }
}
