using System.Text.Json;
using MessageX.Core;
using MessageX.Hosting;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Persists the safe MessageX projection of an authenticated Microsoft Teams activity.</summary>
public sealed class TeamsInboundActivityDurableCodec : IMessageDurableCodec<TeamsInboundActivity> {
    private const int MaximumPayloadBytes = 1024 * 1024;
    private const string Discriminator = "teams.activity.v1";
    private static readonly string[] ForbiddenAttachmentProperties = {
        "token", "access_token", "accessToken", "refresh_token", "refreshToken",
        "authorization", "client_secret", "clientSecret", "serviceUrl", "contentUrl",
        "thumbnailUrl", "url"
    };
    private static readonly JsonSerializerOptions SerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(
        MessageRoute route,
        MessageEventEnvelope<TeamsInboundActivity> envelope) {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(envelope);
        if (!string.Equals(envelope.Provider, MessageProviders.Teams, StringComparison.Ordinal)) {
            throw new MessageDurablePayloadException("The envelope is not owned by the Teams codec.");
        }
        var metadata = MessageDurableEnvelopeMetadata.Capture(envelope);
        Validate(envelope.Payload, route, metadata, envelope.InstallationId, envelope.DeduplicationKey);
        var projection = new TeamsActivityProjection {
            Metadata = metadata,
            SenderId = envelope.Payload.SenderId,
            ActivityId = envelope.Payload.ActivityId,
            ConversationId = envelope.Payload.ConversationId,
            ConversationKind = envelope.Payload.ConversationKind,
            ThreadId = envelope.Payload.ThreadId,
            MessageId = envelope.Payload.MessageId,
            TimestampText = envelope.Payload.TimestampText,
            EventTime = envelope.Payload.EventTime,
            Kind = envelope.Payload.Kind,
            Text = envelope.Payload.Text,
            ActionName = envelope.Payload.ActionName,
            TenantId = envelope.Payload.TenantId,
            TeamId = envelope.Payload.TeamId,
            ChannelId = envelope.Payload.ChannelId,
            Locale = envelope.Payload.Locale,
            ReactionsAdded = envelope.Payload.ReactionsAdded.ToArray(),
            ReactionsRemoved = envelope.Payload.ReactionsRemoved.ToArray(),
            InputData = new Dictionary<string, string?>(envelope.Payload.InputData, StringComparer.Ordinal),
            Attachments = NormalizeAttachments(envelope.Payload.Attachments)
        };
        ValidateSender(metadata, projection.SenderId);
        var serialized = JsonSerializer.SerializeToUtf8Bytes(projection, SerializerOptions);
        if (serialized.Length > MaximumPayloadBytes) {
            throw new MessageDurablePayloadException("The complete Teams durable projection exceeds 1 MiB.");
        }
        return new MessageDurableRecord(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            Discriminator,
            serialized);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<TeamsInboundActivity> Decode(MessageDurableRecord record) {
        ArgumentNullException.ThrowIfNull(record);
        if (!string.Equals(record.PayloadType, Discriminator, StringComparison.Ordinal) ||
            !string.Equals(record.Provider, MessageProviders.Teams, StringComparison.Ordinal)) {
            throw new MessageDurablePayloadException("The durable payload is not owned by the Teams codec.");
        }
        TeamsActivityProjection projection;
        try {
            projection = JsonSerializer.Deserialize<TeamsActivityProjection>(
                    record.CopyPayload(),
                    SerializerOptions) ??
                throw new MessageDurablePayloadException("The Teams durable payload is empty.");
        } catch (JsonException exception) {
            throw new MessageDurablePayloadException("The Teams durable payload is malformed.", exception);
        }
        if (projection.Metadata is null) {
            throw new MessageDurablePayloadException("The Teams durable payload has no safe envelope metadata.");
        }
        ValidateSender(projection.Metadata, projection.SenderId);
        var payload = new TeamsInboundActivity(
            projection.Kind,
            null,
            projection.Text,
            projection.ActionName,
            projection.TenantId,
            projection.TeamId,
            projection.ChannelId,
            projection.Locale,
            projection.SenderId,
            projection.ActivityId,
            projection.ConversationId,
            projection.ConversationKind,
            projection.ThreadId,
            projection.MessageId,
            projection.TimestampText,
            projection.EventTime,
            projection.ReactionsAdded ?? Array.Empty<string>(),
            projection.ReactionsRemoved ?? Array.Empty<string>(),
            projection.InputData,
            NormalizeAttachments(projection.Attachments));
        Validate(payload, record.Route, projection.Metadata, record.InstallationId, record.DeduplicationKey);
        return projection.Metadata.Restore(record, payload);
    }

    private static void Validate(
        TeamsInboundActivity payload,
        MessageRoute route,
        MessageDurableEnvelopeMetadata metadata,
        string installationId,
        string deduplicationKey) {
        if (!Enum.IsDefined(typeof(TeamsInboundActivityKind), payload.Kind) ||
            !IsText(payload.Text, 32 * 1024) ||
            !IsCoordinate(payload.ActionName) ||
            !IsCoordinate(payload.TenantId) ||
            !IsCoordinate(payload.TeamId) ||
            !IsCoordinate(payload.ChannelId) ||
            !IsCoordinate(payload.Locale) ||
            !AreCoordinates(payload.ReactionsAdded) ||
            !AreCoordinates(payload.ReactionsRemoved) ||
            !AreInputsSafe(payload.InputData) ||
            !AreAttachmentsSafe(payload.Attachments) ||
            !string.Equals(payload.TenantId, metadata.ScopeId, StringComparison.Ordinal) ||
            !IdentityCoordinatesMatch(payload, metadata, installationId, deduplicationKey) ||
            !RouteMatches(payload, route)) {
            throw new MessageDurablePayloadException("The Teams durable payload is unsafe or does not match its route.");
        }
    }

    private static bool RouteMatches(TeamsInboundActivity payload, MessageRoute route) => payload.Kind switch {
        TeamsInboundActivityKind.Message => route.Kind switch {
            MessageRouteKind.DirectMessage => route.EventKind == MessageEventKind.MessageReceived,
            MessageRouteKind.Mention => route.EventKind == MessageEventKind.AppMentioned,
            MessageRouteKind.Event => route.EventKind == MessageEventKind.MessageReceived,
            _ => false
        },
        TeamsInboundActivityKind.MessageUpdated =>
            route.Kind == MessageRouteKind.Event && route.EventKind == MessageEventKind.MessageChanged,
        TeamsInboundActivityKind.MessageDeleted =>
            route.Kind == MessageRouteKind.Event && route.EventKind == MessageEventKind.MessageDeleted,
        TeamsInboundActivityKind.ReactionChanged =>
            route.Kind == MessageRouteKind.Event && route.EventKind == MessageEventKind.ReactionChanged,
        TeamsInboundActivityKind.AdaptiveCardAction =>
            route.Kind == MessageRouteKind.Action &&
            string.Equals(route.Name, payload.ActionName, StringComparison.Ordinal),
        _ => false
    };

    private static bool AreCoordinates(IReadOnlyList<string>? values) =>
        values is not null && values.Count <= 100 && values.All(IsRequiredCoordinate);

    private static bool AreInputsSafe(IReadOnlyDictionary<string, string?>? values) =>
        values is not null &&
        values.Count <= 64 &&
        values.All(static pair =>
            IsRequiredCoordinate(pair.Key) &&
            (pair.Value is null || pair.Value.Length <= 4096 && pair.Value.IndexOf('\0') < 0));

    private static bool AreAttachmentsSafe(IReadOnlyList<TeamsInboundAttachment>? values) =>
        values is not null &&
        values.Count <= 32 &&
        values.All(static attachment =>
            IsCoordinate(attachment.ContentType) &&
            IsCoordinate(attachment.Name));

    private static TeamsInboundAttachment[] NormalizeAttachments(
        IReadOnlyList<TeamsInboundAttachment>? attachments) {
        if (attachments is null || attachments.Count == 0) {
            return Array.Empty<TeamsInboundAttachment>();
        }
        if (attachments.Count > 32) {
            throw new MessageDurablePayloadException("Teams durable attachments exceed the supported shape.");
        }
        return attachments.Select(attachment => {
            if (attachment is null) {
                throw new MessageDurablePayloadException(
                    "A Teams durable attachment cannot be null.");
            }
            return new TeamsInboundAttachment(
                attachment.ContentType,
                attachment.Name,
                attachment.Content is null
                    ? null
                    : MessageDurableJsonProjection.CreateSafeClone(
                        attachment.Content,
                        ForbiddenAttachmentProperties));
        }).ToArray();
    }

    private static void ValidateSender(MessageDurableEnvelopeMetadata metadata, string? senderId) {
        if (!IsCanonicalRequiredCoordinate(senderId) ||
            !string.Equals(metadata.SenderId, senderId, StringComparison.Ordinal)) {
            throw new MessageDurablePayloadException(
                "The Teams durable sender does not match its envelope metadata.");
        }
    }

    private static bool IdentityCoordinatesMatch(
        TeamsInboundActivity payload,
        MessageDurableEnvelopeMetadata metadata,
        string installationId,
        string deduplicationKey) {
        if (!IsCanonicalRequiredCoordinate(payload.SenderId) ||
            !IsCanonicalRequiredCoordinate(payload.ActivityId) ||
            !IsCanonicalRequiredCoordinate(payload.ConversationId) ||
            !IsCanonicalRequiredCoordinate(payload.MessageId) ||
            !IsCanonicalOptionalCoordinate(payload.ThreadId) ||
            !Enum.IsDefined(typeof(MessageConversationKind), payload.ConversationKind) ||
            !string.Equals(metadata.SenderId, payload.SenderId, StringComparison.Ordinal) ||
            !string.Equals(metadata.EventId, payload.ActivityId, StringComparison.Ordinal) ||
            !string.Equals(
                deduplicationKey,
                TeamsActivityMapper.CreateDeduplicationKey(
                    installationId,
                    payload.Kind,
                    payload.ActivityId!,
                    payload.TimestampText),
                StringComparison.Ordinal) ||
            metadata.EventTime != payload.EventTime ||
            metadata.Conversation is null ||
            !string.Equals(metadata.Conversation.ConversationId, payload.ConversationId, StringComparison.Ordinal) ||
            !string.Equals(metadata.Conversation.ThreadId, payload.ThreadId, StringComparison.Ordinal) ||
            metadata.Conversation.ConversationKind != payload.ConversationKind ||
            metadata.Message is null ||
            !string.Equals(metadata.Message.ConversationId, payload.ConversationId, StringComparison.Ordinal) ||
            !string.Equals(metadata.Message.ThreadId, payload.ThreadId, StringComparison.Ordinal) ||
            metadata.Message.ConversationKind != payload.ConversationKind ||
            !string.Equals(metadata.Message.MessageId, payload.MessageId, StringComparison.Ordinal)) {
            return false;
        }
        return metadata.Message.Timestamp ==
            (payload.Kind == TeamsInboundActivityKind.ReactionChanged ? null : payload.EventTime);
    }

    private static bool IsRequiredCoordinate(string? value) =>
        IsCoordinate(value) && !string.IsNullOrWhiteSpace(value);

    private static bool IsCanonicalRequiredCoordinate(string? value) =>
        IsRequiredCoordinate(value) && string.Equals(value, value!.Trim(), StringComparison.Ordinal);

    private static bool IsCanonicalOptionalCoordinate(string? value) =>
        value is null || IsCanonicalRequiredCoordinate(value);

    private static bool IsCoordinate(string? value) =>
        value is null || value.Length <= 256 && !value.Any(char.IsControl);

    private static bool IsText(string? value, int maximumLength) =>
        value is null || value.Length <= maximumLength;

    private sealed class TeamsActivityProjection {
        public MessageDurableEnvelopeMetadata? Metadata { get; set; }
        public string? SenderId { get; set; }
        public string? ActivityId { get; set; }
        public string? ConversationId { get; set; }
        public MessageConversationKind ConversationKind { get; set; }
        public string? ThreadId { get; set; }
        public string? MessageId { get; set; }
        public string? TimestampText { get; set; }
        public DateTimeOffset? EventTime { get; set; }
        public TeamsInboundActivityKind Kind { get; set; }
        public string? Text { get; set; }
        public string? ActionName { get; set; }
        public string? TenantId { get; set; }
        public string? TeamId { get; set; }
        public string? ChannelId { get; set; }
        public string? Locale { get; set; }
        public string[]? ReactionsAdded { get; set; }
        public string[]? ReactionsRemoved { get; set; }
        public Dictionary<string, string?>? InputData { get; set; }
        public TeamsInboundAttachment[]? Attachments { get; set; }
    }
}
