using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Verifies and parses Slack slash commands and HTTP interactive requests.</summary>
public static class SlackInteractionReceiver {
    private const int MaximumBodyBytes = 1024 * 1024;
    private const int MaximumCoordinateLength = 256;
    private const int MaximumTransientUrlLength = 4096;
    private static readonly TimeSpan DefaultReplayWindow = TimeSpan.FromMinutes(5);

    /// <summary>Receives one form-encoded Slack slash-command or interactive request.</summary>
    public static MessageReceiveResult<SlackInteractionEvent> Receive(
        MessageInboundRequest request,
        string signingSecret,
        string signature,
        string timestamp,
        TimeSpan? replayWindow = null) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }
        if (!IsForm(request.ContentType)) {
            return Reject(415, MessageReceiveFailureKind.Unsupported);
        }
        if (request.BodyLength is <= 0 or > MaximumBodyBytes) {
            return Reject(413, MessageReceiveFailureKind.Malformed);
        }

        var body = request.CopyBody();
        var verification = SlackRequestVerifier.VerifyRecentDetailed(
            signingSecret,
            signature,
            timestamp,
            body,
            request.ReceivedAt,
            replayWindow ?? DefaultReplayWindow);
        if (verification != SlackRequestVerificationResult.Valid) {
            return verification == SlackRequestVerificationResult.Stale
                ? Reject(401, MessageReceiveFailureKind.Stale)
                : Reject(401, MessageReceiveFailureKind.Unauthorized);
        }
        if (!SlackFormDecoder.TryDecode(body, out var fields)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        return fields.TryGetValue("payload", out var payload)
            ? ReceiveInteractive(request, signature, fields, payload)
            : ReceiveCommand(request, signature, fields);
    }

    private static MessageReceiveResult<SlackInteractionEvent> ReceiveCommand(
        MessageInboundRequest request,
        string signature,
        IReadOnlyDictionary<string, string> fields) {
        if (!TryRequired(fields, "command", 129, out var command) ||
            command[0] != '/' ||
            !TryRequired(fields, "user_id", MaximumCoordinateLength, out var userId) ||
            !TryOptional(fields, "team_id", MaximumCoordinateLength, out var teamId) ||
            !TryOptional(fields, "enterprise_id", MaximumCoordinateLength, out var enterpriseId) ||
            !TryOptional(fields, "channel_id", MaximumCoordinateLength, out var channelId) ||
            !TryOptional(fields, "trigger_id", MaximumCoordinateLength, out var triggerId) ||
            !TryOptional(fields, "response_url", MaximumTransientUrlLength, out var responseUrl) ||
            !TryOptionalText(fields, "text", 40000, out var text)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        var commandName = command.Substring(1);
        if (!IsRouteName(commandName)) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }

        var interaction = new SlackInteractionEvent(
            SlackInteractionKind.SlashCommand,
            commandName,
            text,
            null,
            new SlackTransientInteractionContext(triggerId, responseUrl));
        return Dispatch(
            request,
            signature,
            MessageRoute.ForCommand(commandName),
            interaction,
            teamId ?? enterpriseId,
            userId,
            channelId,
            null);
    }

    private static MessageReceiveResult<SlackInteractionEvent> ReceiveInteractive(
        MessageInboundRequest request,
        string signature,
        IReadOnlyDictionary<string, string> fields,
        string payload) {
        if (fields.Count != 1 || payload.Length is <= 0 or > 512 * 1024) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
        try {
            using var document = JsonDocument.Parse(payload, new JsonDocumentOptions {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !TryRequired(root, "type", 64, out var type) ||
                !TryNestedOptional(root, "team", "id", MaximumCoordinateLength, out var teamId) ||
                !TryNestedOptional(root, "enterprise", "id", MaximumCoordinateLength, out var enterpriseId) ||
                !TryNestedRequired(root, "user", "id", MaximumCoordinateLength, out var userId) ||
                !TryNestedOptional(root, "channel", "id", MaximumCoordinateLength, out var channelId) ||
                !TryOptional(root, "trigger_id", MaximumCoordinateLength, out var triggerId) ||
                !TryOptional(root, "response_url", MaximumTransientUrlLength, out var responseUrl)) {
                return Reject(400, MessageReceiveFailureKind.Malformed);
            }

            SlackInteractionKind kind;
            MessageRoute route;
            string name;
            string? messageTimestamp = null;
            SlackInteractionPayload providerPayload;
            if (string.Equals(type, "block_actions", StringComparison.Ordinal)) {
                if (!TrySingleAction(root, out name, out var action) ||
                    !TryNestedOptional(root, "container", "message_ts", 32, out messageTimestamp) ||
                    !TryNestedOptional(root, "container", "channel_id", MaximumCoordinateLength, out var containerChannel)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                channelId ??= containerChannel;
                kind = SlackInteractionKind.BlockAction;
                route = MessageRoute.ForAction(name);
                providerPayload = new SlackInteractionPayload(new[] { action }, null, null);
            } else if (string.Equals(type, "shortcut", StringComparison.Ordinal) ||
                       string.Equals(type, "message_action", StringComparison.Ordinal)) {
                if (!TryRequired(root, "callback_id", 128, out name)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                SlackMessageInput? message = null;
                if (string.Equals(type, "message_action", StringComparison.Ordinal)) {
                    if (!TryNestedRequired(root, "message", "ts", 32, out messageTimestamp) ||
                        !TryNestedOptionalText(root, "message", "text", 40000, out var messageText)) {
                        return Reject(400, MessageReceiveFailureKind.Malformed);
                    }
                    message = new SlackMessageInput(messageTimestamp, messageText);
                }
                kind = SlackInteractionKind.Shortcut;
                route = MessageRoute.ForAction(name);
                providerPayload = new SlackInteractionPayload(null, null, message);
            } else if (string.Equals(type, "view_submission", StringComparison.Ordinal)) {
                if (!TryReadViewSubmission(root, out name, out var view)) {
                    return Reject(400, MessageReceiveFailureKind.Malformed);
                }
                kind = SlackInteractionKind.ViewSubmission;
                route = MessageRoute.ForSubmission(name);
                providerPayload = new SlackInteractionPayload(null, view, null);
            } else {
                return MessageReceiveResult<SlackInteractionEvent>.Acknowledge(
                    MessageAcknowledgement.Empty(200));
            }
            if (!IsRouteName(name)) {
                return Reject(400, MessageReceiveFailureKind.Malformed);
            }

            var interaction = new SlackInteractionEvent(
                kind,
                name,
                null,
                providerPayload,
                new SlackTransientInteractionContext(triggerId, responseUrl));
            return Dispatch(
                request,
                signature,
                route,
                interaction,
                teamId ?? enterpriseId,
                userId,
                channelId,
                messageTimestamp);
        }
        catch (JsonException) {
            return Reject(400, MessageReceiveFailureKind.Malformed);
        }
    }

    private static MessageReceiveResult<SlackInteractionEvent> Dispatch(
        MessageInboundRequest request,
        string signature,
        MessageRoute route,
        SlackInteractionEvent interaction,
        string? scopeId,
        string userId,
        string? channelId,
        string? messageTimestamp) {
        var deduplicationKey = CreateDeduplicationKey(request.InstallationId, signature);
        var envelope = new MessageEventEnvelope<SlackInteractionEvent>(
            MessageProviders.Slack,
            request.InstallationId,
            deduplicationKey,
            route.EventKind,
            request.ReceivedAt,
            interaction) {
            EventId = deduplicationKey,
            ScopeId = scopeId,
            SenderId = userId,
            CorrelationId = request.CorrelationId
        };
        if (channelId is not null) {
            var conversationKind = channelId.StartsWith("C", StringComparison.Ordinal)
                ? MessageConversationKind.Channel
                : channelId.StartsWith("D", StringComparison.Ordinal)
                    ? MessageConversationKind.DirectMessage
                    : MessageConversationKind.Unknown;
            envelope.Conversation = new MessageReference(MessageProviders.Slack) {
                InstallationId = request.InstallationId,
                ScopeId = scopeId,
                ConversationId = channelId,
                ConversationKind = conversationKind
            };
            var parsedTimestamp = SlackMessageValidator.ParseTimestamp(messageTimestamp);
            if (messageTimestamp is not null && parsedTimestamp is not null) {
                envelope.Message = new MessageReference(MessageProviders.Slack) {
                    InstallationId = request.InstallationId,
                    ScopeId = scopeId,
                    ConversationId = channelId,
                    ConversationKind = conversationKind,
                    MessageId = messageTimestamp,
                    Timestamp = parsedTimestamp
                };
            }
        }
        return MessageReceiveResult<SlackInteractionEvent>.Dispatch(
            route,
            envelope,
            MessageAcknowledgement.Empty(200));
    }

    private static bool TrySingleAction(
        JsonElement root,
        out string name,
        out SlackActionInput actionValue) {
        name = string.Empty;
        actionValue = null!;
        if (!root.TryGetProperty("actions", out var actions) ||
            actions.ValueKind != JsonValueKind.Array ||
            actions.GetArrayLength() != 1) {
            return false;
        }
        var action = actions[0];
        return action.ValueKind == JsonValueKind.Object &&
            TryRequired(action, "action_id", 128, out name) &&
            TryCreateActionInput(action, name, null, out actionValue);
    }

    private static bool TryReadViewSubmission(
        JsonElement root,
        out string callbackId,
        out SlackViewSubmissionInput view) {
        callbackId = string.Empty;
        view = null!;
        if (!root.TryGetProperty("view", out var viewElement) ||
            viewElement.ValueKind != JsonValueKind.Object ||
            !TryRequired(viewElement, "callback_id", 128, out callbackId)) {
            return false;
        }

        var values = new List<SlackViewStateInput>();
        if (viewElement.TryGetProperty("state", out var state)) {
            if (state.ValueKind != JsonValueKind.Object ||
                !state.TryGetProperty("values", out var stateValues) ||
                stateValues.ValueKind != JsonValueKind.Object) {
                return false;
            }
            foreach (var block in stateValues.EnumerateObject()) {
                if (!TryNormalizeCoordinate(block.Name, 128, required: true, out var blockId) ||
                    block.Value.ValueKind != JsonValueKind.Object) {
                    return false;
                }
                foreach (var input in block.Value.EnumerateObject()) {
                    if (values.Count >= 256 ||
                        !TryNormalizeCoordinate(input.Name, 128, required: true, out var actionId) ||
                        input.Value.ValueKind != JsonValueKind.Object ||
                        !TryCreateActionInput(input.Value, actionId, blockId, out var action)) {
                        return false;
                    }
                    values.Add(new SlackViewStateInput(
                        blockId,
                        action.ActionId,
                        action.Type,
                        action.Value,
                        action.SelectedValues));
                }
            }
        }
        view = new SlackViewSubmissionInput(callbackId, values.ToArray());
        return true;
    }

    private static bool TryCreateActionInput(
        JsonElement action,
        string actionId,
        string? blockIdOverride,
        out SlackActionInput value) {
        value = null!;
        if (!TryRequired(action, "type", 64, out var type) ||
            !TryOptional(action, "block_id", 128, out var blockId) ||
            !TryOptionalText(action, "value", 40000, out var scalarValue) ||
            !TryReadSelectedValues(action, out var selectedValues)) {
            return false;
        }
        value = new SlackActionInput(
            actionId,
            type,
            blockIdOverride ?? blockId,
            scalarValue,
            selectedValues);
        return true;
    }

    private static bool TryReadSelectedValues(JsonElement action, out string[] values) {
        var selected = new List<string>();
        if (!TryAppendOptionalText(action, "selected_user", selected) ||
            !TryAppendOptionalText(action, "selected_conversation", selected) ||
            !TryAppendOptionalText(action, "selected_channel", selected) ||
            !TryAppendOptionalText(action, "selected_date", selected) ||
            !TryAppendOptionalText(action, "selected_time", selected) ||
            !TryAppendOptionalObjectValue(action, "selected_option", selected) ||
            !TryAppendOptionalArray(action, "selected_users", selected) ||
            !TryAppendOptionalArray(action, "selected_conversations", selected) ||
            !TryAppendOptionalArray(action, "selected_channels", selected) ||
            !TryAppendOptionalObjectArray(action, "selected_options", selected)) {
            values = Array.Empty<string>();
            return false;
        }
        values = selected.ToArray();
        return true;
    }

    private static bool TryAppendOptionalText(
        JsonElement element,
        string propertyName,
        ICollection<string> values) {
        if (!TryOptionalText(element, propertyName, 40000, out var value)) {
            return false;
        }
        if (value is not null) {
            values.Add(value);
        }
        return values.Count <= 100;
    }

    private static bool TryAppendOptionalObjectValue(
        JsonElement element,
        string propertyName,
        ICollection<string> values) {
        if (!element.TryGetProperty(propertyName, out var owner) || owner.ValueKind == JsonValueKind.Null) {
            return true;
        }
        return owner.ValueKind == JsonValueKind.Object &&
            TryAppendOptionalText(owner, "value", values);
    }

    private static bool TryAppendOptionalArray(
        JsonElement element,
        string propertyName,
        ICollection<string> values) {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() > 100) {
            return false;
        }
        foreach (var item in array.EnumerateArray()) {
            if (item.ValueKind != JsonValueKind.String ||
                !TryAppendValue(item.GetString(), values)) {
                return false;
            }
        }
        return true;
    }

    private static bool TryAppendOptionalObjectArray(
        JsonElement element,
        string propertyName,
        ICollection<string> values) {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() > 100) {
            return false;
        }
        foreach (var item in array.EnumerateArray()) {
            if (item.ValueKind != JsonValueKind.Object ||
                !TryOptionalText(item, "value", 40000, out var itemValue) ||
                itemValue is null ||
                !TryAppendValue(itemValue, values)) {
                return false;
            }
        }
        return true;
    }

    private static bool TryAppendValue(string? candidate, ICollection<string> values) {
        if (candidate is null || candidate.Length > 40000 || candidate.IndexOf('\0') >= 0 || values.Count >= 100) {
            return false;
        }
        values.Add(candidate);
        return true;
    }

    private static string CreateDeduplicationKey(string installationId, string signature) {
        byte[] hash;
        using (var sha256 = SHA256.Create()) {
            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(
                installationId + "\n" + signature.ToLowerInvariant()));
        }
        var builder = new StringBuilder("slack-request:", 14 + (hash.Length * 2));
        foreach (var value in hash) {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    private static bool TryRequired(
        IReadOnlyDictionary<string, string> fields,
        string key,
        int maximumLength,
        out string value) {
        value = string.Empty;
        return fields.TryGetValue(key, out var candidate) &&
            TryNormalizeCoordinate(candidate, maximumLength, required: true, out value);
    }

    private static bool TryOptional(
        IReadOnlyDictionary<string, string> fields,
        string key,
        int maximumLength,
        out string? value) {
        value = null;
        if (!fields.TryGetValue(key, out var candidate)) {
            return true;
        }
        var accepted = TryNormalizeCoordinate(
            candidate,
            maximumLength,
            required: false,
            out string normalized);
        value = normalized.Length == 0 ? null : normalized;
        return accepted;
    }

    private static bool TryOptionalText(
        IReadOnlyDictionary<string, string> fields,
        string key,
        int maximumLength,
        out string? value) {
        value = null;
        if (!fields.TryGetValue(key, out var candidate)) {
            return true;
        }
        if (candidate.Length > maximumLength || candidate.IndexOf('\0') >= 0) {
            return false;
        }
        value = candidate;
        return true;
    }

    private static bool TryRequired(
        JsonElement element,
        string propertyName,
        int maximumLength,
        out string value) {
        value = string.Empty;
        return element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            TryNormalizeCoordinate(property.GetString(), maximumLength, required: true, out value);
    }

    private static bool TryOptional(
        JsonElement element,
        string propertyName,
        int maximumLength,
        out string? value) {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property)) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String) {
            return false;
        }
        var accepted = TryNormalizeCoordinate(
            property.GetString(),
            maximumLength,
            required: false,
            out string normalized);
        value = normalized.Length == 0 ? null : normalized;
        return accepted;
    }

    private static bool TryOptionalText(
        JsonElement element,
        string propertyName,
        int maximumLength,
        out string? value) {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null) {
            return true;
        }
        if (property.ValueKind != JsonValueKind.String) {
            return false;
        }
        var candidate = property.GetString();
        if (candidate is null || candidate.Length > maximumLength || candidate.IndexOf('\0') >= 0) {
            return false;
        }
        value = candidate;
        return true;
    }

    private static bool TryNestedRequired(
        JsonElement root,
        string objectName,
        string propertyName,
        int maximumLength,
        out string value) {
        value = string.Empty;
        return root.TryGetProperty(objectName, out var owner) &&
            owner.ValueKind == JsonValueKind.Object &&
            TryRequired(owner, propertyName, maximumLength, out value);
    }

    private static bool TryNestedOptional(
        JsonElement root,
        string objectName,
        string propertyName,
        int maximumLength,
        out string? value) {
        value = null;
        if (!root.TryGetProperty(objectName, out var owner)) {
            return true;
        }
        if (owner.ValueKind == JsonValueKind.Null) {
            return true;
        }
        return owner.ValueKind == JsonValueKind.Object &&
            TryOptional(owner, propertyName, maximumLength, out value);
    }

    private static bool TryNestedOptionalText(
        JsonElement root,
        string objectName,
        string propertyName,
        int maximumLength,
        out string? value) {
        value = null;
        if (!root.TryGetProperty(objectName, out var owner)) {
            return true;
        }
        if (owner.ValueKind == JsonValueKind.Null) {
            return true;
        }
        return owner.ValueKind == JsonValueKind.Object &&
            TryOptionalText(owner, propertyName, maximumLength, out value);
    }

    private static bool TryNormalizeCoordinate(
        string? candidate,
        int maximumLength,
        bool required,
        out string value) {
        value = string.Empty;
        if (candidate is null ||
            candidate.Length > maximumLength ||
            candidate.Any(char.IsControl)) {
            return !required && candidate is null;
        }
        var normalized = candidate.Trim();
        if (normalized.Length == 0) {
            return !required;
        }
        value = normalized;
        return true;
    }

    private static bool IsRouteName(string value) =>
        value.Length is > 0 and <= 128 && !value.Any(char.IsControl);

    private static bool IsForm(string contentType) {
        var mediaType = contentType.Split(';')[0].Trim();
        return string.Equals(
            mediaType,
            "application/x-www-form-urlencoded",
            StringComparison.OrdinalIgnoreCase);
    }

    private static MessageReceiveResult<SlackInteractionEvent> Reject(
        int statusCode,
        MessageReceiveFailureKind failureKind) =>
        MessageReceiveResult<SlackInteractionEvent>.Reject(
            failureKind,
            MessageAcknowledgement.Empty(statusCode));
}
