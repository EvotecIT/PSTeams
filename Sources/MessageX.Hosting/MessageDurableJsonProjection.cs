using System.Text.Json;

namespace MessageX.Hosting;

/// <summary>Creates bounded JSON projections while removing provider capability fields recursively.</summary>
public static class MessageDurableJsonProjection {
    private const int MaximumDepth = 64;
    private const int MaximumBytes = 1024 * 1024;

    /// <summary>Clones JSON while omitting case-insensitive property names at every depth.</summary>
    public static JsonElement CreateSafeClone(
        JsonElement value,
        IEnumerable<string> forbiddenPropertyNames) {
        if (forbiddenPropertyNames is null) {
            throw new ArgumentNullException(nameof(forbiddenPropertyNames));
        }
        var forbidden = new HashSet<string>(forbiddenPropertyNames, StringComparer.OrdinalIgnoreCase);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) {
            Write(value, writer, forbidden, 0);
        }
        if (stream.Length > MaximumBytes) {
            throw new MessageDurablePayloadException("A safe durable JSON projection cannot exceed 1 MiB.");
        }
        using var document = JsonDocument.Parse(stream.ToArray(), new JsonDocumentOptions { MaxDepth = MaximumDepth });
        return document.RootElement.Clone();
    }

    private static void Write(
        JsonElement value,
        Utf8JsonWriter writer,
        ISet<string> forbidden,
        int depth) {
        if (depth > MaximumDepth) {
            throw new MessageDurablePayloadException("A safe durable JSON projection cannot exceed 64 levels.");
        }
        switch (value.ValueKind) {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in value.EnumerateObject()) {
                    if (forbidden.Contains(property.Name)) {
                        continue;
                    }
                    writer.WritePropertyName(property.Name);
                    Write(property.Value, writer, forbidden, depth + 1);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray()) {
                    Write(item, writer, forbidden, depth + 1);
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
                throw new MessageDurablePayloadException("Undefined JSON values cannot enter durable storage.");
        }
    }
}
