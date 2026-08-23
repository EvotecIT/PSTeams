using System.Text;
using System.Text.Json;

namespace MessageX.Slack;

internal static class SlackRichTextProjection {
    private const int MaximumBytes = 40000;
    private const int MaximumPropertyNameLength = 256;
    private static readonly string[] ForbiddenProperties = {
        "token", "access_token", "refresh_token", "oauth_token", "bot_token",
        "interaction_token", "authorization", "client_secret", "signing_secret", "signature",
        "trigger_id", "response_url"
    };

    public static bool TryRead(
        JsonElement owner,
        string propertyName,
        out MessageDataValue? value) {
        value = null;
        if (!owner.TryGetProperty(propertyName, out var candidate) ||
            candidate.ValueKind == JsonValueKind.Null) {
            return true;
        }
        try {
            value = Normalize(MessageDataValue.ParseJson(candidate.GetRawText()));
            return true;
        } catch (Exception exception) when (IsProjectionException(exception)) {
            return false;
        }
    }

    public static MessageDataValue? Normalize(MessageDataValue? value) =>
        value is null ? null : NormalizeRequired(value);

    private static MessageDataValue NormalizeRequired(MessageDataValue value) {
        if (value.Kind != MessageDataValueKind.Object ||
            Encoding.UTF8.GetByteCount(value.ToJsonString()) > MaximumBytes) {
            throw new MessageDurablePayloadException(
                "A Slack rich-text input must be an object no larger than 40 KB.");
        }
        var clone = MessageDurableJsonProjection.CreateSafeClone(value, ForbiddenProperties);
        Validate(clone);
        if (Encoding.UTF8.GetByteCount(clone.ToJsonString()) > MaximumBytes) {
            throw new MessageDurablePayloadException("A Slack rich-text input cannot exceed 40 KB.");
        }
        return clone;
    }

    private static void Validate(MessageDataValue value) {
        switch (value.Kind) {
            case MessageDataValueKind.Object:
                foreach (var property in value.Properties) {
                    if (property.Key.Length > MaximumPropertyNameLength ||
                        property.Key.Any(char.IsControl)) {
                        throw new MessageDurablePayloadException(
                            "A Slack rich-text property name is unsafe.");
                    }
                    Validate(property.Value);
                }
                break;
            case MessageDataValueKind.Array:
                foreach (var item in value.Items) {
                    Validate(item);
                }
                break;
            case MessageDataValueKind.String:
                var text = value.GetString();
                if (text is null || text.Length > MaximumBytes || text.IndexOf('\0') >= 0) {
                    throw new MessageDurablePayloadException("A Slack rich-text value is unsafe.");
                }
                break;
            case MessageDataValueKind.Number:
            case MessageDataValueKind.Boolean:
            case MessageDataValueKind.Null:
                break;
            default:
                throw new MessageDurablePayloadException("A Slack rich-text value is malformed.");
        }
    }

    private static bool IsProjectionException(Exception exception) =>
        exception is MessageDurablePayloadException or JsonException or ArgumentException or
            InvalidOperationException or NotSupportedException;
}
