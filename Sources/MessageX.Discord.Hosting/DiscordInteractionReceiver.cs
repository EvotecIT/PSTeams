using System.Globalization;
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
        TimeSpan? replayWindow = null,
        string? expectedApplicationId = null,
        string? expectedInstallationOwnerId = null,
        Func<DiscordInstallationContext, string?>? installationResolver = null) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }
        if (!IsJson(request.ContentType)) {
            return Reject(415, MessageReceiveFailureKind.Unsupported);
        }
        if (request.BodyLength is <= 0 or > MaximumBodyBytes) {
            return Reject(413, MessageReceiveFailureKind.Malformed);
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
            return ReceiveDispatchable(
                request,
                signatureHex,
                root,
                kind,
                expectedApplicationId,
                expectedInstallationOwnerId,
                installationResolver);
        }
        catch (JsonException) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
    }

    private static MessageReceiveResult<DiscordInboundInteraction> ReceiveDispatchable(
        MessageInboundRequest request,
        string signatureHex,
        JsonElement root,
        DiscordInteractionKind kind,
        string? expectedApplicationId,
        string? expectedInstallationOwnerId,
        Func<DiscordInstallationContext, string?>? installationResolver) {
        if (!TryRequiredSnowflake(root, "id", out var interactionId) ||
            !DiscordSnowflake.TryGetTimestamp(interactionId, out var interactionCreatedAt) ||
            !TryRequiredSnowflake(root, "application_id", out var applicationId) ||
            !TryRequired(root, "token", MaximumTokenLength, out var token) ||
            !root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Object ||
            !TryOptionalSnowflake(root, "guild_id", out var guildId) ||
            !TryOptionalSnowflake(root, "channel_id", out var channelId) ||
            !TryNestedOptionalInt(root, "channel", "type", out var channelType) ||
            !TryOptional(root, "locale", 64, out var locale) ||
            !TryOptional(root, "guild_locale", 64, out var guildLocale) ||
            !TryOptionalContext(root, out var context) ||
            !TryUserId(root, out var userId) ||
            !TryInstallationOwner(
                root,
                context,
                expectedInstallationOwnerId,
                out var installationOwnerId,
                out var installationType,
                out var authorizationOwnerId,
                out var installationOwnerMatched) ||
            !TryNestedOptionalSnowflake(root, "message", "id", out var messageId)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        if ((expectedApplicationId is not null &&
             !string.Equals(expectedApplicationId, applicationId, StringComparison.Ordinal)) ||
            (expectedInstallationOwnerId is not null && !installationOwnerMatched)) {
            return Reject(403, MessageReceiveFailureKind.Unauthorized);
        }
        var installationId = request.InstallationId;
        if (installationResolver is not null) {
            if (installationType is null || authorizationOwnerId is null) {
                return Reject(403, MessageReceiveFailureKind.Unauthorized);
            }
            var resolvedInstallationId = installationResolver(new DiscordInstallationContext(
                applicationId,
                installationType.Value,
                authorizationOwnerId));
            if (!IsInstallationId(resolvedInstallationId)) {
                return Reject(403, MessageReceiveFailureKind.Unauthorized);
            }
            installationId = resolvedInstallationId!;
        }

        string name;
        DiscordApplicationCommandType? commandType = null;
        string? targetId = null;
        MessageRoute route;
        MessageAcknowledgement acknowledgement;
        switch (kind) {
            case DiscordInteractionKind.ApplicationCommand:
                if (!TryRequired(data, "name", 128, out name) ||
                    !TryRequiredInt(data, "type", out var commandTypeValue) ||
                    commandTypeValue is < 1 or > 3) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                commandType = (DiscordApplicationCommandType)commandTypeValue;
                if (commandTypeValue is 2 or 3 && !TryRequiredSnowflake(data, "target_id", out targetId)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForCommand(
                    name,
                    commandTypeValue.ToString(CultureInfo.InvariantCulture));
                acknowledgement = DiscordInteractionAcknowledgement.DeferredMessage();
                break;
            case DiscordInteractionKind.MessageComponent:
                if (!TryRequiredOpaque(data, "custom_id", 100, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForAction(name);
                acknowledgement = DiscordInteractionAcknowledgement.DeferredUpdate();
                break;
            case DiscordInteractionKind.Autocomplete:
                if (!TryRequired(data, "name", 128, out name) ||
                    !TryRequiredInt(data, "type", out var autocompleteTypeValue) ||
                    autocompleteTypeValue != (int)DiscordApplicationCommandType.ChatInput) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                commandType = DiscordApplicationCommandType.ChatInput;
                route = MessageRoute.ForAutocomplete(name);
                acknowledgement = DiscordInteractionAcknowledgement.EmptyAutocomplete();
                break;
            case DiscordInteractionKind.ModalSubmit:
                if (!TryRequiredOpaque(data, "custom_id", 100, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                route = MessageRoute.ForSubmission(name);
                acknowledgement = DiscordInteractionAcknowledgement.DeferredMessage();
                break;
            default:
                return Reject(400, MessageReceiveFailureKind.Unsupported);
        }
        if (commandType == DiscordApplicationCommandType.Message) {
            if (messageId is not null &&
                !string.Equals(messageId, targetId, StringComparison.Ordinal)) {
                return Reject(400, MessageReceiveFailureKind.Malformed);
            }
            messageId = targetId;
        }

        var scopeId = guildId ?? installationOwnerId ?? applicationId;
        var transientContext = new DiscordTransientInteractionContext(
            applicationId,
            token,
            interactionCreatedAt.Add(InteractionTokenLifetime));
        var payload = new DiscordInboundInteraction(
            kind,
            name,
            installationOwnerId,
            locale,
            guildLocale,
            context,
            commandType,
            targetId,
            applicationId,
            userId,
            interactionId,
            guildId,
            channelId,
            channelType,
            messageId,
            MessageDurableJsonProjection.CreateSafeClone(
                MessageDataValue.ParseJson(data.GetRawText()),
                DiscordSafeInteractionData.ForbiddenPropertyNames),
            transientContext);
        var deduplicationKey = DiscordInteractionIdentity.CreateDeduplicationKey(
            installationId,
            interactionId);
        var envelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            installationId,
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
            var isThread = channelType is 10 or 11 or 12;
            var conversationKind = isThread
                ? MessageConversationKind.Thread
                : channelType is 1 or 3 || context is 1 or 2
                ? MessageConversationKind.DirectMessage
                : guildId is not null
                    ? MessageConversationKind.Channel
                    : MessageConversationKind.Unknown;
            envelope.Conversation = new MessageReference(MessageProviders.Discord) {
                InstallationId = installationId,
                ScopeId = scopeId,
                ConversationId = channelId,
                ThreadId = isThread ? channelId : null,
                ConversationKind = conversationKind
            };
            var effectiveMessageId = commandType == DiscordApplicationCommandType.Message
                ? targetId
                : messageId;
            if (effectiveMessageId is not null) {
                envelope.Message = new MessageReference(MessageProviders.Discord, effectiveMessageId) {
                    InstallationId = installationId,
                    ScopeId = scopeId,
                    ConversationId = channelId,
                    ThreadId = isThread ? channelId : null,
                    ConversationKind = conversationKind
                };
            }
        }
        return MessageReceiveResult<DiscordInboundInteraction>.Dispatch(
            route,
            envelope,
            acknowledgement,
            requiresSynchronousDispatch: kind == DiscordInteractionKind.Autocomplete);
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

    private static bool TryInstallationOwner(
        JsonElement root,
        int? context,
        string? expectedOwnerId,
        out string? ownerId,
        out int? integrationType,
        out string? authorizationOwnerId,
        out bool expectedOwnerMatched) {
        ownerId = null;
        integrationType = null;
        authorizationOwnerId = null;
        expectedOwnerMatched = expectedOwnerId is null;
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
        var normalizedGuildOwner = guildOwner is not null and not "0" ? guildOwner : null;
        var contextualOwner = context switch {
            0 => normalizedGuildOwner,
            1 or 2 => userOwner,
            _ => null
        };
        var contextualAuthorizationOwner = context switch {
            0 => guildOwner,
            1 or 2 => userOwner,
            _ => null
        };
        var contextualIntegrationType = context switch {
            0 => 0,
            1 or 2 => 1,
            _ => (int?)null
        };
        if (expectedOwnerId is not null) {
            if (contextualAuthorizationOwner is not null) {
                expectedOwnerMatched = string.Equals(
                    contextualAuthorizationOwner,
                    expectedOwnerId,
                    StringComparison.Ordinal);
                ownerId = contextualOwner;
                if (expectedOwnerMatched) {
                    integrationType = contextualIntegrationType;
                    authorizationOwnerId = contextualAuthorizationOwner;
                }
                return true;
            }
            if (string.Equals(guildOwner, expectedOwnerId, StringComparison.Ordinal)) {
                expectedOwnerMatched = true;
                ownerId = string.Equals(expectedOwnerId, "0", StringComparison.Ordinal)
                    ? null
                    : expectedOwnerId;
                integrationType = 0;
                authorizationOwnerId = guildOwner;
                return true;
            }
            if (string.Equals(userOwner, expectedOwnerId, StringComparison.Ordinal)) {
                expectedOwnerMatched = true;
                ownerId = expectedOwnerId;
                integrationType = 1;
                authorizationOwnerId = userOwner;
                return true;
            }
            return true;
        }
        if (contextualOwner is not null) {
            ownerId = contextualOwner;
            integrationType = contextualIntegrationType;
            authorizationOwnerId = contextualAuthorizationOwner;
            return true;
        }
        if (contextualAuthorizationOwner is not null) {
            integrationType = contextualIntegrationType;
            authorizationOwnerId = contextualAuthorizationOwner;
            return true;
        }
        if (normalizedGuildOwner is not null && userOwner is not null &&
            !string.Equals(normalizedGuildOwner, userOwner, StringComparison.Ordinal)) {
            return false;
        }
        ownerId = normalizedGuildOwner ?? userOwner;
        if (guildOwner is not null) {
            integrationType = 0;
            authorizationOwnerId = guildOwner;
        } else if (userOwner is not null) {
            integrationType = 1;
            authorizationOwnerId = userOwner;
        }
        return true;
    }

    private static bool IsInstallationId(string? value) =>
        value is not null &&
        value.Length is > 0 and <= 256 &&
        string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
        !value.Any(char.IsControl);

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

    private static bool TryNestedOptionalInt(
        JsonElement root,
        string ownerName,
        string propertyName,
        out int? value) {
        value = null;
        if (!root.TryGetProperty(ownerName, out var owner) || owner.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (owner.ValueKind != JsonValueKind.Object) {
            return false;
        }
        if (!owner.TryGetProperty(propertyName, out var property)) {
            return true;
        }
        if (
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var parsed) ||
            parsed is < 0 or > 16) {
            return false;
        }
        value = parsed;
        return true;
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

    private static bool TryRequiredOpaque(
        JsonElement root,
        string propertyName,
        int maximumLength,
        out string value) {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String) {
            return false;
        }
        var candidate = property.GetString();
        if (candidate is null || candidate.Length == 0 ||
            candidate.Length > maximumLength ||
            candidate.Any(char.IsControl)) {
            return false;
        }
        value = candidate;
        return true;
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
