using System.Text.Json;
using MessageX.Hosting;

namespace MessageX.Slack;

/// <summary>Durable codec for verified Slack Events API callbacks.</summary>
public sealed class SlackInboundEventDurableCodec : IMessageDurableCodec<SlackInboundEvent>
{
    private const string Discriminator = "slack.event.v1";

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(MessageRoute route, MessageEventEnvelope<SlackInboundEvent> envelope)
    {
        if (route is null) throw new ArgumentNullException(nameof(route));
        if (envelope is null) throw new ArgumentNullException(nameof(envelope));
        var providerEvent = SlackDurableCodecValidation.NormalizeEvent(envelope.Payload.ProviderEvent);
        if (!string.Equals(envelope.Payload.Text, providerEvent.Text, StringComparison.Ordinal))
        {
            throw new MessageDurablePayloadException("The Slack event text must match its safe provider projection.");
        }
        var projection = new SlackEventProjection
        {
            Metadata = MessageDurableEnvelopeMetadata.Capture(envelope),
            EventType = SlackDurableCodecValidation.Required(envelope.Payload.EventType, 256),
            WorkspaceId = SlackDurableCodecValidation.Optional(envelope.ScopeId, 256),
            ProviderEvent = providerEvent,
            RetryReason = SlackDurableCodecValidation.Optional(envelope.Payload.RetryReason, 256),
            RetryNumber = SlackDurableCodecValidation.RetryNumber(envelope.Payload.RetryNumber)
        };
        if (!SlackDurableCodecValidation.EventMatches(projection.EventType, providerEvent, route) ||
            !SlackDurableCodecValidation.EventCoordinatesMatch(
                providerEvent,
                projection.Metadata,
                envelope.DeduplicationKey,
                projection.WorkspaceId))
        {
            throw new MessageDurablePayloadException("The Slack event does not match its durable route.");
        }
        return SlackDurableCodecJson.Record(envelope, route, Discriminator, projection);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<SlackInboundEvent> Decode(MessageDurableRecord record)
    {
        var projection = SlackDurableCodecJson.Decode<SlackEventProjection>(record, Discriminator);
        try
        {
            var providerEvent = SlackDurableCodecValidation.NormalizeEvent(projection.ProviderEvent);
            var eventType = SlackDurableCodecValidation.Required(projection.EventType, 256);
            if (projection.Metadata is null ||
                !SlackDurableCodecValidation.EventMatches(eventType, providerEvent, record.Route) ||
                !SlackDurableCodecValidation.EventCoordinatesMatch(
                    providerEvent,
                    projection.Metadata,
                    record.DeduplicationKey,
                    SlackDurableCodecValidation.Optional(projection.WorkspaceId, 256)))
            {
                throw new MessageDurablePayloadException("The Slack event durable payload is incomplete.");
            }
            var payload = new SlackInboundEvent(eventType, providerEvent, providerEvent.Text,
                SlackDurableCodecValidation.Optional(projection.RetryReason, 256),
                SlackDurableCodecValidation.RetryNumber(projection.RetryNumber));
            return projection.Metadata.Restore(record, payload);
        }
        catch (MessageDurablePayloadException)
        {
            throw;
        }
        catch (Exception exception) when (SlackDurableCodecJson.IsProjectionException(exception))
        {
            throw new MessageDurablePayloadException("The Slack event durable payload is malformed.", exception);
        }
    }
}

/// <summary>Durable codec for verified Slack slash commands and interactions.</summary>
public sealed class SlackInteractionEventDurableCodec : IMessageDurableCodec<SlackInteractionEvent>
{
    private const string Discriminator = "slack.interaction.v1";

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(MessageRoute route, MessageEventEnvelope<SlackInteractionEvent> envelope)
    {
        if (route is null) throw new ArgumentNullException(nameof(route));
        if (envelope is null) throw new ArgumentNullException(nameof(envelope));
        var name = SlackDurableCodecValidation.InteractionName(envelope.Payload.Kind, envelope.Payload.Name);
        var providerPayload = SlackDurableCodecValidation.NormalizeInteraction(envelope.Payload.ProviderPayload);
        var metadata = MessageDurableEnvelopeMetadata.Capture(envelope);
        var projection = new SlackInteractionProjection
        {
            Metadata = metadata,
            SenderId = SlackDurableCodecValidation.Required(envelope.Payload.UserId, 256),
            RequestId = SlackDurableCodecValidation.Required(envelope.Payload.RequestId, 256),
            ChannelId = SlackDurableCodecValidation.Optional(envelope.Payload.ChannelId, 256),
            MessageTimestamp = SlackDurableCodecValidation.Optional(envelope.Payload.MessageTimestamp, 256),
            ThreadTimestamp = SlackDurableCodecValidation.Optional(envelope.Payload.ThreadTimestamp, 256),
            Kind = envelope.Payload.Kind,
            Name = name,
            Text = SlackDurableCodecValidation.OptionalText(envelope.Payload.Text, 40000),
            ProviderPayload = providerPayload,
            WorkspaceId = SlackDurableCodecValidation.Optional(envelope.Payload.WorkspaceId, 256),
            EnterpriseId = SlackDurableCodecValidation.Optional(envelope.Payload.EnterpriseId, 256)
        };
        if (!SlackDurableCodecValidation.InteractionMatches(
                envelope.Payload.Kind,
                name,
                envelope.Payload.Text,
                providerPayload,
                route) ||
            !SlackDurableCodecValidation.InteractionCoordinatesMatch(
                projection.Metadata,
                envelope.DeduplicationKey,
                projection.WorkspaceId,
                projection.EnterpriseId,
                envelope.Payload.Kind,
                providerPayload,
                projection.SenderId,
                projection.RequestId,
                projection.ChannelId,
                projection.MessageTimestamp,
                projection.ThreadTimestamp))
        {
            throw new MessageDurablePayloadException("The Slack interaction does not match its durable route.");
        }
        return SlackDurableCodecJson.Record(envelope, route, Discriminator, projection);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<SlackInteractionEvent> Decode(MessageDurableRecord record)
    {
        var projection = SlackDurableCodecJson.Decode<SlackInteractionProjection>(record, Discriminator);
        try
        {
            var name = SlackDurableCodecValidation.InteractionName(projection.Kind, projection.Name);
            var providerPayload = SlackDurableCodecValidation.NormalizeInteraction(projection.ProviderPayload);
            if (projection.Metadata is null ||
                 !SlackDurableCodecValidation.InteractionMatches(
                     projection.Kind,
                     name,
                     projection.Text,
                     providerPayload,
                     record.Route) ||
                 !SlackDurableCodecValidation.InteractionCoordinatesMatch(
                     projection.Metadata,
                     record.DeduplicationKey,
                     projection.WorkspaceId,
                     projection.EnterpriseId,
                     projection.Kind,
                     providerPayload,
                     projection.SenderId,
                     projection.RequestId,
                     projection.ChannelId,
                     projection.MessageTimestamp,
                     projection.ThreadTimestamp))
            {
                throw new MessageDurablePayloadException("The Slack interaction durable payload is incomplete.");
            }
            var payload = new SlackInteractionEvent(projection.Kind, name,
                SlackDurableCodecValidation.OptionalText(projection.Text, 40000), providerPayload,
                SlackDurableCodecValidation.Optional(projection.WorkspaceId, 256),
                SlackDurableCodecValidation.Optional(projection.EnterpriseId, 256),
                SlackDurableCodecValidation.Required(projection.SenderId, 256),
                SlackTransientInteractionContext.Unavailable,
                SlackDurableCodecValidation.Required(projection.RequestId, 256),
                SlackDurableCodecValidation.Optional(projection.ChannelId, 256),
                SlackDurableCodecValidation.Optional(projection.MessageTimestamp, 256),
                SlackDurableCodecValidation.Optional(projection.ThreadTimestamp, 256));
            return projection.Metadata.Restore(record, payload);
        }
        catch (MessageDurablePayloadException)
        {
            throw;
        }
        catch (Exception exception) when (SlackDurableCodecJson.IsProjectionException(exception))
        {
            throw new MessageDurablePayloadException("The Slack interaction durable payload is malformed.", exception);
        }
    }
}

internal static class SlackDurableCodecJson
{
    private const int MaximumPayloadBytes = 1024 * 1024;
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static MessageDurableRecord Record<T>(MessageEventEnvelope<T> envelope, MessageRoute route,
        string payloadType, object projection)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(projection, SerializerOptions);
        if (payload.Length > MaximumPayloadBytes)
        {
            throw new MessageDurablePayloadException("The complete Slack durable projection exceeds 1 MiB.");
        }
        return new MessageDurableRecord(envelope.Provider, envelope.InstallationId,
            envelope.DeduplicationKey, route, envelope.ReceivedAt, payloadType, payload);
    }

