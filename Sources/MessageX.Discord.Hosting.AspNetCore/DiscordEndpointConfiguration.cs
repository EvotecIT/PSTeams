namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Trusted route configuration for one Discord application installation.</summary>
public sealed class DiscordEndpointConfiguration {
    private readonly string _publicKeyHex;

    /// <summary>Creates configuration for one explicitly mapped installation.</summary>
    public DiscordEndpointConfiguration(
        string installationId,
        string publicKeyHex,
        string applicationId,
        string installationOwnerId,
        TimeSpan? replayWindow = null) {
        InstallationId = NormalizeInstallation(installationId);
        var normalizedApplicationId = NormalizeSnowflake(applicationId, nameof(applicationId));
        var normalizedOwnerId = NormalizeSnowflake(
            installationOwnerId,
            nameof(installationOwnerId),
            allowZero: true);
        ApplicationId = normalizedApplicationId;
        InstallationOwnerId = normalizedOwnerId;
        if (string.IsNullOrWhiteSpace(publicKeyHex) ||
            publicKeyHex.Length != 64 ||
            publicKeyHex.Any(value => !Uri.IsHexDigit(value))) {
            throw new ArgumentException("A 32-byte hexadecimal Discord public key is required.", nameof(publicKeyHex));
        }
        _publicKeyHex = publicKeyHex;
        ReplayWindow = replayWindow ?? TimeSpan.FromMinutes(5);
        if (ReplayWindow <= TimeSpan.Zero || ReplayWindow > TimeSpan.FromHours(1)) {
            throw new ArgumentOutOfRangeException(nameof(replayWindow));
        }
    }

    /// <summary>Non-secret installation identifier selected by the endpoint route.</summary>
    public string InstallationId { get; }

    /// <summary>Expected Discord application identifier.</summary>
    public string ApplicationId { get; }

    /// <summary>Expected guild or user installation owner identifier.</summary>
    public string InstallationOwnerId { get; }

    /// <summary>Maximum accepted difference between the signed timestamp and host receive time.</summary>
    public TimeSpan ReplayWindow { get; }

    internal string PublicKeyHex => _publicKeyHex;

    /// <inheritdoc />
    public override string ToString() => InstallationId;

    private static string NormalizeInstallation(string? value) {
        if (value is null || value.Length > 256 || value.Any(char.IsControl)) {
            throw new ArgumentException("A bounded installation identifier is required.", nameof(value));
        }
        var normalized = value.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("A bounded installation identifier is required.", nameof(value))
            : normalized;
    }

    private static string NormalizeSnowflake(
        string? value,
        string parameterName,
        bool allowZero = false) {
        if (allowZero && string.Equals(value, "0", StringComparison.Ordinal)) {
            return "0";
        }
        if (value is null || value.Length is < 17 or > 20 || value.Any(character => character is < '0' or > '9')) {
            throw new ArgumentException("A Discord snowflake identifier is required.", parameterName);
        }
        return value;
    }
}
