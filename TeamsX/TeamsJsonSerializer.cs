using System.Collections;

#if NET472
using System.Web.Script.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace TeamsX;

internal static class TeamsJsonSerializer {
#if NET472
    public static string Serialize(object? value) {
        var serializer = new JavaScriptSerializer {
            MaxJsonLength = int.MaxValue
        };

        return serializer.Serialize(PruneNulls(value));
    }
#else
    private static readonly JsonSerializerOptions SerializerOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string Serialize(object? value) {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }
#endif

    private static object? PruneNulls(object? value) {
        if (value is null) {
            return null;
        }

        if (value is IDictionary dictionary) {
            var normalized = new Dictionary<string, object?>();
            foreach (DictionaryEntry entry in dictionary) {
                var normalizedValue = PruneNulls(entry.Value);
                if (normalizedValue is not null) {
                    normalized[entry.Key?.ToString() ?? string.Empty] = normalizedValue;
                }
            }

            return normalized;
        }

        if (value is IEnumerable enumerable && value is not string) {
            var items = new List<object?>();
            foreach (var item in enumerable) {
                var normalizedItem = PruneNulls(item);
                if (normalizedItem is not null) {
                    items.Add(normalizedItem);
                }
            }

            return items;
        }

        return value;
    }
}
