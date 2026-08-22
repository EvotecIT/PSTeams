using System.Security.Cryptography;
using System.Text;
using MessageX.Core;
using MessageX.Hosting;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Schema;
using Microsoft.Teams.Apps.Schema.Entities;
using Microsoft.Teams.Core.Schema;

namespace MessageX.Teams.Hosting.AspNetCore;

internal static class TeamsActivityMapper {
    private const int MaximumCoordinateLength = 256;
    private const int MaximumTextLength = 32 * 1024;

    public static string MapInstallationId(string installationId) =>
        NormalizeRequired(installationId, nameof(installationId));

    public static TeamsInboundDispatch MapMessage(
        MessageActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        CoreActivity? verifiedSource = null) {
        ArgumentNullException.ThrowIfNull(activity);
        var conversationKind = GetConversationKind(activity, verifiedSource);
        var recipientId = activity.Recipient?.Id;
        var isMention = recipientId is not null && activity
            .GetMentions()
            .Any(mention => string.Equals(
                mention.Mentioned?.Id,
                recipientId,
                StringComparison.Ordinal));

        MessageRoute route;
        MessageEventKind eventKind;
        if (conversationKind == MessageConversationKind.DirectMessage) {
            route = MessageRoute.ForDirectMessage();
            eventKind = MessageEventKind.MessageReceived;
        } else if (isMention) {
            route = MessageRoute.ForMention();
            eventKind = MessageEventKind.AppMentioned;
        } else {
            route = MessageRoute.ForEvent(MessageEventKind.MessageReceived);
            eventKind = MessageEventKind.MessageReceived;
        }

        return Create(
            activity,
            installationId,
            receivedAt,
            TeamsInboundActivityKind.Message,
            eventKind,
            route,
            RemoveRecipientMention(activity, recipientId),
            null,
            Array.Empty<string>(),
            Array.Empty<string>(),
            verifiedSource);
    }

    public static TeamsInboundDispatch MapMessageUpdate(
        MessageUpdateActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        CoreActivity? verifiedSource = null) =>
        Create(
            activity,
            installationId,
            receivedAt,
            TeamsInboundActivityKind.MessageUpdated,
            MessageEventKind.MessageChanged,
            MessageRoute.ForEvent(MessageEventKind.MessageChanged),
            activity.TextWithoutMentions,
            null,
            Array.Empty<string>(),
            Array.Empty<string>(),
            verifiedSource);

    public static TeamsInboundDispatch MapMessageDelete(
        MessageDeleteActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        CoreActivity? verifiedSource = null) =>
        Create(
            activity,
            installationId,
            receivedAt,
            TeamsInboundActivityKind.MessageDeleted,
            MessageEventKind.MessageDeleted,
            MessageRoute.ForEvent(MessageEventKind.MessageDeleted),
            null,
            null,
            Array.Empty<string>(),
            Array.Empty<string>(),
            verifiedSource);

    public static TeamsInboundDispatch MapReaction(
        MessageReactionActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        CoreActivity? verifiedSource = null) =>
        Create(
            activity,
            installationId,
            receivedAt,
            TeamsInboundActivityKind.ReactionChanged,
            MessageEventKind.ReactionChanged,
            MessageRoute.ForEvent(MessageEventKind.ReactionChanged),
            null,
            null,
            NormalizeReactions(activity.ReactionsAdded),
            NormalizeReactions(activity.ReactionsRemoved),
            verifiedSource);

    public static TeamsInboundDispatch MapAdaptiveCardAction(
        InvokeActivity<AdaptiveCardActionValue> activity,
        string installationId,
        DateTimeOffset receivedAt,
        CoreActivity? verifiedSource = null) {
        ArgumentNullException.ThrowIfNull(activity);
        var actionName = NormalizeRequired(
            activity.Value?.Action?.Verb ?? activity.Value?.Action?.Id,
            "actionName");
        return MapAdaptiveCardActionCore(
            activity,
            installationId,
            receivedAt,
            actionName,
            verifiedSource);
    }

