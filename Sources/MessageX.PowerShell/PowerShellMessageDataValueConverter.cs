using System.Collections;
using System.Globalization;
using System.Management.Automation;

namespace MessageX.PowerShell;

internal static class PowerShellMessageDataValueConverter {
    private const int MaximumDepth = 32;
    private const int MaximumCollectionItems = 1024;

    public static MessageDataValue? FromDictionary(IDictionary? dictionary) =>
        dictionary is null ? null : ConvertValue(dictionary, 0);

    private static MessageDataValue ConvertValue(object? value, int depth) {
        if (depth > MaximumDepth) {
            throw new ArgumentException("Provider data exceeds the supported nesting depth.", nameof(value));
        }
        value = value is PSObject psObject ? psObject.BaseObject : value;
        return value switch {
            null => MessageDataValue.Null(),
            MessageDataValue dataValue => dataValue,
            string text => MessageDataValue.FromString(text),
            char character => MessageDataValue.FromString(character.ToString()),
            bool boolean => MessageDataValue.FromBoolean(boolean),
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
                MessageDataValue.FromNumber(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture)),
            IDictionary dictionary => ConvertDictionary(dictionary, depth + 1),
            IEnumerable values => ConvertSequence(values, depth + 1),
            _ => throw new ArgumentException(
                $"Provider data value type '{value.GetType().FullName}' is not JSON-compatible.",
                nameof(value))
        };
    }

    private static MessageDataValue ConvertDictionary(IDictionary dictionary, int depth) {
        if (dictionary.Count > MaximumCollectionItems) {
            throw new ArgumentException("Provider objects exceed the supported property count.", nameof(dictionary));
        }
        var values = new List<KeyValuePair<string, MessageDataValue>>(dictionary.Count);
        foreach (DictionaryEntry entry in dictionary) {
            var name = entry.Key?.ToString();
            if (string.IsNullOrEmpty(name) || name.Any(char.IsControl)) {
                throw new ArgumentException("Provider object property names must be non-empty bounded text.", nameof(dictionary));
            }
            values.Add(new KeyValuePair<string, MessageDataValue>(name!, ConvertValue(entry.Value, depth)));
        }
        return MessageDataValue.FromObject(values);
    }

    private static MessageDataValue ConvertSequence(IEnumerable sequence, int depth) {
        var values = new List<MessageDataValue>();
        foreach (var item in sequence) {
            if (values.Count >= MaximumCollectionItems) {
                throw new ArgumentException("Provider arrays exceed the supported item count.", nameof(sequence));
            }
            values.Add(ConvertValue(item, depth));
        }
        return MessageDataValue.FromArray(values);
    }
}