    public static T Decode<T>(MessageDurableRecord record, string payloadType) where T : class
    {
        if (record is null) throw new ArgumentNullException(nameof(record));
        if (!string.Equals(record.PayloadType, payloadType, StringComparison.Ordinal) ||
            !string.Equals(record.Provider, MessageProviders.Slack, StringComparison.Ordinal))
        {
            throw new MessageDurablePayloadException("The durable payload is not owned by this Slack codec.");
        }
        try
        {
            return JsonSerializer.Deserialize<T>(record.CopyPayload(), SerializerOptions)
                ?? throw new MessageDurablePayloadException("The Slack durable payload is empty.");
        }
        catch (Exception exception) when (IsProjectionException(exception))
        {
            throw new MessageDurablePayloadException("The Slack durable payload is malformed.", exception);
        }
    }

    public static bool IsProjectionException(Exception exception) =>
        exception is JsonException or ArgumentException or InvalidOperationException or NotSupportedException;
}

internal static class SlackDurableCodecValidation
{
    public static SlackEventPayload NormalizeEvent(SlackEventPayload? value)
    {
        if (value is null) throw new MessageDurablePayloadException("The Slack event projection is required.");
        var type = Required(value.Type, 256);
        var normalized = new SlackEventPayload(type, Optional(value.Subtype, 256),
            Optional(value.UserId, 256), Optional(value.ChannelId, 256), Optional(value.ChannelType, 256),
            Optional(value.MessageTimestamp, 256), Optional(value.EventTimestamp, 256),
            Optional(value.ThreadTimestamp, 256), OptionalText(value.Text, 40000), Optional(value.Reaction, 256),
            Optional(value.ItemType, 256));
        if (type is "reaction_added" or "reaction_removed" &&
            (normalized.UserId is null || normalized.ChannelId is null ||
             normalized.MessageTimestamp is null || normalized.Reaction is null ||
             !string.Equals(normalized.ItemType, "message", StringComparison.Ordinal)))
        {
            throw new MessageDurablePayloadException("A Slack reaction projection requires its actor and message target coordinates.");
        }
        return normalized;
    }