    internal static TeamsInboundDispatch MapAdaptiveCardActionCore(
        TeamsActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        string actionName,
        CoreActivity? verifiedSource = null) {
        ArgumentNullException.ThrowIfNull(activity);
        actionName = NormalizeRequired(actionName, nameof(actionName));
        return Create(
            activity,
            installationId,
            receivedAt,
            TeamsInboundActivityKind.AdaptiveCardAction,
            MessageEventKind.ActionInvoked,
            MessageRoute.ForAction(actionName),
            null,
            actionName,
            Array.Empty<string>(),
            Array.Empty<string>(),
            verifiedSource);
    }

    private static TeamsInboundDispatch Create(
        TeamsActivity activity,
        string installationId,
        DateTimeOffset receivedAt,
        TeamsInboundActivityKind activityKind,
        MessageEventKind eventKind,
        MessageRoute route,
        string? text,
        string? actionName,
        IReadOnlyList<string> reactionsAdded,
        IReadOnlyList<string> reactionsRemoved,
        CoreActivity? verifiedSource) {
        ArgumentNullException.ThrowIfNull(activity);
        var safeInstallationId = NormalizeRequired(installationId, nameof(installationId));
        var activityId = NormalizeRequired(
            verifiedSource?.Id ?? activity.Id,
            "activity.Id");
        var conversationId = NormalizeRequired(activity.Conversation?.Id, "activity.Conversation.Id");
        var tenantId = NormalizeOptional(
            activity.Conversation?.TenantId ?? activity.ChannelData?.Tenant?.Id,
            "tenantId");
        var senderId = NormalizeOptional(
            activity.From?.AadObjectId ?? activity.From?.Id,
            "senderId");
        var replyToId = NormalizeOptional(
            verifiedSource?.ReplyToId ?? activity.ReplyToId,
            "replyToId");
        var messageId = activityKind == TeamsInboundActivityKind.ReactionChanged
            ? replyToId ?? throw new ArgumentException(
                "A Teams reaction must identify the reacted-to message.",
                "activity.ReplyToId")
            : activityId;
        var teamId = NormalizeOptional(
            activity.ChannelData?.Team?.Id ?? activity.ChannelData?.TeamsTeamId,
            "teamId");
        var channelId = NormalizeOptional(
            activity.ChannelData?.Channel?.Id ?? activity.ChannelData?.TeamsChannelId,
            "channelId");
        var conversationKind = GetConversationKind(
            activity,
            verifiedSource,
            useReplyAsThread: activityKind != TeamsInboundActivityKind.ReactionChanged);
        var timestampText = GetTimestamp(activity, verifiedSource);
        var timestamp = ParseTimestamp(timestampText);
        var locale = NormalizeOptional(activity.Locale, "locale");

        var payload = new TeamsInboundActivity(
            activityKind,
            activity,
            NormalizeText(text, nameof(text)),
            NormalizeOptional(actionName, nameof(actionName)),
            tenantId,
            teamId,
            channelId,
            locale,
            reactionsAdded,
            reactionsRemoved);
        var envelope = new MessageEventEnvelope<TeamsInboundActivity>(
            MessageProviders.Teams,
            safeInstallationId,
            CreateDeduplicationKey(
                safeInstallationId,
                activityKind,
                activityId,
                timestampText),
            eventKind,
            receivedAt,
            payload) {
            EventId = activityId,
            ScopeId = tenantId,
            SenderId = senderId,
            EventTime = timestamp,
            Conversation = new MessageReference(MessageProviders.Teams) {
                InstallationId = safeInstallationId,
                ScopeId = tenantId,
                ConversationId = conversationId,
                ConversationKind = conversationKind,
                ThreadId = conversationKind == MessageConversationKind.Thread ? replyToId : null
            },
            Message = new MessageReference(MessageProviders.Teams, messageId) {
                InstallationId = safeInstallationId,
                ScopeId = tenantId,
                ConversationId = conversationId,
                ConversationKind = conversationKind,
                ThreadId = conversationKind == MessageConversationKind.Thread ? replyToId : null,
                Timestamp = timestamp
            }
        };
        return new TeamsInboundDispatch(route, envelope);
    }

