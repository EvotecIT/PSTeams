using System.Text.Json.Serialization;
using Microsoft.Teams.Apps.Schema;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Safe Microsoft Teams activity metadata with transient access to the verified SDK activity.</summary>
public sealed class TeamsInboundActivity {
    internal TeamsInboundActivity(
        TeamsInboundActivityKind kind,
        TeamsActivity? activity,
        string? text,
        string? actionName,
        string? tenantId,
        string? teamId,
        string? channelId,
        string? locale,
        IReadOnlyList<string> reactionsAdded,
        IReadOnlyList<string> reactionsRemoved,
        IReadOnlyDictionary<string, string?>? inputData = null) {
        Kind = kind;
        Activity = activity;
        Text = text;
        ActionName = actionName;
        TenantId = tenantId;
        TeamId = teamId;
        ChannelId = channelId;
        Locale = locale;
        ReactionsAdded = reactionsAdded;
        ReactionsRemoved = reactionsRemoved;
        InputData = inputData is null
            ? new Dictionary<string, string?>(StringComparer.Ordinal)
            : new Dictionary<string, string?>(inputData, StringComparer.Ordinal);
    }

    /// <summary>Adapted activity shape.</summary>
    public TeamsInboundActivityKind Kind { get; }

    /// <summary>Message text, when applicable.</summary>
    public string? Text { get; }

    /// <summary>Adaptive Card action verb or identifier, when applicable.</summary>
    public string? ActionName { get; }

    /// <summary>Teams tenant identifier.</summary>
    public string? TenantId { get; }

    /// <summary>Teams team identifier, when supplied.</summary>
    public string? TeamId { get; }

    /// <summary>Teams channel identifier, when supplied.</summary>
    public string? ChannelId { get; }

    /// <summary>Activity locale, when supplied.</summary>
    public string? Locale { get; }

    /// <summary>Reaction types added by this activity.</summary>
    public IReadOnlyList<string> ReactionsAdded { get; }

    /// <summary>Reaction types removed by this activity.</summary>
    public IReadOnlyList<string> ReactionsRemoved { get; }

    /// <summary>Bounded scalar Adaptive Card input values, keyed by the submitted input identifier.</summary>
    public IReadOnlyDictionary<string, string?> InputData { get; }

    /// <summary>
    /// Verified Microsoft SDK activity for synchronous handler use. It is null after durable restoration and intentionally excluded from persistence.
    /// </summary>
    [JsonIgnore]
    public TeamsActivity? Activity { get; }
}