    public static SlackInteractionPayload? NormalizeInteraction(SlackInteractionPayload? value)
    {
        if (value is null) return null;
        if (value.Actions.Length > 1 || value.View?.Values.Length > 256 || value.State.Length > 256)
        {
            throw new MessageDurablePayloadException("The Slack interaction projection exceeds its supported shape.");
        }
        var actions = value.Actions.Select(NormalizeAction).ToArray();
        var state = value.State.Select(NormalizeViewValue).ToArray();
        SlackViewSubmissionInput? view = null;
        if (value.View is not null)
        {
            view = new SlackViewSubmissionInput(Required(value.View.CallbackId, 128),
                value.View.Values.Select(NormalizeViewValue).ToArray(),
                OptionalText(value.View.PrivateMetadata, 3000));
        }
        SlackMessageInput? message = null;
        if (value.Message is not null)
        {
            var timestamp = Required(value.Message.Timestamp, 32);
            if (SlackMessageValidator.ParseTimestamp(timestamp) is null)
            {
                throw new MessageDurablePayloadException("The Slack shortcut message timestamp is invalid.");
            }
            message = new SlackMessageInput(timestamp,
                OptionalText(value.Message.Text, 40000));
        }
        return new SlackInteractionPayload(actions, view, message, state);
    }

    public static bool InteractionMatches(
        SlackInteractionKind kind,
        string name,
        string? text,
        SlackInteractionPayload? providerPayload,
        MessageRoute route)
    {
        return kind switch
        {
            SlackInteractionKind.SlashCommand => route.Kind == MessageRouteKind.Command &&
                route.Qualifier is null && providerPayload is null &&
                string.Equals(name, route.Name, StringComparison.OrdinalIgnoreCase),
            SlackInteractionKind.Shortcut => route.Kind == MessageRouteKind.Action &&
                text is null &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 0 && providerPayload.View is null && providerPayload.State.Length == 0,
            SlackInteractionKind.BlockAction => route.Kind == MessageRouteKind.Action &&
                text is null &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 1 && providerPayload.View is null &&
                providerPayload.Message is null &&
                string.Equals(providerPayload.Actions[0].ActionId, name, StringComparison.Ordinal),
            SlackInteractionKind.ViewSubmission => route.Kind == MessageRouteKind.Submission &&
                text is null &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 0 && providerPayload.Message is null &&
                providerPayload.State.Length == 0 && providerPayload.View is not null &&
                string.Equals(providerPayload.View.CallbackId, name, StringComparison.Ordinal),
            _ => false
        };
    }

