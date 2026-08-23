using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX;

/// <summary>Serializer-neutral immutable value for bounded provider data.</summary>
[JsonConverter(typeof(MessageDataValueJsonConverter))]
public sealed class MessageDataValue {
    private static readonly IReadOnlyDictionary<string, MessageDataValue> EmptyProperties =
        new ReadOnlyDictionary<string, MessageDataValue>(
            new Dictionary<string, MessageDataValue>(StringComparer.Ordinal));
    private static readonly IReadOnlyList<MessageDataValue> EmptyItems = Array.Empty<MessageDataValue>();
    private readonly string? _text;
    private readonly bool _boolean;
    private readonly IReadOnlyDictionary<string, MessageDataValue>? _properties;
    private readonly IReadOnlyList<MessageDataValue>? _items;

    private MessageDataValue(
        MessageDataValueKind kind,
        string? text = null,
        bool boolean = false,
        IReadOnlyDictionary<string, MessageDataValue>? properties = null,
        IReadOnlyList<MessageDataValue>? items = null) {
        Kind = kind;
        _text = text;
        _boolean = boolean;
        _properties = properties;
        _items = items;
    }

    /// <summary>The JSON-compatible value kind without exposing a serializer-owned type.</summary>
    public MessageDataValueKind Kind { get; }

    /// <summary>Object properties, or an empty collection for other value kinds.</summary>
    public IReadOnlyDictionary<string, MessageDataValue> Properties => _properties ?? EmptyProperties;

    /// <summary>Array items, or an empty collection for other value kinds.</summary>
    public IReadOnlyList<MessageDataValue> Items => _items ?? EmptyItems;

    /// <summary>Number of object properties or array items; zero for scalar values.</summary>
    public int Count => Kind switch {
        MessageDataValueKind.Object => Properties.Count,
        MessageDataValueKind.Array => Items.Count,
        _ => 0
    };

    /// <summary>Gets one array item.</summary>
    public MessageDataValue this[int index] => Kind == MessageDataValueKind.Array
        ? Items[index]
        : throw new InvalidOperationException("The provider value is not an array.");

