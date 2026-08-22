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
}
