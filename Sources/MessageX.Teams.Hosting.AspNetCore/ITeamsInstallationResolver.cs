namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Maps Microsoft-authenticated Teams coordinates to trusted host installation identity.</summary>
public interface ITeamsInstallationResolver {
    /// <summary>Returns one non-secret MessageX installation identifier or fails closed.</summary>
    string ResolveInstallationId(TeamsInstallationContext context);
}

