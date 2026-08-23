namespace MessageX.Slack;

/// <summary>Verified Slack application and workspace coordinates used to select a MessageX installation.</summary>
public sealed class SlackInstallationContext {
    internal SlackInstallationContext(
        string applicationId,
        string? workspaceId,
        string? enterpriseId) {
        ApplicationId = applicationId;
        WorkspaceId = workspaceId;
        EnterpriseId = enterpriseId;
    }

    /// <summary>Slack application identifier from the verified request.</summary>
    public string ApplicationId { get; }

    /// <summary>Workspace identifier from the verified request, when present.</summary>
    public string? WorkspaceId { get; }

    /// <summary>Enterprise Grid identifier from the verified request, when present.</summary>
    public string? EnterpriseId { get; }
}
