using System.Text;
using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Verifies and parses Slack HTTP Events API requests without taking a web-host dependency.</summary>
public static class SlackEventsApiReceiver {
    private const int MaximumBodyBytes = 1024 * 1024;
    private const int MaximumCoordinateLength = 256;
    private static readonly TimeSpan DefaultReplayWindow = TimeSpan.FromMinutes(5);

    /// <summary>Verifies and receives one JSON Events API request.</summary>
    public static MessageReceiveResult<SlackInboundEvent> Receive(
        MessageInboundRequest request,
        string signingSecret,
        string signature,
        string timestamp,
        int? retryNumber = null,
        string? retryReason = null,
        TimeSpan? replayWindow = null) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }
        if (!IsJson(request.ContentType)) {
            return Reject(415, MessageReceiveFailureKind.Unsupported);
        }
        if (request.BodyLength is <= 0 or > MaximumBodyBytes) {
            return Reject(413, MessageReceiveFailureKind.Malformed);
        }
        if (retryNumber is < 0 or > 99 || !IsOptionalCoordinate(retryReason)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }

        var body = request.CopyBody();
        if (!SlackRequestVerifier.VerifyRecent(
                signingSecret,
                signature,
                timestamp,
                body,
                request.ReceivedAt,
                replayWindow ?? DefaultReplayWindow)) {
            return Reject(401, MessageReceiveFailureKind.Unauthorized);
        }

        try {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 32
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !TryReadRequired(root, "type", out var envelopeType)) {
                return Reject(400, MessageReceiveFailureKind.Malformed);
            }
            if (string.Equals(envelopeType, "url_verification", StringComparison.Ordinal)) {
                return ReceiveUrlVerification(root);
            }
            if (!string.Equals(envelopeType, "event_callback", StringComparison.Ordinal)) {
                return MessageReceiveResult<SlackInboundEvent>.Acknowledge(
                    MessageAcknowledgement.Empty(200));
            }
            return ReceiveEventCallback(request, root, retryNumber, retryReason?.Trim());
        }
        catch (JsonException) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
    }

    private static MessageReceiveResult<SlackInboundEvent> ReceiveUrlVerification(JsonElement root) {
        if (!TryReadRequired(root, "challenge", out var challenge)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        var response = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> {
            ["challenge"] = challenge
        });
        return MessageReceiveResult<SlackInboundEvent>.Acknowledge(
            new MessageAcknowledgement(200, "application/json; charset=utf-8", response));
    }

    private static MessageReceiveResult<SlackInboundEvent> ReceiveEventCallback(
        MessageInboundRequest request,
        JsonElement root,
        int? retryNumber,
        string? retryReason) {
        if (!TryReadRequired(root, "event_id", out var eventId) ||
            !root.TryGetProperty("event", out var providerEvent) ||
            providerEvent.ValueKind != JsonValueKind.Object ||
            !TryReadRequired(providerEvent, "type", out var eventType)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }

        if (!TryReadCoordinate(providerEvent, "subtype", out var subtype) ||
            !TryReadCoordinate(providerEvent, "channel_type", out var channelType) ||
            !TryReadCoordinate(providerEvent, "event_ts", out var eventTimestamp) ||
            !TryReadCoordinate(providerEvent, "ts", out var messageTimestamp) ||
            !TryReadText(providerEvent, "text", out var text)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        eventTimestamp ??= messageTimestamp;
        var eventKind = ClassifyEvent(eventType, subtype);
        if (eventKind == MessageEventKind.Unknown) {
            return MessageReceiveResult<SlackInboundEvent>.Acknowledge(
                MessageAcknowledgement.Empty(200));
        }
        var route = CreateRoute(eventKind, channelType);
        if (!TryReadCoordinate(root, "team_id", out var teamId) ||
            !TryReadCoordinate(providerEvent, "user", out var userId) ||
            !TryReadCoordinate(providerEvent, "channel", out var channelId)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        var payload = new SlackInboundEvent(
            eventType,
            providerEvent.Clone(),
            text,
            retryReason,
            retryNumber);
        var envelope = new MessageEventEnvelope<SlackInboundEvent>(
            MessageProviders.Slack,
            request.InstallationId,
            eventId,
            eventKind,
            request.ReceivedAt,
            payload) {
            EventId = eventId,
            ScopeId = teamId,
            SenderId = userId,
            CorrelationId = request.CorrelationId,
            EventTime = ReadUnixTime(root, "event_time")
        };
        if (channelId is not null) {
            envelope.Conversation = new MessageReference(MessageProviders.Slack) {
                InstallationId = request.InstallationId,
                ScopeId = teamId,
                ConversationId = channelId,
                ConversationKind = GetConversationKind(channelId, eventKind)
            };
        }
        var parsedTimestamp = SlackMessageValidator.ParseTimestamp(eventTimestamp);
        if (channelId is not null && eventTimestamp is not null && parsedTimestamp is not null) {
            envelope.Message = new MessageReference(MessageProviders.Slack) {
                InstallationId = request.InstallationId,
                ScopeId = teamId,
                ConversationId = channelId,
                MessageId = eventTimestamp,
                Timestamp = parsedTimestamp,
                ConversationKind = envelope.Conversation?.ConversationKind ?? MessageConversationKind.Unknown
            };
        }
        return MessageReceiveResult<SlackInboundEvent>.Dispatch(
            route,
            envelope,
            MessageAcknowledgement.Empty(200));
    }

    private static MessageEventKind ClassifyEvent(string eventType, string? subtype) {
        if (string.Equals(eventType, "app_mention", StringComparison.Ordinal)) {
            return MessageEventKind.AppMentioned;
        }
        if (string.Equals(eventType, "reaction_added", StringComparison.Ordinal) ||
            string.Equals(eventType, "reaction_removed", StringComparison.Ordinal)) {
            return MessageEventKind.ReactionChanged;
        }
        if (string.Equals(eventType, "app_uninstalled", StringComparison.Ordinal)) {
            return MessageEventKind.Removed;
        }
        if (!string.Equals(eventType, "message", StringComparison.Ordinal)) {
            return MessageEventKind.Unknown;
        }
        if (string.Equals(subtype, "message_changed", StringComparison.Ordinal)) {
            return MessageEventKind.MessageChanged;
        }
        if (string.Equals(subtype, "message_deleted", StringComparison.Ordinal)) {
            return MessageEventKind.MessageDeleted;
        }
        return subtype is null ? MessageEventKind.MessageReceived : MessageEventKind.Unknown;
    }

    private static MessageRoute CreateRoute(MessageEventKind eventKind, string? channelType) {
        if (eventKind == MessageEventKind.AppMentioned) {
            return MessageRoute.ForMention();
        }
        if (eventKind == MessageEventKind.MessageReceived &&
            string.Equals(channelType, "im", StringComparison.Ordinal)) {
            return MessageRoute.ForDirectMessage();
        }
        return MessageRoute.ForEvent(eventKind);
    }

    private static MessageConversationKind GetConversationKind(string channelId, MessageEventKind eventKind) {
        if (eventKind == MessageEventKind.MessageReceived && channelId.StartsWith("D", StringComparison.Ordinal)) {
            return MessageConversationKind.DirectMessage;
        }
        return channelId.StartsWith("C", StringComparison.Ordinal)
            ? MessageConversationKind.Channel
            : MessageConversationKind.Unknown;
    }

    private static DateTimeOffset? ReadUnixTime(JsonElement element, string propertyName) {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt64(out var unixSeconds)) {
            return null;
        }
        try {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException) {
            return null;
        }
    }

    private static bool TryReadRequired(JsonElement element, string propertyName, out string value) {
        value = ReadOptional(element, propertyName) ?? string.Empty;
        return value.Length > 0;
    }

    private static string? ReadOptional(JsonElement element, string propertyName) {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String) {
            return null;
        }
        var value = property.GetString();
        return IsOptionalCoordinate(value) ? value?.Trim() : null;
    }

    private static bool TryReadCoordinate(
        JsonElement element,
        string propertyName,
        out string? value) {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property)) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String) {
            return false;
        }
        var candidate = property.GetString();
        if (!IsOptionalCoordinate(candidate)) {
            return false;
        }
        var normalized = candidate?.Trim();
        value = string.IsNullOrEmpty(normalized) ? null : normalized;
        return true;
    }

    private static bool TryReadText(JsonElement element, string propertyName, out string? value) {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property)) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String) {
            return false;
        }
        value = property.GetString();
        return value is null or { Length: <= 40000 };
    }

    private static bool IsOptionalCoordinate(string? value) =>
        value is null ||
        (value.Length <= MaximumCoordinateLength && !value.Any(char.IsControl));

    private static bool IsJson(string contentType) {
        var mediaType = contentType.Split(';')[0].Trim();
        return string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static MessageReceiveResult<SlackInboundEvent> Reject(
        int statusCode,
        MessageReceiveFailureKind failureKind) =>
        MessageReceiveResult<SlackInboundEvent>.Reject(
            failureKind,
            MessageAcknowledgement.Empty(statusCode));
}
