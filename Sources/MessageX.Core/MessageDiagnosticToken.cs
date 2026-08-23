namespace MessageX.Core;

/// <summary>Normalizes non-secret provider and transport diagnostic identifiers.</summary>
public static class MessageDiagnosticToken {
    private const int MaximumLength = 128;

    /// <summary>Returns a bounded safe diagnostic identifier, or null when the value is unsuitable.</summary>
    public static string? Normalize(string? value) {
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate!.Length > MaximumLength) {
            return null;
        }

        foreach (var character in candidate) {
            var isAsciiLetterOrDigit = character is >= 'a' and <= 'z' or
                >= 'A' and <= 'Z' or
                >= '0' and <= '9';
            if (!isAsciiLetterOrDigit && character is not '-' and not '_' and not '.' and not ':') {
                return null;
            }
        }

        return candidate;
    }
}
