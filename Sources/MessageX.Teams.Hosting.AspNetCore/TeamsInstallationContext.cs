namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Safe verified Teams coordinates supplied to the host's installation resolver.</summary>
public sealed class TeamsInstallationContext {
    internal TeamsInstallationContext(string? tenantId, string? teamId, string conversationId) {
        TenantId = tenantId;
        TeamId = teamId;
        ConversationId = conversationId;
    }

    /// <summary>Microsoft Entra tenant identifier, when present.</summary>
    public string? TenantId { get; }

    /// <summary>Teams team identifier for channel conversations, when present.</summary>
    public string? TeamId { get; }

    /// <summary>Verified provider conversation identifier.</summary>
    public string ConversationId { get; }
}

