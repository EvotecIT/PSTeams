using System.Globalization;

namespace MessageX.Discord;

internal static class DiscordSnowflake {
    private const long DiscordEpochMilliseconds = 1420070400000;

    public static string Normalize(string? value, string parameterName) {
        if (value is null || value.Length > 20 || value.Any(char.IsControl)) {
            throw new ArgumentException("A valid Discord snowflake identifier is required.", parameterName);
        }
        var normalized = value.Trim();
        if (string.IsNullOrEmpty(normalized) ||
            normalized[0] == '0' ||
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

    public static bool TryGetTimestamp(string? value, out DateTimeOffset timestamp) {
        timestamp = default;
        if (!TryNormalize(value, out var normalized) ||
            !ulong.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var snowflake)) {
            return false;
        }
        var unixMilliseconds = (long)(snowflake >> 22) + DiscordEpochMilliseconds;
        try {
            timestamp = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds);
            return true;
        } catch (ArgumentOutOfRangeException) {
            return false;
        }
    }
}
