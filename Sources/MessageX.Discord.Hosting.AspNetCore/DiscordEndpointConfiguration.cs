namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Trusted route configuration for one Discord application installation.</summary>
public sealed class DiscordEndpointConfiguration {
    private readonly string _publicKeyHex;

    /// <summary>Creates configuration for one explicitly mapped installation.</summary>
    public DiscordEndpointConfiguration(
        string installationId,
        string publicKeyHex,
        TimeSpan? replayWindow = null) {
        InstallationId = NormalizeInstallation(installationId);
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
}
