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
            ProviderEvent = providerEvent,
            RetryReason = SlackDurableCodecValidation.Optional(envelope.Payload.RetryReason, 256),
            RetryNumber = SlackDurableCodecValidation.RetryNumber(envelope.Payload.RetryNumber)
        };
        if (!SlackDurableCodecValidation.EventMatches(projection.EventType, providerEvent, route))
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
                !SlackDurableCodecValidation.EventMatches(eventType, providerEvent, record.Route))
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
        var name = SlackDurableCodecValidation.Required(envelope.Payload.Name, 128);
        var providerPayload = SlackDurableCodecValidation.NormalizeInteraction(envelope.Payload.ProviderPayload);
        if (!SlackDurableCodecValidation.InteractionMatches(envelope.Payload.Kind, name, providerPayload, route))
        {
            throw new MessageDurablePayloadException("The Slack interaction does not match its durable route.");
        }
        var projection = new SlackInteractionProjection
        {
            Metadata = MessageDurableEnvelopeMetadata.Capture(envelope),
            Kind = envelope.Payload.Kind,
            Name = name,
            Text = SlackDurableCodecValidation.OptionalText(envelope.Payload.Text, 40000),
            ProviderPayload = providerPayload
        };
        return SlackDurableCodecJson.Record(envelope, route, Discriminator, projection);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<SlackInteractionEvent> Decode(MessageDurableRecord record)
    {
        var projection = SlackDurableCodecJson.Decode<SlackInteractionProjection>(record, Discriminator);
        try
        {
            var name = SlackDurableCodecValidation.Required(projection.Name, 128);
            var providerPayload = SlackDurableCodecValidation.NormalizeInteraction(projection.ProviderPayload);
            if (projection.Metadata is null ||
                !SlackDurableCodecValidation.InteractionMatches(projection.Kind, name, providerPayload, record.Route))
            {
                throw new MessageDurablePayloadException("The Slack interaction durable payload is incomplete.");
            }
            var payload = new SlackInteractionEvent(projection.Kind, name,
                SlackDurableCodecValidation.OptionalText(projection.Text, 40000), providerPayload,
                SlackTransientInteractionContext.Unavailable);
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
        return new SlackEventPayload(Required(value.Type, 256), Optional(value.Subtype, 256),
            Optional(value.UserId, 256), Optional(value.ChannelId, 256), Optional(value.ChannelType, 256),
            Optional(value.MessageTimestamp, 256), Optional(value.EventTimestamp, 256),
            Optional(value.ThreadTimestamp, 256), OptionalText(value.Text, 40000), Optional(value.Reaction, 256),
            Optional(value.ItemType, 256));
    }

    public static SlackInteractionPayload? NormalizeInteraction(SlackInteractionPayload? value)
    {
        if (value is null) return null;
        if (value.Actions.Length > 1 || value.View?.Values.Length > 256)
        {
            throw new MessageDurablePayloadException("The Slack interaction projection exceeds its supported shape.");
        }
        var actions = value.Actions.Select(NormalizeAction).ToArray();
        SlackViewSubmissionInput? view = null;
        if (value.View is not null)
        {
            view = new SlackViewSubmissionInput(Required(value.View.CallbackId, 128),
                value.View.Values.Select(NormalizeViewValue).ToArray());
        }
        SlackMessageInput? message = null;
        if (value.Message is not null)
        {
            message = new SlackMessageInput(Required(value.Message.Timestamp, 32),
                OptionalText(value.Message.Text, 40000));
        }
        return new SlackInteractionPayload(actions, view, message);
    }

    public static bool InteractionMatches(SlackInteractionKind kind, string name,
        SlackInteractionPayload? providerPayload, MessageRoute route)
    {
        return kind switch
        {
            SlackInteractionKind.SlashCommand => route.Kind == MessageRouteKind.Command && providerPayload is null &&
                string.Equals(name, route.Name, StringComparison.OrdinalIgnoreCase),
            SlackInteractionKind.Shortcut => route.Kind == MessageRouteKind.Action &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 0 && providerPayload.View is null,
            SlackInteractionKind.BlockAction => route.Kind == MessageRouteKind.Action &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 1 && providerPayload.View is null &&
                string.Equals(providerPayload.Actions[0].ActionId, name, StringComparison.Ordinal),
            SlackInteractionKind.ViewSubmission => route.Kind == MessageRouteKind.Submission &&
                string.Equals(name, route.Name, StringComparison.Ordinal) && providerPayload is not null &&
                providerPayload.Actions.Length == 0 && providerPayload.Message is null && providerPayload.View is not null &&
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
        if (expectedKind == MessageEventKind.AppMentioned) return route.Kind == MessageRouteKind.Mention;
        if (expectedKind == MessageEventKind.MessageReceived && providerEvent.ChannelType is "im" or "mpim")
        {
            return route.Kind == MessageRouteKind.DirectMessage;
        }
        return route.Kind == MessageRouteKind.Event;
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
        return new SlackActionInput(Required(value.ActionId, 128), Required(value.Type, 64), Optional(value.BlockId, 128),
            OptionalText(value.Value, 40000), value.SelectedValues.Select(item => OptionalText(item, 40000)
                ?? throw new MessageDurablePayloadException("A Slack selected value is required.")).ToArray());
    }

    private static SlackViewStateInput NormalizeViewValue(SlackViewStateInput? value)
    {
        if (value is null || value.SelectedValues.Length > 100)
        {
            throw new MessageDurablePayloadException("The Slack view projection is malformed.");
        }
        return new SlackViewStateInput(Required(value.BlockId, 128), Required(value.ActionId, 128),
            Required(value.Type, 64), OptionalText(value.Value, 40000),
            value.SelectedValues.Select(item => OptionalText(item, 40000)
                ?? throw new MessageDurablePayloadException("A Slack selected value is required.")).ToArray());
    }
}

internal sealed class SlackEventProjection
{
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public string? EventType { get; set; }
    public SlackEventPayload? ProviderEvent { get; set; }
    public string? RetryReason { get; set; }
    public int? RetryNumber { get; set; }
}

internal sealed class SlackInteractionProjection
{
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public SlackInteractionKind Kind { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public SlackInteractionPayload? ProviderPayload { get; set; }
}
