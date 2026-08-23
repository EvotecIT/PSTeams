namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Resolves a MessageX installation from verified Discord application authorization coordinates.</summary>
public interface IDiscordInstallationResolver {
    /// <summary>Returns the trusted MessageX installation identifier, or <see langword="null"/> when unknown.</summary>
    string? ResolveInstallationId(DiscordInstallationContext context);
}
