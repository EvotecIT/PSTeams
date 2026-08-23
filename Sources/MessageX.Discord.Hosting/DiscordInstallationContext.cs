namespace MessageX.Discord;

/// <summary>Verified Discord application and authorization coordinates used to select a MessageX installation.</summary>
public sealed class DiscordInstallationContext {
    internal DiscordInstallationContext(
        string applicationId,
        int integrationType,
        string installationOwnerId) {
        ApplicationId = applicationId;
        IntegrationType = integrationType;
        InstallationOwnerId = installationOwnerId;
    }

    /// <summary>Discord application identifier from the verified interaction.</summary>
    public string ApplicationId { get; }

    /// <summary>Discord installation integration type: zero for guild install or one for user install.</summary>
    public int IntegrationType { get; }

    /// <summary>Guild or user owner identifier from the verified authorization map.</summary>
    public string InstallationOwnerId { get; }
}
