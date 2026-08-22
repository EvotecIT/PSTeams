using System.Text.Json;
using MessageX.Hosting;

namespace MessageX.Slack;

/// <summary>Durable codec for verified Slack Events API callbacks.</summary>
public sealed class SlackInboundEventDurableCodec : IMessageDurableCodec<SlackInboundEvent> {
    private const string Discriminator = "slack.event.v1";

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(
        MessageRoute route,
        MessageEventEnvelope<SlackInboundEvent> envelope) {
        if (route is null) {
            throw new ArgumentNullException(nameof(route));
        }
        if (envelope is null) {
            throw new ArgumentNullException(nameof(envelope));
        }
        var projection = new SlackEventProjection {
            Metadata = MessageDurableEnvelopeMetadata.Capture(envelope),
            EventType = envelope.Payload.EventType,
            ProviderEvent = SlackDurableCodecJson.Sanitize(envelope.Payload.ProviderEvent),
            Text = envelope.Payload.Text,
            RetryReason = envelope.Payload.RetryReason,
            RetryNumber = envelope.Payload.RetryNumber
        };
        return SlackDurableCodecJson.Record(envelope, route, Discriminator, projection);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<SlackInboundEvent> Decode(MessageDurableRecord record) {
        var projection = SlackDurableCodecJson.Decode<SlackEventProjection>(record, Discriminator);
        if (projection.Metadata is null ||
            !SlackDurableCodecJson.EventMatches(projection.EventType, projection.ProviderEvent, record.Route)) {
            throw new MessageDurablePayloadException("The Slack event durable payload is incomplete.");
        }
        var payload = new SlackInboundEvent(
            projection.EventType!,
            SlackDurableCodecJson.Sanitize(projection.ProviderEvent),
            projection.Text,
            projection.RetryReason,
            projection.RetryNumber);
        return projection.Metadata.Restore(record, payload);
    }
}

/// <summary>Durable codec for verified Slack slash commands and interactions.</summary>
public sealed class SlackInteractionEventDurableCodec : IMessageDurableCodec<SlackInteractionEvent> {
    private const string Discriminator = "slack.interaction.v1";

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(
        MessageRoute route,
        MessageEventEnvelope<SlackInteractionEvent> envelope) {
        if (route is null) {
            throw new ArgumentNullException(nameof(route));
        }
        if (envelope is null) {
            throw new ArgumentNullException(nameof(envelope));
        }
        var projection = new SlackInteractionProjection {
            Metadata = MessageDurableEnvelopeMetadata.Capture(envelope),
            Kind = envelope.Payload.Kind,
            Name = envelope.Payload.Name,
            Text = envelope.Payload.Text,
            ProviderPayload = envelope.Payload.ProviderPayload is { } providerPayload
                ? SlackDurableCodecJson.Sanitize(providerPayload)
                : null
        };
        return SlackDurableCodecJson.Record(envelope, route, Discriminator, projection);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<SlackInteractionEvent> Decode(MessageDurableRecord record) {
        var projection = SlackDurableCodecJson.Decode<SlackInteractionProjection>(record, Discriminator);
        if (projection.Metadata is null ||
            !SlackDurableCodecJson.InteractionMatches(
                projection.Kind,
                projection.Name,
                projection.ProviderPayload,
                record.Route)) {
            throw new MessageDurablePayloadException("The Slack interaction durable payload is incomplete.");
        }
        var payload = new SlackInteractionEvent(
            projection.Kind,
            projection.Name!,
            projection.Text,
            projection.ProviderPayload is { } providerPayload
                ? SlackDurableCodecJson.Sanitize(providerPayload)
                : null,
            new SlackTransientInteractionContext(null, null));
        return projection.Metadata.Restore(record, payload);
    }
}

internal static class SlackDurableCodecJson {
    private static readonly JsonSerializerOptions SerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly string[] ForbiddenProperties = {
        "token", "access_token", "refresh_token", "oauth_token", "bot_access_token",
        "response_url", "response_urls", "trigger_id", "signing_secret", "client_secret",
        "authorization", "authorizations"
    };

    public static JsonElement Sanitize(JsonElement value) =>
        MessageDurableJsonProjection.CreateSafeClone(value, ForbiddenProperties);

    public static MessageDurableRecord Record<T>(
        MessageEventEnvelope<T> envelope,
        MessageRoute route,
        string payloadType,
        object projection) => new(
        envelope.Provider,
        envelope.InstallationId,
        envelope.DeduplicationKey,
        route,
        envelope.ReceivedAt,
        payloadType,
        JsonSerializer.SerializeToUtf8Bytes(projection, SerializerOptions));

