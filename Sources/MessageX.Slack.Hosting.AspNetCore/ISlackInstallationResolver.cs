namespace MessageX.Slack.Hosting.AspNetCore;

/// <summary>Resolves a MessageX installation from verified Slack application and workspace coordinates.</summary>
public interface ISlackInstallationResolver {
    /// <summary>Returns the trusted MessageX installation identifier, or <see langword="null"/> when unknown.</summary>
    string? ResolveInstallationId(SlackInstallationContext context);
}
