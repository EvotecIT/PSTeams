using System.Text;

namespace MessageX.Hosting;

/// <summary>Creates bounded provider-data projections while removing capability fields recursively.</summary>
public static class MessageDurableJsonProjection {
    private const int MaximumDepth = 64;
    private const int MaximumBytes = 1024 * 1024;

    /// <summary>Clones provider data while omitting case-insensitive property names at every depth.</summary>
    public static MessageDataValue CreateSafeClone(
        MessageDataValue value,
        IEnumerable<string> forbiddenPropertyNames) {
        if (value is null) {
            throw new ArgumentNullException(nameof(value));
        }
        if (forbiddenPropertyNames is null) {
            throw new ArgumentNullException(nameof(forbiddenPropertyNames));
        }
        var forbidden = new HashSet<string>(forbiddenPropertyNames, StringComparer.OrdinalIgnoreCase);
        var clone = Clone(value, forbidden, 0);
        if (Encoding.UTF8.GetByteCount(clone.ToJsonString()) > MaximumBytes) {
            throw new MessageDurablePayloadException("A safe durable provider projection cannot exceed 1 MiB.");
        }
        return clone;
    }

    private static MessageDataValue Clone(
        MessageDataValue value,
        ISet<string> forbidden,
        int depth) {
        if (depth > MaximumDepth) {
            throw new MessageDurablePayloadException("A safe durable provider projection cannot exceed 64 levels.");
        }
        return value.Kind switch {
            MessageDataValueKind.Object => MessageDataValue.FromObject(
                value.Properties
                    .Where(property => !forbidden.Contains(property.Key))
                    .Select(property => new KeyValuePair<string, MessageDataValue>(
                        property.Key,
                        Clone(property.Value, forbidden, depth + 1)))),
            MessageDataValueKind.Array => MessageDataValue.FromArray(
                value.Items.Select(item => Clone(item, forbidden, depth + 1))),
            MessageDataValueKind.String or
            MessageDataValueKind.Number or
            MessageDataValueKind.Boolean or
            MessageDataValueKind.Null => value,
            _ => throw new MessageDurablePayloadException(
                "Undefined provider values cannot enter durable storage.")
        };
    }
}
