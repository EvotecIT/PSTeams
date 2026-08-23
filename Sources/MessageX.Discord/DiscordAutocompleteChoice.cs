namespace MessageX.Discord;

/// <summary>One typed Discord autocomplete choice.</summary>
public sealed class DiscordAutocompleteChoice {
    private const long MaximumSafeInteger = 9007199254740992L;

    private DiscordAutocompleteChoice(string name, object value) {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100 || name.Any(char.IsControl)) {
            throw new ArgumentException("Choice names must be bounded text without control characters.", nameof(name));
        }
        Name = name.Trim();
        Value = value;
    }

    /// <summary>Choice label displayed to the user.</summary>
    public string Name { get; }

    internal object Value { get; }

    /// <summary>Creates a string-valued autocomplete choice.</summary>
    public static DiscordAutocompleteChoice FromString(string name, string value) {
        if (value is null || value.Length > 100 || value.Any(char.IsControl)) {
            throw new ArgumentException("Choice values must be bounded text without control characters.", nameof(value));
        }
        return new DiscordAutocompleteChoice(name, value);
    }

    /// <summary>Creates an integer-valued autocomplete choice.</summary>
    public static DiscordAutocompleteChoice FromInteger(string name, long value) {
        if (value is < -MaximumSafeInteger or > MaximumSafeInteger) {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        return new DiscordAutocompleteChoice(name, value);
    }

    /// <summary>Creates a numeric autocomplete choice.</summary>
    public static DiscordAutocompleteChoice FromNumber(string name, double value) {
        if (double.IsNaN(value) || double.IsInfinity(value) ||
            value < -MaximumSafeInteger || value > MaximumSafeInteger) {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        return new DiscordAutocompleteChoice(name, value);
    }
}
