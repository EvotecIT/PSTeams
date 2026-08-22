using System.Globalization;

namespace MessageX.Discord;

internal static class DiscordSnowflake {
    public static string Normalize(string? value, string parameterName) {
        if (value is null || value.Length > 20 || value.Any(char.IsControl)) {
            throw new ArgumentException("A valid Discord snowflake identifier is required.", parameterName);
        }
        var normalized = value.Trim();
        if (string.IsNullOrEmpty(normalized) ||
            !ulong.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ||
            parsed == 0) {
            throw new ArgumentException("A valid Discord snowflake identifier is required.", parameterName);
        }
        return normalized;
    }

    public static bool TryNormalize(string? value, out string normalized) {
        try {
            normalized = Normalize(value, nameof(value));
            return true;
        } catch (ArgumentException) {
            normalized = string.Empty;
            return false;
        }
    }
}