    public static T Decode<T>(MessageDurableRecord record, string payloadType) where T : class {
        if (record is null) {
            throw new ArgumentNullException(nameof(record));
        }
        if (!string.Equals(record.PayloadType, payloadType, StringComparison.Ordinal) ||
            !string.Equals(record.Provider, MessageProviders.Slack, StringComparison.Ordinal)) {
            throw new MessageDurablePayloadException("The durable payload is not owned by this Slack codec.");
        }
        try {
            return JsonSerializer.Deserialize<T>(record.CopyPayload(), SerializerOptions)
                ?? throw new MessageDurablePayloadException("The Slack durable payload is empty.");
        }
        catch (JsonException exception) {
            throw new MessageDurablePayloadException("The Slack durable payload is malformed.", exception);
        }
    }

    public static bool InteractionMatches(
        SlackInteractionKind kind,
        string? name,
        JsonElement? providerPayload,
        MessageRoute route) {
        if (!IsSafeText(name, 128, required: true) ||
            !string.Equals(name!.Trim(), route.Name, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return kind switch {
            SlackInteractionKind.SlashCommand => route.Kind == MessageRouteKind.Command && providerPayload is null,
            SlackInteractionKind.Shortcut => route.Kind == MessageRouteKind.Action &&
                providerPayload is { ValueKind: JsonValueKind.Object } shortcut &&
                PropertyEquals(shortcut, "callback_id", name),
            SlackInteractionKind.BlockAction => route.Kind == MessageRouteKind.Action &&
                providerPayload is { ValueKind: JsonValueKind.Object } action &&
                action.TryGetProperty("actions", out var actions) &&
                actions.ValueKind == JsonValueKind.Array && actions.GetArrayLength() == 1 &&
                PropertyEquals(actions[0], "action_id", name),
            SlackInteractionKind.ViewSubmission => route.Kind == MessageRouteKind.Submission &&
                providerPayload is { ValueKind: JsonValueKind.Object } submission &&
                submission.TryGetProperty("view", out var view) &&
                view.ValueKind == JsonValueKind.Object && PropertyEquals(view, "callback_id", name),
            _ => false
        };
    }

    public static bool EventMatches(string? eventType, JsonElement providerEvent, MessageRoute route) {
        if (!IsSafeText(eventType, 128, required: true) ||
            providerEvent.ValueKind != JsonValueKind.Object ||
            !PropertyEquals(providerEvent, "type", eventType)) {
            return false;
        }
        var subtype = providerEvent.TryGetProperty("subtype", out var subtypeProperty) &&
            subtypeProperty.ValueKind == JsonValueKind.String
                ? subtypeProperty.GetString()
                : null;
        var expectedKind = eventType switch {
            "app_mention" => MessageEventKind.AppMentioned,
            "reaction_added" or "reaction_removed" => MessageEventKind.ReactionChanged,
            "app_uninstalled" => MessageEventKind.Removed,
            "message" when subtype == "message_changed" => MessageEventKind.MessageChanged,
            "message" when subtype == "message_deleted" => MessageEventKind.MessageDeleted,
            "message" when subtype is null => MessageEventKind.MessageReceived,
            _ => MessageEventKind.Unknown
        };
        if (expectedKind == MessageEventKind.Unknown || route.EventKind != expectedKind) {
            return false;
        }
        if (expectedKind == MessageEventKind.AppMentioned) {
            return route.Kind == MessageRouteKind.Mention;
        }
        if (expectedKind == MessageEventKind.MessageReceived &&
            PropertyEquals(providerEvent, "channel_type", "im")) {
            return route.Kind == MessageRouteKind.DirectMessage;
        }
        return route.Kind == MessageRouteKind.Event;
    }

    private static bool PropertyEquals(JsonElement owner, string propertyName, string? expected) =>
        owner.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.String &&
        string.Equals(property.GetString(), expected, StringComparison.Ordinal);

    public static bool IsSafeText(string? value, int maximumLength, bool required) =>
        value is not null && value.Length <= maximumLength && !value.Any(char.IsControl) &&
        (!required || value.Trim().Length > 0);
}

internal sealed class SlackEventProjection {
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public string? EventType { get; set; }
    public JsonElement ProviderEvent { get; set; }
    public string? Text { get; set; }
    public string? RetryReason { get; set; }
    public int? RetryNumber { get; set; }
}

internal sealed class SlackInteractionProjection {
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public SlackInteractionKind Kind { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public JsonElement? ProviderPayload { get; set; }
}
