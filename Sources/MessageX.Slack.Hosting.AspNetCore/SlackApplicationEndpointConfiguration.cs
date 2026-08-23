namespace MessageX.Slack.Hosting.AspNetCore;

/// <summary>Trusted endpoint configuration for one Slack application shared by multiple installations.</summary>
public sealed class SlackApplicationEndpointConfiguration {
    private readonly string _signingSecret;

    /// <summary>Creates configuration for a shared Slack application endpoint.</summary>
    public SlackApplicationEndpointConfiguration(
        string signingSecret,
        string applicationId,
        TimeSpan? replayWindow = null) {
        if (string.IsNullOrWhiteSpace(signingSecret) ||
            signingSecret.Length > 4096 ||
            signingSecret.Any(char.IsControl)) {
            throw new ArgumentException("A Slack signing secret is required.", nameof(signingSecret));
        }
        _signingSecret = signingSecret;
        ApplicationId = Normalize(applicationId, nameof(applicationId));
        ReplayWindow = replayWindow ?? TimeSpan.FromMinutes(5);
        if (ReplayWindow <= TimeSpan.Zero || ReplayWindow > TimeSpan.FromHours(1)) {
            throw new ArgumentOutOfRangeException(nameof(replayWindow));
        }
    }

    /// <summary>Expected Slack application identifier.</summary>
    public string ApplicationId { get; }

    /// <summary>Maximum accepted difference between the signed timestamp and host receive time.</summary>
    public TimeSpan ReplayWindow { get; }

    internal string SigningSecret => _signingSecret;

    /// <inheritdoc />
    public override string ToString() => ApplicationId;

    private static string Normalize(string? value, string parameterName) {
        if (value is null || value.Length > 256 || value.Any(char.IsControl)) {
            throw new ArgumentException("A bounded Slack coordinate is required.", parameterName);
        }
        var normalized = value.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("A bounded Slack coordinate is required.", parameterName)
            : normalized;
    }
}
