using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Verified transient Slack slash-command or interactive request payload.</summary>
public sealed class SlackInteractionEvent {
    /// <summary>Creates a verified Slack interaction value or rehydrates its safe persisted projection.</summary>
    public SlackInteractionEvent(
        SlackInteractionKind kind,
        string name,
        string? text,
        SlackInteractionPayload? providerPayload,
        SlackTransientInteractionContext? transientContext = null)
        : this(kind, name, text, providerPayload, null, null, transientContext) {
    }

    /// <summary>Creates a verified Slack interaction with both workspace and enterprise identity coordinates.</summary>
    [JsonConstructor]
    public SlackInteractionEvent(
        SlackInteractionKind kind,
        string name,
        string? text,
        SlackInteractionPayload? providerPayload,
        string? workspaceId,
        string? enterpriseId,
        SlackTransientInteractionContext? transientContext = null) {
        Kind = kind;
        Name = name;
        Text = text;
        ProviderPayload = providerPayload;
        WorkspaceId = workspaceId;
        EnterpriseId = enterpriseId;
        TransientContext = transientContext ?? SlackTransientInteractionContext.Unavailable;
    }

    /// <summary>Supported interaction shape.</summary>
    public SlackInteractionKind Kind { get; }

    /// <summary>Normalized command, action, shortcut, or view callback name.</summary>
    public string Name { get; }

    /// <summary>Slash-command text, when present.</summary>
    public string? Text { get; }

    /// <summary>
    /// Safe typed provider payload for interactive requests. Transient response and trigger capabilities are excluded.
    /// </summary>
    public SlackInteractionPayload? ProviderPayload { get; }

    /// <summary>Slack workspace identity when supplied by the verified request.</summary>
    public string? WorkspaceId { get; }

    /// <summary>Slack Enterprise Grid identity when supplied by the verified request.</summary>
    public string? EnterpriseId { get; }

    /// <summary>Explicitly transient response and trigger capabilities.</summary>
    [JsonIgnore]
    public SlackTransientInteractionContext TransientContext { get; }
}
