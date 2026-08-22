namespace MessageX.Slack;

/// <summary>Trusted Slack application and workspace coordinates bound to one HTTP route.</summary>
public sealed class SlackInstallationIdentity {
    /// <summary>Creates one exact route identity.</summary>
    public SlackInstallationIdentity(string applicationId, string workspaceId, string? enterpriseId = null) {
        ApplicationId = Normalize(applicationId, nameof(applicationId));
        WorkspaceId = Normalize(workspaceId, nameof(workspaceId));
        EnterpriseId = enterpriseId is null ? null : Normalize(enterpriseId, nameof(enterpriseId));
    }

    /// <summary>Expected Slack application identifier.</summary>
    public string ApplicationId { get; }

    /// <summary>Expected Slack team/workspace identifier.</summary>
    public string WorkspaceId { get; }

    /// <summary>Expected enterprise identifier for an Enterprise Grid installation, when applicable.</summary>
    public string? EnterpriseId { get; }

    internal bool Matches(string? applicationId, string? workspaceId, string? enterpriseId) =>
        string.Equals(ApplicationId, applicationId, StringComparison.Ordinal) &&
        string.Equals(WorkspaceId, workspaceId, StringComparison.Ordinal) &&
        (EnterpriseId is null || string.Equals(EnterpriseId, enterpriseId, StringComparison.Ordinal));

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