    public static bool EventMatches(string eventType, SlackEventPayload providerEvent, MessageRoute route)
    {
        if (!string.Equals(providerEvent.Type, eventType, StringComparison.Ordinal)) return false;
        var expectedKind = eventType switch
        {
            "app_mention" => MessageEventKind.AppMentioned,
            "reaction_added" or "reaction_removed" => MessageEventKind.ReactionChanged,
            "app_uninstalled" => MessageEventKind.Removed,
            "message" when providerEvent.Subtype == "message_changed" => MessageEventKind.MessageChanged,
            "message" when providerEvent.Subtype == "message_deleted" => MessageEventKind.MessageDeleted,
            "message" when providerEvent.Subtype is null => MessageEventKind.MessageReceived,
            _ => MessageEventKind.Unknown
        };
        if (expectedKind == MessageEventKind.Unknown || route.EventKind != expectedKind) return false;
        if (expectedKind is MessageEventKind.MessageChanged or MessageEventKind.MessageDeleted &&
            providerEvent.MessageTimestamp is null) return false;
        if (expectedKind == MessageEventKind.AppMentioned) return route.Kind == MessageRouteKind.Mention;
        if (expectedKind == MessageEventKind.MessageReceived && providerEvent.ChannelType is "im" or "mpim")
        {
            return route.Kind == MessageRouteKind.DirectMessage;
        }
        return route.Kind == MessageRouteKind.Event;
    }

    public static bool EventCoordinatesMatch(
        SlackEventPayload providerEvent,
        MessageDurableEnvelopeMetadata metadata,
        string deduplicationKey,
        string? workspaceId) {
        if (!string.Equals(metadata.EventId, deduplicationKey, StringComparison.Ordinal) ||
            !string.Equals(metadata.ScopeId, workspaceId, StringComparison.Ordinal) ||
            !string.Equals(metadata.SenderId, providerEvent.UserId, StringComparison.Ordinal)) {
            return false;
        }
        if (providerEvent.ChannelId is null) {
            return metadata.Conversation is null && metadata.Message is null;
        }
        if (metadata.Conversation is null ||
            !string.Equals(metadata.Conversation.ConversationId, providerEvent.ChannelId, StringComparison.Ordinal) ||
            !string.Equals(metadata.Conversation.ThreadId, providerEvent.ThreadTimestamp, StringComparison.Ordinal) ||
            metadata.Conversation.ConversationKind != ConversationKind(providerEvent)) {
            return false;
        }
        if (providerEvent.MessageTimestamp is null) {
            return metadata.Message is null;
        }
        return metadata.Message is not null &&
            string.Equals(metadata.Message.ConversationId, providerEvent.ChannelId, StringComparison.Ordinal) &&
            string.Equals(metadata.Message.ThreadId, providerEvent.ThreadTimestamp, StringComparison.Ordinal) &&
            string.Equals(metadata.Message.MessageId, providerEvent.MessageTimestamp, StringComparison.Ordinal);
    }

    public static bool InteractionCoordinatesMatch(
        MessageDurableEnvelopeMetadata metadata,
        string deduplicationKey,
        string? workspaceId,
        string? enterpriseId,
        SlackInteractionKind kind,
        SlackInteractionPayload? providerPayload,
        string? senderId,
        string? requestId,
        string? channelId,
        string? messageTimestamp,
        string? threadTimestamp) {
        if (!string.Equals(metadata.EventId, deduplicationKey, StringComparison.Ordinal) ||
            !string.Equals(metadata.EventId, Required(requestId, 256), StringComparison.Ordinal) ||
            !string.Equals(metadata.ScopeId, workspaceId ?? enterpriseId, StringComparison.Ordinal) ||
            !string.Equals(metadata.SenderId, Required(senderId, 256), StringComparison.Ordinal)) {
            return false;
        }
        channelId = Optional(channelId, 256);
        messageTimestamp = Optional(messageTimestamp, 256);
        threadTimestamp = Optional(threadTimestamp, 256);
        if (channelId is null) {
            return messageTimestamp is null && threadTimestamp is null &&
                metadata.Conversation is null && metadata.Message is null;
        }
        var conversationKind = threadTimestamp is not null
            ? MessageConversationKind.Thread
            : channelId.StartsWith("C", StringComparison.Ordinal)
                ? MessageConversationKind.Channel
                : channelId.StartsWith("D", StringComparison.Ordinal)
                    ? MessageConversationKind.DirectMessage
                    : MessageConversationKind.Unknown;
        if (metadata.Conversation is null ||
            !string.Equals(metadata.Conversation.ConversationId, channelId, StringComparison.Ordinal) ||
            !string.Equals(metadata.Conversation.ThreadId, threadTimestamp, StringComparison.Ordinal) ||
            metadata.Conversation.ConversationKind != conversationKind) {
            return false;
        }
        var timestamp = SlackMessageValidator.ParseTimestamp(messageTimestamp);
        if (messageTimestamp is null) {
            if (metadata.Message is not null) return false;
        } else if (timestamp is null || metadata.Message is null ||
                   !string.Equals(metadata.Message.ConversationId, channelId, StringComparison.Ordinal) ||
                   !string.Equals(metadata.Message.ThreadId, threadTimestamp, StringComparison.Ordinal) ||
                   metadata.Message.ConversationKind != conversationKind ||
                   !string.Equals(metadata.Message.MessageId, messageTimestamp, StringComparison.Ordinal) ||
                   metadata.Message.Timestamp != timestamp) {
            return false;
        }
        return kind != SlackInteractionKind.Shortcut ||
            string.Equals(providerPayload?.Message?.Timestamp, messageTimestamp, StringComparison.Ordinal);
    }

