namespace MessageX.Hosting;

internal static class MessageDurableValidation {
    public static string Required(string? value, string parameterName, int maximumLength = 256) {
        if (value is null || value.Length > maximumLength || value.Any(char.IsControl)) {
            throw new ArgumentException(
                "Durable coordinates must be bounded non-empty text without control characters.",
                parameterName);
        }
        var normalized = value.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException(
                "Durable coordinates must be bounded non-empty text without control characters.",
                parameterName)
            : normalized;
    }

    public static string RequiredOpaque(string? value, string parameterName, int maximumLength = 256) {
        if (value is null ||
            value.Length == 0 ||
            value.Length > maximumLength ||
            value.Any(char.IsControl) ||
            char.IsWhiteSpace(value[0]) ||
            char.IsWhiteSpace(value[value.Length - 1])) {
            throw new ArgumentException(
                "Opaque durable coordinates must already be canonical bounded text without edge whitespace or control characters.",
                parameterName);
        }
        return value;
    }
}
