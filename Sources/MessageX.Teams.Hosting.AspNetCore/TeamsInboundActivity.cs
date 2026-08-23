using MessageX.Core;
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
        string? senderId,
        string? activityId,
        string? conversationId,
        MessageConversationKind conversationKind,
        string? threadId,
        string? messageId,
        string? timestampText,
        DateTimeOffset? eventTime,
        IReadOnlyList<string> reactionsAdded,
        IReadOnlyList<string> reactionsRemoved,
        IReadOnlyDictionary<string, string?>? inputData = null,
        IReadOnlyList<TeamsInboundAttachment>? attachments = null) {
        Kind = kind;
        Activity = activity;
        Text = text;
        ActionName = actionName;
        TenantId = tenantId;
        TeamId = teamId;
        ChannelId = channelId;
        Locale = locale;
        SenderId = senderId;
        ActivityId = activityId;
        ConversationId = conversationId;
        ConversationKind = conversationKind;
        ThreadId = threadId;
        MessageId = messageId;
        TimestampText = timestampText;
        EventTime = eventTime;
        ReactionsAdded = reactionsAdded;
        ReactionsRemoved = reactionsRemoved;
        InputData = inputData is null
            ? new Dictionary<string, string?>(StringComparer.Ordinal)
            : new Dictionary<string, string?>(inputData, StringComparer.Ordinal);
        Attachments = attachments?.ToArray() ?? Array.Empty<TeamsInboundAttachment>();
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

    /// <summary>Invoking Teams identity retained from the verified activity.</summary>
    public string? SenderId { get; }

    internal string? ActivityId { get; }
    internal string? ConversationId { get; }
    internal MessageConversationKind ConversationKind { get; }
    internal string? ThreadId { get; }
    internal string? MessageId { get; }
    internal string? TimestampText { get; }
    internal DateTimeOffset? EventTime { get; }

    /// <summary>Reaction types added by this activity.</summary>
    public IReadOnlyList<string> ReactionsAdded { get; }

    /// <summary>Reaction types removed by this activity.</summary>
    public IReadOnlyList<string> ReactionsRemoved { get; }

    /// <summary>Bounded scalar Adaptive Card input values, keyed by the submitted input identifier.</summary>
    public IReadOnlyDictionary<string, string?> InputData { get; }

    /// <summary>Capability-free attachment metadata and bounded content retained for durable handlers.</summary>
    public IReadOnlyList<TeamsInboundAttachment> Attachments { get; }

    internal TeamsActivity? Activity { get; }
}
