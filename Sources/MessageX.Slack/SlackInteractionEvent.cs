using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Verified transient Slack slash-command or interactive request payload.</summary>
public sealed class SlackInteractionEvent {
    internal SlackInteractionEvent(
        SlackInteractionKind kind,
        string name,
        string? text,
        JsonElement? providerPayload,
        SlackTransientInteractionContext transientContext) {
        Kind = kind;
        Name = name;
        Text = text;
        ProviderPayload = providerPayload;
        TransientContext = transientContext;
    }

    /// <summary>Supported interaction shape.</summary>
    public SlackInteractionKind Kind { get; }

    /// <summary>Normalized command, action, shortcut, or view callback name.</summary>
    public string Name { get; }

    /// <summary>Slash-command text, when present.</summary>
    public string? Text { get; }

    /// <summary>
    /// Parsed provider payload for interactive requests. This may contain user input and transient provider
    /// capabilities and must not be persisted or logged as a durable message reference.
    /// </summary>
    [JsonIgnore]
    public JsonElement? ProviderPayload { get; }

    /// <summary>Explicitly transient response and trigger capabilities.</summary>
    [JsonIgnore]
    public SlackTransientInteractionContext TransientContext { get; }
}
