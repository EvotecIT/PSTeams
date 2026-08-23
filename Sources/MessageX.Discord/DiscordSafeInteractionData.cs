using System.Text.Json;

namespace MessageX.Discord;

internal static class DiscordSafeInteractionData {
    private const int MaximumDepth = 64;
    private const int MaximumBytes = 1024 * 1024;
    private static readonly HashSet<string> ForbiddenProperties = new(
        new[] {
            "token", "access_token", "refresh_token", "oauth_token", "bot_token",
            "interaction_token", "authorization", "client_secret", "public_key", "signature"
        },
        StringComparer.OrdinalIgnoreCase);

    public static JsonElement Create(JsonElement value) {
        if (value.ValueKind == JsonValueKind.Undefined) {
            using var empty = JsonDocument.Parse("{}");
            return empty.RootElement.Clone();
        }
        if (value.ValueKind != JsonValueKind.Object) {
            throw new ArgumentException("Discord interaction data must be a JSON object.", nameof(value));
        }
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) {
            Write(value, writer, 0);
        }
        if (stream.Length > MaximumBytes) {
            throw new ArgumentException("Discord interaction data cannot exceed 1 MiB.", nameof(value));
        }
        using var document = JsonDocument.Parse(
            stream.ToArray(),
            new JsonDocumentOptions { MaxDepth = MaximumDepth });
        return document.RootElement.Clone();
    }

    private static void Write(JsonElement value, Utf8JsonWriter writer, int depth) {
        if (depth > MaximumDepth) {
            throw new ArgumentException("Discord interaction data cannot exceed 64 levels.", nameof(value));
        }
        switch (value.ValueKind) {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in value.EnumerateObject()) {
                    if (ForbiddenProperties.Contains(property.Name)) {
                        continue;
                    }
                    writer.WritePropertyName(property.Name);
                    Write(property.Value, writer, depth + 1);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray()) {
                    Write(item, writer, depth + 1);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(value.GetString());
                break;
            case JsonValueKind.Number:
                value.WriteTo(writer);
                break;
            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;
            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;
            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
            default:
                throw new ArgumentException("Discord interaction data contains an unsupported JSON value.", nameof(value));
        }
    }
}