    private static MessageConversationKind GetConversationKind(
        TeamsActivity activity,
        CoreActivity? verifiedSource,
        bool useReplyAsThread = true) {
        if (useReplyAsThread &&
            !string.IsNullOrWhiteSpace(verifiedSource?.ReplyToId ?? activity.ReplyToId)) {
            return MessageConversationKind.Thread;
        }

        var type = activity.Conversation?.ConversationType?.ToString();
        if (string.Equals(type, "personal", StringComparison.OrdinalIgnoreCase)) {
            return MessageConversationKind.DirectMessage;
        }
        if (string.Equals(type, "groupChat", StringComparison.OrdinalIgnoreCase)) {
            return MessageConversationKind.GroupChat;
        }
        if (string.Equals(type, "channel", StringComparison.OrdinalIgnoreCase)) {
            return MessageConversationKind.Channel;
        }
        return MessageConversationKind.Unknown;
    }

    private static string? RemoveRecipientMention(MessageActivity activity, string? recipientId) {
        var text = activity.Text;
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(recipientId)) {
            return text?.Trim();
        }
        foreach (var mention in activity.GetMentions()) {
            if (string.Equals(mention.Mentioned?.Id, recipientId, StringComparison.Ordinal) &&
                !string.IsNullOrEmpty(mention.Text)) {
                text = text.Replace(mention.Text, string.Empty, StringComparison.Ordinal);
            }
        }
        return text.Trim();
    }

    private static string? GetTimestamp(
        TeamsActivity activity,
        CoreActivity? verifiedSource) {
        if (!string.IsNullOrWhiteSpace(activity.Timestamp)) {
            return activity.Timestamp;
        }
        if (verifiedSource?.Properties.TryGetValue("timestamp", out var timestamp) != true ||
            timestamp is null) {
            return null;
        }
        if (timestamp is System.Text.Json.JsonElement element) {
            return element.ValueKind == System.Text.Json.JsonValueKind.String
                ? element.GetString()
                : null;
        }
        return timestamp as string;
    }

    private static IReadOnlyList<string> NormalizeReactions(IList<MessageReaction>? reactions) =>
        reactions is null
            ? Array.Empty<string>()
            : reactions
                .Select(reaction => NormalizeOptional(reaction.Type?.ToString(), "reaction"))
                .Where(reaction => reaction is not null)
                .Cast<string>()
                .ToArray();

    private static DateTimeOffset? ParseTimestamp(string? value) =>
        DateTimeOffset.TryParse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal |
            System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var timestamp)
            ? timestamp
            : null;

    private static string CreateDeduplicationKey(
        string installationId,
        TeamsInboundActivityKind kind,
        string activityId,
        string? timestamp) {
        var source = string.Join("\n", installationId, kind, activityId, timestamp ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
    }

    private static string NormalizeRequired(string? value, string parameterName) =>
        NormalizeOptional(value, parameterName) ??
        throw new ArgumentException("A non-empty Teams coordinate is required.", parameterName);

    private static string? NormalizeOptional(string? value, string parameterName) {
        if (value is not null &&
            (value.Length > MaximumCoordinateLength || value.Any(char.IsControl))) {
            throw new ArgumentException(
                "Teams coordinates must be bounded text without control characters.",
                parameterName);
        }
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    private static string? NormalizeText(string? value, string parameterName) {
        if (value is not null && value.Length > MaximumTextLength) {
            throw new ArgumentException(
                $"Teams message text cannot exceed {MaximumTextLength} characters.",
                parameterName);
        }
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