    /// <summary>Parses one JSON-compatible value into the owned immutable representation.</summary>
    public static MessageDataValue ParseJson(string json) {
        if (json is null) {
            throw new ArgumentNullException(nameof(json));
        }
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 64
        });
        return FromElement(document.RootElement, 0);
    }

    /// <summary>Creates a null provider value.</summary>
    public static MessageDataValue Null() => new(MessageDataValueKind.Null);

    /// <summary>Creates a string provider value.</summary>
    public static MessageDataValue FromString(string value) =>
        new(MessageDataValueKind.String, value ?? throw new ArgumentNullException(nameof(value)));

    /// <summary>Creates a Boolean provider value.</summary>
    public static MessageDataValue FromBoolean(bool value) => new(MessageDataValueKind.Boolean, boolean: value);

    /// <summary>Creates a number while preserving its exact JSON representation.</summary>
    public static MessageDataValue FromNumber(string value) {
        if (value is null) {
            throw new ArgumentNullException(nameof(value));
        }
        using var document = JsonDocument.Parse(value);
        if (document.RootElement.ValueKind != JsonValueKind.Number) {
            throw new ArgumentException("A valid JSON number is required.", nameof(value));
        }
        return new MessageDataValue(MessageDataValueKind.Number, document.RootElement.GetRawText());
    }

    /// <summary>Creates an immutable provider object.</summary>
    public static MessageDataValue FromObject(
        IEnumerable<KeyValuePair<string, MessageDataValue>> properties) {
        if (properties is null) {
            throw new ArgumentNullException(nameof(properties));
        }
        var values = new Dictionary<string, MessageDataValue>(StringComparer.Ordinal);
        foreach (var property in properties) {
            if (property.Key is null) {
                throw new ArgumentException("Provider object property names cannot be null.", nameof(properties));
            }
            if (property.Value is null) {
                throw new ArgumentException("Provider object values cannot be null.", nameof(properties));
            }
            if (values.ContainsKey(property.Key)) {
                throw new ArgumentException("Provider object property names must be unique.", nameof(properties));
            }
            values.Add(property.Key, property.Value);
        }
        return new MessageDataValue(
            MessageDataValueKind.Object,
            properties: new ReadOnlyDictionary<string, MessageDataValue>(values));
    }

    /// <summary>Creates an immutable provider array.</summary>
    public static MessageDataValue FromArray(IEnumerable<MessageDataValue> items) {
        if (items is null) {
            throw new ArgumentNullException(nameof(items));
        }
        var values = items.ToArray();
        if (values.Any(static value => value is null)) {
            throw new ArgumentException("Provider array values cannot be null.", nameof(items));
        }
        return new MessageDataValue(
            MessageDataValueKind.Array,
            items: Array.AsReadOnly(values));
    }

    /// <summary>Gets a required object property.</summary>
    public MessageDataValue GetProperty(string name) {
        if (!TryGetProperty(name, out var value)) {
            throw new KeyNotFoundException($"The provider object does not contain property '{name}'.");
        }
        return value;
    }

    /// <summary>Attempts to get one object property.</summary>
    public bool TryGetProperty(string name, out MessageDataValue value) {
        if (name is null) {
            throw new ArgumentNullException(nameof(name));
        }
        if (Kind == MessageDataValueKind.Object && Properties.TryGetValue(name, out var candidate)) {
            value = candidate;
            return true;
        }
        value = null!;
        return false;
    }

    /// <summary>Gets a string value, or null for a null value.</summary>
    public string? GetString() => Kind switch {
        MessageDataValueKind.String => _text,
        MessageDataValueKind.Null => null,
        _ => throw new InvalidOperationException("The provider value is not a string or null.")
    };

    /// <summary>Gets a Boolean value.</summary>
    public bool GetBoolean() => Kind == MessageDataValueKind.Boolean
        ? _boolean
        : throw new InvalidOperationException("The provider value is not Boolean.");

    /// <summary>Gets the exact JSON number text.</summary>
    public string GetNumberText() => Kind == MessageDataValueKind.Number
        ? _text!
        : throw new InvalidOperationException("The provider value is not a number.");

    /// <summary>Attempts to read an exact 32-bit integer number.</summary>
    public bool TryGetInt32(out int value) {
        value = default;
        return Kind == MessageDataValueKind.Number &&
            int.TryParse(_text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Serializes the owned value to its canonical JSON representation.</summary>
    public string ToJsonString() {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) {
            Write(writer);
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <inheritdoc />
    public override string ToString() => ToJsonString();

    internal static MessageDataValue FromElement(JsonElement element, int depth) {
        if (depth > 64) {
            throw new JsonException("Provider data cannot exceed 64 levels.");
        }
        switch (element.ValueKind) {
            case JsonValueKind.Object:
                var properties = new List<KeyValuePair<string, MessageDataValue>>();
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject()) {
                    if (!names.Add(property.Name)) {
                        throw new JsonException("Provider object property names must be unique.");
                    }
                    properties.Add(new KeyValuePair<string, MessageDataValue>(
                        property.Name,
                        FromElement(property.Value, depth + 1)));
                }
                return FromObject(properties);
            case JsonValueKind.Array:
                return FromArray(element.EnumerateArray().Select(value => FromElement(value, depth + 1)));
            case JsonValueKind.String:
                return FromString(element.GetString()!);
            case JsonValueKind.Number:
                return new MessageDataValue(MessageDataValueKind.Number, element.GetRawText());
            case JsonValueKind.True:
                return FromBoolean(true);
            case JsonValueKind.False:
                return FromBoolean(false);
            case JsonValueKind.Null:
                return Null();
            default:
                throw new JsonException("Undefined provider values are not supported.");
        }
    }

    internal void Write(Utf8JsonWriter writer) {
        switch (Kind) {
            case MessageDataValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in Properties) {
                    writer.WritePropertyName(property.Key);
                    property.Value.Write(writer);
                }
                writer.WriteEndObject();
                break;
            case MessageDataValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in Items) {
                    item.Write(writer);
                }
                writer.WriteEndArray();
                break;
            case MessageDataValueKind.String:
                writer.WriteStringValue(_text);
                break;
            case MessageDataValueKind.Number:
                writer.WriteRawValue(_text!, skipInputValidation: false);
                break;
            case MessageDataValueKind.Boolean:
                writer.WriteBooleanValue(_boolean);
                break;
            case MessageDataValueKind.Null:
                writer.WriteNullValue();
                break;
            default:
                throw new InvalidOperationException("The provider value kind is unsupported.");
        }
    }
}

/// <summary>Value kinds supported by <see cref="MessageDataValue"/>.</summary>
public enum MessageDataValueKind {
    /// <summary>An object with named properties.</summary>
    Object = 0,
    /// <summary>An ordered array.</summary>
    Array = 1,
    /// <summary>A string.</summary>
    String = 2,
    /// <summary>An exact JSON number.</summary>
    Number = 3,
    /// <summary>A Boolean value.</summary>
    Boolean = 4,
    /// <summary>A null value.</summary>
    Null = 5
}

internal sealed class MessageDataValueJsonConverter : JsonConverter<MessageDataValue> {
    public override MessageDataValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        using var document = JsonDocument.ParseValue(ref reader);
        return MessageDataValue.FromElement(document.RootElement, 0);
    }

    public override void Write(
        Utf8JsonWriter writer,
        MessageDataValue value,
        JsonSerializerOptions options) => value.Write(writer);
}
