namespace MessageX.Slack.Hosting.AspNetCore;

/// <summary>Trusted route configuration for one Slack installation.</summary>
public sealed class SlackEndpointConfiguration {
    private readonly string _signingSecret;

    /// <summary>Creates configuration for one explicitly mapped installation.</summary>
    public SlackEndpointConfiguration(
        string installationId,
        string signingSecret,
        string applicationId,
        string workspaceId,
        string? enterpriseId = null,
        TimeSpan? replayWindow = null) {
        InstallationId = NormalizeInstallation(installationId);
        Identity = new SlackInstallationIdentity(applicationId, workspaceId, enterpriseId);
        if (string.IsNullOrWhiteSpace(signingSecret) ||
            signingSecret.Length > 4096 ||
            signingSecret.Any(char.IsControl)) {
            throw new ArgumentException("A Slack signing secret is required.", nameof(signingSecret));
        }
        _signingSecret = signingSecret;
        ReplayWindow = replayWindow ?? TimeSpan.FromMinutes(5);
        if (ReplayWindow <= TimeSpan.Zero || ReplayWindow > TimeSpan.FromHours(1)) {
            throw new ArgumentOutOfRangeException(nameof(replayWindow));
        }
    }

    /// <summary>Non-secret installation identifier selected by the endpoint route.</summary>
    public string InstallationId { get; }

    /// <summary>Exact non-secret provider coordinates expected on this route.</summary>
    public SlackInstallationIdentity Identity { get; }

    /// <summary>Maximum accepted difference between the signed timestamp and host receive time.</summary>
    public TimeSpan ReplayWindow { get; }

    internal string SigningSecret => _signingSecret;

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
