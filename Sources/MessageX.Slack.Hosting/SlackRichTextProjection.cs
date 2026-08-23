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
        out JsonElement? value) {
        value = null;
        if (!owner.TryGetProperty(propertyName, out var candidate) ||
            candidate.ValueKind == JsonValueKind.Null) {
            return true;
        }
        try {
            value = Normalize(candidate);
            return true;
        } catch (Exception exception) when (IsProjectionException(exception)) {
            return false;
        }
    }

    public static JsonElement? Normalize(JsonElement? value) =>
        value is null ? null : Normalize(value.Value);

    private static JsonElement Normalize(JsonElement value) {
        if (value.ValueKind != JsonValueKind.Object ||
            Encoding.UTF8.GetByteCount(value.GetRawText()) > MaximumBytes) {
            throw new MessageDurablePayloadException(
                "A Slack rich-text input must be an object no larger than 40 KB.");
        }
        var clone = MessageDurableJsonProjection.CreateSafeClone(value, ForbiddenProperties);
        Validate(clone);
        if (JsonSerializer.SerializeToUtf8Bytes(clone).Length > MaximumBytes) {
            throw new MessageDurablePayloadException("A Slack rich-text input cannot exceed 40 KB.");
        }
        return clone;
    }

    private static void Validate(JsonElement value) {
        switch (value.ValueKind) {
            case JsonValueKind.Object:
                foreach (var property in value.EnumerateObject()) {
                    if (property.Name.Length > MaximumPropertyNameLength ||
                        property.Name.Any(char.IsControl)) {
                        throw new MessageDurablePayloadException(
                            "A Slack rich-text property name is unsafe.");
                    }
                    Validate(property.Value);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray()) {
                    Validate(item);
                }
                break;
            case JsonValueKind.String:
                var text = value.GetString();
                if (text is null || text.Length > MaximumBytes || text.IndexOf('\0') >= 0) {
                    throw new MessageDurablePayloadException("A Slack rich-text value is unsafe.");
                }
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new MessageDurablePayloadException("A Slack rich-text value is malformed.");
        }
    }

    private static bool IsProjectionException(Exception exception) =>
        exception is MessageDurablePayloadException or JsonException or ArgumentException or
            InvalidOperationException or NotSupportedException;
}
