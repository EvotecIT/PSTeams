namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Trusted endpoint configuration for one Discord application shared by multiple installations.</summary>
public sealed class DiscordApplicationEndpointConfiguration {
    private readonly string _publicKeyHex;

    /// <summary>Creates configuration for a shared Discord application endpoint.</summary>
    public DiscordApplicationEndpointConfiguration(
        string publicKeyHex,
        string applicationId,
        TimeSpan? replayWindow = null) {
        ApplicationId = DiscordEndpointConfiguration.NormalizeSnowflake(
            applicationId,
            nameof(applicationId));
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

    /// <summary>Expected Discord application identifier.</summary>
    public string ApplicationId { get; }

    /// <summary>Maximum accepted difference between the signed timestamp and host receive time.</summary>
    public TimeSpan ReplayWindow { get; }

    internal string PublicKeyHex => _publicKeyHex;

    /// <inheritdoc />
    public override string ToString() => ApplicationId;
}
