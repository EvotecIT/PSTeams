using System.Text.Json;
using MessageX.Core;
using MessageX.Hosting;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Persists the safe MessageX projection of an authenticated Microsoft Teams activity.</summary>
public sealed class TeamsInboundActivityDurableCodec : IMessageDurableCodec<TeamsInboundActivity> {
    private const string Discriminator = "teams.activity.v1";
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
        var metadata = MessageDurableEnvelopeMetadata.Capture(envelope);
        Validate(envelope.Payload, route, metadata);
        var projection = new TeamsActivityProjection {
            Metadata = metadata,
            Kind = envelope.Payload.Kind,
            Text = envelope.Payload.Text,
            ActionName = envelope.Payload.ActionName,
            TenantId = envelope.Payload.TenantId,
            TeamId = envelope.Payload.TeamId,
            ChannelId = envelope.Payload.ChannelId,
            Locale = envelope.Payload.Locale,
            ReactionsAdded = envelope.Payload.ReactionsAdded.ToArray(),
            ReactionsRemoved = envelope.Payload.ReactionsRemoved.ToArray(),
            InputData = new Dictionary<string, string?>(envelope.Payload.InputData, StringComparer.Ordinal)
        };
        return new MessageDurableRecord(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            Discriminator,
            JsonSerializer.SerializeToUtf8Bytes(projection, SerializerOptions));
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
        var payload = new TeamsInboundActivity(
            projection.Kind,
            null,
            projection.Text,
            projection.ActionName,
            projection.TenantId,
            projection.TeamId,
            projection.ChannelId,
            projection.Locale,
            projection.ReactionsAdded ?? Array.Empty<string>(),
            projection.ReactionsRemoved ?? Array.Empty<string>(),
            projection.InputData);
        Validate(payload, record.Route, projection.Metadata);
        return projection.Metadata.Restore(record, payload);
    }

    private static void Validate(
        TeamsInboundActivity payload,
        MessageRoute route,
        MessageDurableEnvelopeMetadata metadata) {
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
            !string.Equals(payload.TenantId, metadata.ScopeId, StringComparison.Ordinal) ||
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

    private static bool IsRequiredCoordinate(string? value) =>
        IsCoordinate(value) && !string.IsNullOrWhiteSpace(value);

    private static bool IsCoordinate(string? value) =>
        value is null || value.Length <= 256 && !value.Any(char.IsControl);

    private static bool IsText(string? value, int maximumLength) =>
        value is null || value.Length <= maximumLength;

    private sealed class TeamsActivityProjection {
        public MessageDurableEnvelopeMetadata? Metadata { get; set; }
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
    }
}
