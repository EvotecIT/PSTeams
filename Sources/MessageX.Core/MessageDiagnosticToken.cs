namespace MessageX.Core;

internal static class MessageDiagnosticToken {
    private const int MaximumLength = 128;

    public static string? Normalize(string? value) {
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate!.Length > MaximumLength) {
            return null;
        }

        foreach (var character in candidate) {
            var isAsciiLetterOrDigit = character is >= 'a' and <= 'z' or
                >= 'A' and <= 'Z' or
                >= '0' and <= '9';
            if (!isAsciiLetterOrDigit && character is not '-' and not '_' and not '.') {
                return null;
            }
        }

        return candidate;
    }
}
