using System.Drawing;
namespace TeamsX;

/// <summary>
/// Normalizes named and hexadecimal color values into Teams theme-color format.
/// </summary>
public static class TeamsColorUtility {
    public static string? NormalizeToHex(string? color) {
        if (string.IsNullOrWhiteSpace(color)) {
            return null;
        }

        var candidate = color.Trim();
        if (candidate.StartsWith("#", StringComparison.Ordinal)) {
            return IsHexColor(candidate)
                ? candidate.ToUpperInvariant()
                : throw new ArgumentException("The Input value is not a valid colorname nor an valid color hex code.", nameof(color));
        }

        var resolved = Color.FromName(candidate);
        if (resolved.ToArgb() == 0 &&
            !string.Equals(candidate, "Transparent", StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException("The Input value is not a valid colorname nor an valid color hex code.", nameof(color));
        }
        return $"#{resolved.R:X2}{resolved.G:X2}{resolved.B:X2}";
    }

    private static bool IsHexColor(string value) {
        if (value.Length != 7 || value[0] != '#') {
            return false;
        }

        for (var index = 1; index < value.Length; index++) {
            if (!Uri.IsHexDigit(value[index])) {
                return false;
            }
        }

        return true;
    }
}