    private static MessageConversationKind ConversationKind(SlackEventPayload providerEvent) {
        if (providerEvent.ThreadTimestamp is not null) return MessageConversationKind.Thread;
        if (providerEvent.ChannelType == "mpim") return MessageConversationKind.GroupChat;
        if (providerEvent.ChannelType == "im" ||
            providerEvent.ChannelId?.StartsWith("D", StringComparison.Ordinal) == true) {
            return MessageConversationKind.DirectMessage;
        }
        return providerEvent.ChannelId?.StartsWith("C", StringComparison.Ordinal) == true
            ? MessageConversationKind.Channel
            : MessageConversationKind.Unknown;
    }

    public static string Required(string? value, int maximumLength) => Optional(value, maximumLength)
        ?? throw new MessageDurablePayloadException("A required Slack durable coordinate is missing or unsafe.");

    public static string? Optional(string? value, int maximumLength)
    {
        if (value is null) return null;
        if (value.Length > maximumLength || value.Any(char.IsControl))
        {
            throw new MessageDurablePayloadException("A Slack durable coordinate is unsafe.");
        }
        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    public static string InteractionName(SlackInteractionKind kind, string? value) =>
        Required(value, kind == SlackInteractionKind.BlockAction ? 255 : 128);

    public static string? OptionalText(string? value, int maximumLength)
    {
        if (value is not null && (value.Length > maximumLength || value.IndexOf('\0') >= 0))
        {
            throw new MessageDurablePayloadException("Slack durable text exceeds its safe boundary.");
        }
        return value;
    }

    public static int? RetryNumber(int? value)
    {
        if (value is < 0 or > 99)
        {
            throw new MessageDurablePayloadException("The Slack retry number is outside its supported range.");
        }
        return value;
    }

    private static SlackActionInput NormalizeAction(SlackActionInput? value)
    {
        if (value is null || value.SelectedValues.Length > 100)
        {
            throw new MessageDurablePayloadException("The Slack action projection is malformed.");
        }
        return new SlackActionInput(Required(value.ActionId, 255), Required(value.Type, 64), Optional(value.BlockId, 128),
            OptionalText(value.Value, 40000), value.SelectedValues.Select(item => OptionalText(item, 40000)
                ?? throw new MessageDurablePayloadException("A Slack selected value is required.")).ToArray(),
            SlackRichTextProjection.Normalize(value.RichTextValue));
    }

    private static SlackViewStateInput NormalizeViewValue(SlackViewStateInput? value)
    {
        if (value is null || value.SelectedValues.Length > 100 || value.FileIds.Length > 10)
        {
            throw new MessageDurablePayloadException("The Slack view projection is malformed.");
        }
        return new SlackViewStateInput(Required(value.BlockId, 128), Required(value.ActionId, 255),
            Required(value.Type, 64), OptionalText(value.Value, 40000),
            value.SelectedValues.Select(item => OptionalText(item, 40000)
                ?? throw new MessageDurablePayloadException("A Slack selected value is required.")).ToArray(),
            value.FileIds.Select(item => Required(item, 256)).ToArray(),
            SlackRichTextProjection.Normalize(value.RichTextValue));
    }
}

internal sealed class SlackEventProjection
{
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public string? EventType { get; set; }
    public string? WorkspaceId { get; set; }
    public SlackEventPayload? ProviderEvent { get; set; }
    public string? RetryReason { get; set; }
    public int? RetryNumber { get; set; }
}

internal sealed class SlackInteractionProjection
{
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public string? SenderId { get; set; }
    public string? RequestId { get; set; }
    public string? ChannelId { get; set; }
    public string? MessageTimestamp { get; set; }
    public string? ThreadTimestamp { get; set; }
    public SlackInteractionKind Kind { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public SlackInteractionPayload? ProviderPayload { get; set; }
    public string? WorkspaceId { get; set; }
    public string? EnterpriseId { get; set; }
}
