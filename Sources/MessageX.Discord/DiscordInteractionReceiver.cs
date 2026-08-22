using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MessageX.Discord;

/// <summary>Verifies and parses Discord HTTP interactions without taking a web-host dependency.</summary>
public static class DiscordInteractionReceiver {
    private const int MaximumBodyBytes = 1024 * 1024;
    private const int MaximumTokenLength = 2048;
    private static readonly TimeSpan DefaultReplayWindow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan InteractionTokenLifetime = TimeSpan.FromMinutes(15);

    /// <summary>Receives one Discord interaction request from exact raw HTTP bytes.</summary>
    public static MessageReceiveResult<DiscordInboundInteraction> Receive(
        MessageInboundRequest request,
        string publicKeyHex,
        string signatureHex,
        string timestamp,
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
        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds)) {
            return Reject(401, MessageReceiveFailureKind.Unauthorized);
        }
        DateTimeOffset signedAt;
        try {
            signedAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException) {
            return Reject(401, MessageReceiveFailureKind.Unauthorized);
        }

        var body = request.CopyBody();
        if (!DiscordInteractionVerifier.VerifyRecent(
                publicKeyHex,
                signatureHex,
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
                MaxDepth = 64
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !TryRequiredInt(root, "type", out var typeValue) ||
                typeValue is < 1 or > 5) {
                return Reject(400, MessageReceiveFailureKind.Malformed);
            }

            var kind = (DiscordInteractionKind)typeValue;
            if (kind == DiscordInteractionKind.Ping) {
                return MessageReceiveResult<DiscordInboundInteraction>.Acknowledge(
                    DiscordInteractionAcknowledgement.Pong());
            }
            return ReceiveDispatchable(request, signatureHex, signedAt, root, kind);
        }
        catch (JsonException) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
    }

    private static MessageReceiveResult<DiscordInboundInteraction> ReceiveDispatchable(
        MessageInboundRequest request,
        string signatureHex,
        DateTimeOffset signedAt,
        JsonElement root,
        DiscordInteractionKind kind) {
        if (!TryRequiredSnowflake(root, "id", out var interactionId) ||
            !TryRequiredSnowflake(root, "application_id", out var applicationId) ||
            !TryRequired(root, "token", MaximumTokenLength, out var token) ||
            !root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Object ||
            !TryOptionalSnowflake(root, "guild_id", out var guildId) ||
            !TryOptionalSnowflake(root, "channel_id", out var channelId) ||
            !TryOptional(root, "locale", 64, out var locale) ||
            !TryOptional(root, "guild_locale", 64, out var guildLocale) ||
            !TryOptionalContext(root, out var context) ||
            !TryUserId(root, out var userId) ||
            !TryInstallationOwner(root, out var installationOwnerId) ||
            !TryNestedOptionalSnowflake(root, "message", "id", out var messageId)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }

        string name;
        MessageRoute route;
        MessageAcknowledgement acknowledgement;
        switch (kind) {
            case DiscordInteractionKind.ApplicationCommand:
                if (!TryRequired(data, "name", 128, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForCommand(name);
                acknowledgement = DiscordInteractionAcknowledgement.DeferredMessage();
                break;
            case DiscordInteractionKind.MessageComponent:
                if (!TryRequired(data, "custom_id", 100, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForAction(name);
                acknowledgement = DiscordInteractionAcknowledgement.DeferredUpdate();
                break;
            case DiscordInteractionKind.Autocomplete:
                if (!TryRequired(data, "name", 128, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForAutocomplete(name);
                acknowledgement = DiscordInteractionAcknowledgement.EmptyAutocomplete();
                break;
            case DiscordInteractionKind.ModalSubmit:
                if (!TryRequired(data, "custom_id", 100, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForSubmission(name);
                acknowledgement = DiscordInteractionAcknowledgement.DeferredMessage();
                break;
            default:
                return Reject(400, MessageReceiveFailureKind.Unsupported);
        }

        var scopeId = guildId ?? installationOwnerId ?? applicationId;
        var transientContext = new DiscordTransientInteractionContext(
            applicationId,
            token,
            signedAt.Add(InteractionTokenLifetime));
        var payload = new DiscordInboundInteraction(
            kind,
            name,
            installationOwnerId,
            locale,
            guildLocale,
            context,
            data.Clone(),
            transientContext);
        var deduplicationKey = CreateDeduplicationKey(request.InstallationId, signatureHex);
        var envelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            request.InstallationId,
            deduplicationKey,
            route.EventKind,
            request.ReceivedAt,
            payload) {
            EventId = interactionId,
            ScopeId = scopeId,
            SenderId = userId,
            CorrelationId = request.CorrelationId
        };
        if (channelId is not null) {
            var conversationKind = context is 1 or 2
                ? MessageConversationKind.DirectMessage
                : guildId is not null
                    ? MessageConversationKind.Channel
                    : MessageConversationKind.Unknown;
            envelope.Conversation = new MessageReference(MessageProviders.Discord) {
                InstallationId = request.InstallationId,
                ScopeId = scopeId,
                ConversationId = channelId,
                ConversationKind = conversationKind
            };
            if (messageId is not null) {
                envelope.Message = new MessageReference(MessageProviders.Discord, messageId) {
                    InstallationId = request.InstallationId,
                    ScopeId = scopeId,
                    ConversationId = channelId,
                    ConversationKind = conversationKind
                };
            }
        }
        return MessageReceiveResult<DiscordInboundInteraction>.Dispatch(
            route,
            envelope,
            acknowledgement);
    }

    private static string CreateDeduplicationKey(string installationId, string signatureHex) {
        byte[] hash;
        using (var sha256 = SHA256.Create()) {
            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(
                installationId + "\n" + signatureHex.ToLowerInvariant()));
        }
        var builder = new StringBuilder("discord-request:", 16 + (hash.Length * 2));
        foreach (var value in hash) {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    private static bool TryUserId(JsonElement root, out string userId) {
        userId = string.Empty;
        if (!TryNestedOptionalSnowflake(root, "member", "user", "id", out var memberUserId) ||
            !TryNestedOptionalSnowflake(root, "user", "id", out var directUserId) ||
            (memberUserId is not null && directUserId is not null &&
             !string.Equals(memberUserId, directUserId, StringComparison.Ordinal))) {
            return false;
        }
        userId = memberUserId ?? directUserId ?? string.Empty;
        return userId.Length > 0;
    }

    private static bool TryInstallationOwner(JsonElement root, out string? ownerId) {
        ownerId = null;
        if (!root.TryGetProperty("authorizing_integration_owners", out var owners)) {
            return true;
        }
        if (owners.ValueKind != JsonValueKind.Object) {
            return false;
        }
        if (!TryOptionalOwner(owners, "0", allowZero: true, out var guildOwner) ||
            !TryOptionalOwner(owners, "1", allowZero: false, out var userOwner)) {
            return false;
        }
        ownerId = guildOwner is not null and not "0" ? guildOwner : userOwner;
        return true;
    }

    private static bool TryOptionalOwner(
        JsonElement owners,
        string propertyName,
        bool allowZero,
        out string? ownerId) {
        ownerId = null;
        if (!owners.TryGetProperty(propertyName, out var property)) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String) {
            return false;
        }
        var candidate = property.GetString();
        if (allowZero && string.Equals(candidate, "0", StringComparison.Ordinal)) {
            ownerId = candidate;
            return true;
        }
        if (!DiscordSnowflake.TryNormalize(candidate, out var normalized)) {
            return false;
        }
        ownerId = normalized;
        return true;
    }

    private static bool TryOptionalContext(JsonElement root, out int? context) {
        context = null;
        if (!root.TryGetProperty("context", out var property)) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var value) ||
            value is < 0 or > 2) {
            return false;
        }
        context = value;
        return true;
    }

    private static bool TryRequiredInt(JsonElement root, string propertyName, out int value) {
        value = 0;
        return root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out value);
    }

    private static bool TryRequiredSnowflake(JsonElement root, string propertyName, out string value) {
        value = string.Empty;
        return root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            DiscordSnowflake.TryNormalize(property.GetString(), out value);
    }

    private static bool TryOptionalSnowflake(JsonElement root, string propertyName, out string? value) {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String ||
            !DiscordSnowflake.TryNormalize(property.GetString(), out var normalized)) {
            return false;
        }
        value = normalized;
        return true;
    }

    private static bool TryNestedOptionalSnowflake(
        JsonElement root,
        string ownerName,
        string propertyName,
        out string? value) {
        value = null;
        if (!root.TryGetProperty(ownerName, out var owner) || owner.ValueKind == JsonValueKind.Null) {
            return true;
        }
        return owner.ValueKind == JsonValueKind.Object &&
            TryOptionalSnowflake(owner, propertyName, out value);
    }

    private static bool TryNestedOptionalSnowflake(
        JsonElement root,
        string outerName,
        string ownerName,
        string propertyName,
        out string? value) {
        value = null;
        if (!root.TryGetProperty(outerName, out var outer) || outer.ValueKind == JsonValueKind.Null) {
            return true;
        }
        return outer.ValueKind == JsonValueKind.Object &&
            TryNestedOptionalSnowflake(outer, ownerName, propertyName, out value);
    }

    private static bool TryRequired(
        JsonElement root,
        string propertyName,
        int maximumLength,
        out string value) {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String) {
            return false;
        }
        return TryNormalize(property.GetString(), maximumLength, required: true, out value);
    }

    private static bool TryOptional(
        JsonElement root,
        string propertyName,
        int maximumLength,
        out string? value) {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String ||
            !TryNormalize(property.GetString(), maximumLength, required: false, out var normalized)) {
            return false;
        }
        value = normalized.Length == 0 ? null : normalized;
        return true;
    }

    private static bool TryNormalize(
        string? candidate,
        int maximumLength,
        bool required,
        out string value) {
        value = string.Empty;
        if (candidate is null || candidate.Length > maximumLength || candidate.Any(char.IsControl)) {
            return !required && candidate is null;
        }
        var normalized = candidate.Trim();
        if (normalized.Length == 0) {
            return !required;
        }
        value = normalized;
        return true;
    }

    private static bool IsJson(string contentType) {
        var mediaType = contentType.Split(';')[0].Trim();
        return string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static MessageReceiveResult<DiscordInboundInteraction> Reject(
        int statusCode,
        MessageReceiveFailureKind failureKind) =>
        MessageReceiveResult<DiscordInboundInteraction>.Reject(
            failureKind,
            MessageAcknowledgement.Empty(statusCode));
}
