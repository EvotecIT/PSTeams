namespace MessageX.Hosting;

/// <summary>Validated application route derived from a verified provider event.</summary>
public sealed class MessageRoute {
    private const int MaximumNameLength = 128;

    private MessageRoute(
        MessageRouteKind kind,
        MessageEventKind eventKind,
        string? name,
        MessageRouteNameComparison nameComparison = MessageRouteNameComparison.None,
        string? qualifier = null) {
        Kind = kind;
        EventKind = eventKind;
        Name = name;
        NameComparison = nameComparison;
        Qualifier = qualifier;
    }

    /// <summary>Application route category.</summary>
    public MessageRouteKind Kind { get; }

    /// <summary>Provider-neutral event kind for an event route.</summary>
    public MessageEventKind EventKind { get; }

    /// <summary>Normalized command or action name.</summary>
    public string? Name { get; }

    /// <summary>Comparison semantics for the normalized route name.</summary>
    public MessageRouteNameComparison NameComparison { get; }

    /// <summary>Optional provider-native route variant, such as an application-command type.</summary>
    public string? Qualifier { get; }

    /// <summary>Creates a provider-neutral event route.</summary>
    public static MessageRoute ForEvent(MessageEventKind eventKind) {
        if (eventKind == MessageEventKind.Unknown ||
            !Enum.IsDefined(typeof(MessageEventKind), eventKind)) {
            throw new ArgumentOutOfRangeException(nameof(eventKind), "A routable event kind is required.");
        }
        return new MessageRoute(MessageRouteKind.Event, eventKind, null);
    }

    /// <summary>Creates a named command route.</summary>
    public static MessageRoute ForCommand(string name) => ForCommand(name, null);

    /// <summary>Creates a named command route with an exact provider-native variant.</summary>
    public static MessageRoute ForCommand(string name, string? qualifier) =>
        new(
            MessageRouteKind.Command,
            MessageEventKind.CommandInvoked,
            NormalizeName(name, nameof(name)),
            MessageRouteNameComparison.OrdinalIgnoreCase,
            NormalizeQualifier(qualifier, nameof(qualifier)));

    /// <summary>Creates an application-mention route.</summary>
    public static MessageRoute ForMention() =>
        new(MessageRouteKind.Mention, MessageEventKind.AppMentioned, null);

    /// <summary>Creates a direct-message route.</summary>
    public static MessageRoute ForDirectMessage() =>
        new(MessageRouteKind.DirectMessage, MessageEventKind.MessageReceived, null);

    /// <summary>Creates a named interactive-action route.</summary>
    public static MessageRoute ForAction(string name) =>
        new(
            MessageRouteKind.Action,
            MessageEventKind.ActionInvoked,
            NormalizeOpaqueName(name, nameof(name)),
            MessageRouteNameComparison.Ordinal);

    /// <summary>Creates a named modal or dialog submission route.</summary>
    public static MessageRoute ForSubmission(string name) =>
        new(
            MessageRouteKind.Submission,
            MessageEventKind.ModalSubmitted,
            NormalizeOpaqueName(name, nameof(name)),
            MessageRouteNameComparison.Ordinal);

    /// <summary>Creates a named provider-native autocomplete route.</summary>
    public static MessageRoute ForAutocomplete(string name) =>
        new(
            MessageRouteKind.Autocomplete,
            MessageEventKind.AutocompleteRequested,
            NormalizeName(name, nameof(name)),
            MessageRouteNameComparison.OrdinalIgnoreCase);

    /// <summary>Recreates one validated route from durable coordinates.</summary>
    public static MessageRoute FromDurableCoordinates(
        MessageRouteKind kind,
        MessageEventKind eventKind,
        string? name = null,
        string? qualifier = null) => kind switch {
            MessageRouteKind.Event when name is null && qualifier is null => ForEvent(eventKind),
            MessageRouteKind.Command when eventKind == MessageEventKind.CommandInvoked => ForDurableCommand(name, qualifier),
            MessageRouteKind.Mention when eventKind == MessageEventKind.AppMentioned && name is null && qualifier is null => ForMention(),
            MessageRouteKind.DirectMessage when eventKind == MessageEventKind.MessageReceived && name is null && qualifier is null => ForDirectMessage(),
            MessageRouteKind.Action when eventKind == MessageEventKind.ActionInvoked && qualifier is null => ForAction(name!),
            MessageRouteKind.Submission when eventKind == MessageEventKind.ModalSubmitted && qualifier is null => ForSubmission(name!),
            MessageRouteKind.Autocomplete when eventKind == MessageEventKind.AutocompleteRequested && qualifier is null => ForAutocomplete(name!),
            _ => throw new ArgumentException("The durable route coordinates do not describe a valid MessageX route.")
        };

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

    private static string NormalizeOpaqueName(string? value, string parameterName) {
        if (value is null || value.Length == 0 ||
            value.Length > MaximumNameLength ||
            value.Any(char.IsControl)) {
            throw new ArgumentException(
                "Route names must be bounded non-empty text without control characters.",
                parameterName);
        }
        return value!;
    }

    private static string? NormalizeQualifier(string? value, string parameterName) {
        if (value is null) {
            return null;
        }
        return NormalizeName(value, parameterName);
    }

    private static MessageRoute ForDurableCommand(string? name, string? qualifier) {
        if (!IsCanonicalNormalized(name) || (qualifier is not null && !IsCanonicalNormalized(qualifier))) {
            throw new ArgumentException("Durable command coordinates must already use their canonical form.");
        }
        return ForCommand(name!, qualifier);
    }

    private static bool IsCanonicalNormalized(string? value) =>
        value is not null && string.Equals(value, value.Trim(), StringComparison.Ordinal);
}
