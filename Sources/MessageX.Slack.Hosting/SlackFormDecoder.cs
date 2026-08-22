using System.Text;

namespace MessageX.Slack;

internal static class SlackFormDecoder {
    private const int MaximumFields = 64;
    private const int MaximumKeyBytes = 128;
    private const int MaximumValueBytes = 512 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool TryDecode(byte[] body, out IReadOnlyDictionary<string, string> fields) {
        fields = new Dictionary<string, string>(StringComparer.Ordinal);
        if (body.Length == 0) {
            return false;
        }

        var decoded = new Dictionary<string, string>(StringComparer.Ordinal);
        var fieldStart = 0;
        while (fieldStart <= body.Length) {
            if (decoded.Count >= MaximumFields) {
                return false;
            }
            var fieldEnd = Array.IndexOf(body, (byte)'&', fieldStart);
            if (fieldEnd < 0) {
                fieldEnd = body.Length;
            }
            if (fieldEnd == fieldStart) {
                return false;
            }
            var equals = Array.IndexOf(body, (byte)'=', fieldStart, fieldEnd - fieldStart);
            if (equals < 0) {
                equals = fieldEnd;
            }
            if (!TryDecodeComponent(body, fieldStart, equals - fieldStart, MaximumKeyBytes, out var key) ||
                !TryDecodeComponent(body, equals < fieldEnd ? equals + 1 : fieldEnd, fieldEnd - Math.Min(equals + 1, fieldEnd), MaximumValueBytes, out var value) ||
                string.IsNullOrEmpty(key) ||
                key.Any(char.IsControl) ||
                decoded.ContainsKey(key)) {
                return false;
            }
            decoded.Add(key, value);
            if (fieldEnd == body.Length) {
                break;
            }
            fieldStart = fieldEnd + 1;
        }
        fields = decoded;
        return true;
    }

    private static bool TryDecodeComponent(
        byte[] source,
        int start,
        int length,
        int maximumBytes,
        out string value) {
        value = string.Empty;
        if (length > maximumBytes) {
            return false;
        }
        var bytes = new byte[length];
        var count = 0;
        for (var index = start; index < start + length; index++) {
            var current = source[index];
            if (current == (byte)'+') {
                bytes[count++] = (byte)' ';
                continue;
            }
            if (current != (byte)'%') {
                bytes[count++] = current;
                continue;
            }
            if (index + 2 >= start + length ||
                !TryDecodeNibble(source[index + 1], out var high) ||
                !TryDecodeNibble(source[index + 2], out var low)) {
                return false;
            }
            bytes[count++] = (byte)((high << 4) | low);
            index += 2;
        }
        try {
            value = StrictUtf8.GetString(bytes, 0, count);
            return true;
        }
        catch (DecoderFallbackException) {
            return false;
        }
    }

    private static bool TryDecodeNibble(byte value, out int nibble) {
        if (value is >= (byte)'0' and <= (byte)'9') {
            nibble = value - (byte)'0';
            return true;
        }
        if (value is >= (byte)'a' and <= (byte)'f') {
            nibble = value - (byte)'a' + 10;
            return true;
        }
        if (value is >= (byte)'A' and <= (byte)'F') {
            nibble = value - (byte)'A' + 10;
            return true;
        }
        nibble = 0;
        return false;
    }
}
