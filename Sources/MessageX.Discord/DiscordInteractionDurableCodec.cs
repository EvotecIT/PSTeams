using System.Text.Json;
using MessageX.Hosting;

namespace MessageX.Discord;

/// <summary>Durable codec for verified Discord HTTP interactions.</summary>
public sealed class DiscordInteractionDurableCodec : IMessageDurableCodec<DiscordInboundInteraction>
{
    private const int MaximumPayloadBytes = 1024 * 1024;
    private const string Discriminator = "discord.interaction.v1";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static readonly string[] ForbiddenProperties = {
        "token", "access_token", "refresh_token", "oauth_token", "bot_token",
        "interaction_token", "authorization", "client_secret", "public_key", "signature"
    };

    /// <inheritdoc />
    public string PayloadType => Discriminator;

    /// <inheritdoc />
    public MessageDurableRecord Encode(
        MessageRoute route,
        MessageEventEnvelope<DiscordInboundInteraction> envelope)
    {
        if (route is null)
        {
            throw new ArgumentNullException(nameof(route));
        }
        if (envelope is null)
        {
            throw new ArgumentNullException(nameof(envelope));
        }
        var projection = new DiscordInteractionProjection
        {
            Metadata = MessageDurableEnvelopeMetadata.Capture(envelope),
            Kind = envelope.Payload.Kind,
            Name = envelope.Payload.Name,
            InstallationOwnerId = envelope.Payload.InstallationOwnerId,
            Locale = envelope.Payload.Locale,
            GuildLocale = envelope.Payload.GuildLocale,
            Context = envelope.Payload.Context,
            CommandType = envelope.Payload.CommandType,
            ApplicationId = envelope.Payload.TransientContext.ApplicationId,
            Data = MessageDurableJsonProjection.CreateSafeClone(envelope.Payload.Data, ForbiddenProperties)
        };
        var payload = JsonSerializer.SerializeToUtf8Bytes(projection, SerializerOptions);
        if (payload.Length > MaximumPayloadBytes)
        {
            throw new MessageDurablePayloadException("The complete Discord durable projection exceeds 1 MiB.");
        }
        return new MessageDurableRecord(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            Discriminator,
            payload);
    }

    /// <inheritdoc />
    public MessageEventEnvelope<DiscordInboundInteraction> Decode(MessageDurableRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }
        if (!string.Equals(record.PayloadType, Discriminator, StringComparison.Ordinal) ||
            !string.Equals(record.Provider, MessageProviders.Discord, StringComparison.Ordinal))
        {
            throw new MessageDurablePayloadException("The durable payload is not owned by the Discord codec.");
        }
        DiscordInteractionProjection projection;
        try
        {
            projection = JsonSerializer.Deserialize<DiscordInteractionProjection>(
                record.CopyPayload(),
                SerializerOptions) ?? throw new MessageDurablePayloadException("The Discord durable payload is empty.");
        }
        catch (Exception exception) when (IsProjectionException(exception))
        {
            throw new MessageDurablePayloadException("The Discord durable payload is malformed.", exception);
        }
        try
        {
            if (projection.Metadata is null ||
                !RouteMatches(projection.Kind, projection.Name, projection.CommandType, record.Route) ||
                !DiscordSnowflake.TryNormalize(projection.ApplicationId, out var applicationId) ||
                !IsSafeOptional(projection.InstallationOwnerId, 256) ||
                !IsSafeOptional(projection.Locale, 64) ||
                !IsSafeOptional(projection.GuildLocale, 64) ||
                projection.Context is < 0 or > 2 ||
                projection.Data.ValueKind != JsonValueKind.Object ||
                !DataMatches(projection.Kind, projection.Name!, projection.Data))
            {
                throw new MessageDurablePayloadException("The Discord interaction durable payload is incomplete.");
            }
            var payload = new DiscordInboundInteraction(
                projection.Kind,
                projection.Name!,
                projection.InstallationOwnerId,
                projection.Locale,
                projection.GuildLocale,
                projection.Context,
                projection.CommandType,
                MessageDurableJsonProjection.CreateSafeClone(projection.Data, ForbiddenProperties),
                new DiscordTransientInteractionContext(applicationId, null, null));
            return projection.Metadata.Restore(record, payload);
        }
        catch (MessageDurablePayloadException)
        {
            throw;
        }
        catch (Exception exception) when (IsProjectionException(exception))
        {
            throw new MessageDurablePayloadException("The Discord durable payload is malformed.", exception);
        }
    }

    private static bool RouteMatches(
        DiscordInteractionKind kind,
        string? name,
        DiscordApplicationCommandType? commandType,
        MessageRoute route)
    {
        if (name is null || name.Length is 0 or > 128 || name.Any(char.IsControl) ||
            !RouteNameMatches(name.Trim(), route))
        {
            return false;
        }
        return kind switch
        {
            DiscordInteractionKind.ApplicationCommand => route.Kind == MessageRouteKind.Command &&
                commandType is >= DiscordApplicationCommandType.ChatInput and <= DiscordApplicationCommandType.Message &&
                string.Equals(route.Qualifier, ((int)commandType.Value).ToString(), StringComparison.Ordinal),
            DiscordInteractionKind.MessageComponent => route.Kind == MessageRouteKind.Action && commandType is null,
            DiscordInteractionKind.Autocomplete => route.Kind == MessageRouteKind.Autocomplete &&
                commandType == DiscordApplicationCommandType.ChatInput,
            DiscordInteractionKind.ModalSubmit => route.Kind == MessageRouteKind.Submission && commandType is null,
            _ => false
        };
    }

    private static bool RouteNameMatches(string name, MessageRoute route) => route.NameComparison switch
    {
        MessageRouteNameComparison.Ordinal => string.Equals(name, route.Name, StringComparison.Ordinal),
        MessageRouteNameComparison.OrdinalIgnoreCase =>
            string.Equals(name, route.Name, StringComparison.OrdinalIgnoreCase),
        _ => false
    };

    private static bool IsProjectionException(Exception exception) =>
        exception is JsonException or ArgumentException or InvalidOperationException or NotSupportedException;

    private static bool IsSafeOptional(string? value, int maximumLength) =>
        value is null || (value.Length <= maximumLength && !value.Any(char.IsControl));

    private static bool DataMatches(
        DiscordInteractionKind kind,
        string name,
        JsonElement data)
    {
        var propertyName = kind is DiscordInteractionKind.MessageComponent or DiscordInteractionKind.ModalSubmit
            ? "custom_id"
            : "name";
        if (!data.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }
        var candidate = property.GetString();
        if (candidate is null || candidate.Length > 128 || candidate.Any(char.IsControl))
        {
            return false;
        }
        return string.Equals(candidate.Trim(), name, StringComparison.Ordinal);
    }
}

internal sealed class DiscordInteractionProjection
{
    public MessageDurableEnvelopeMetadata? Metadata { get; set; }
    public DiscordInteractionKind Kind { get; set; }
    public string? Name { get; set; }
    public string? InstallationOwnerId { get; set; }
    public string? Locale { get; set; }
    public string? GuildLocale { get; set; }
    public int? Context { get; set; }
    public DiscordApplicationCommandType? CommandType { get; set; }
    public string? ApplicationId { get; set; }
    public JsonElement Data { get; set; }
}
