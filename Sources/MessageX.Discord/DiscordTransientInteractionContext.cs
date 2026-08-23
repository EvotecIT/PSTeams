using System.Text.Json.Serialization;

namespace MessageX.Discord;

/// <summary>Short-lived Discord follow-up capability that must never enter durable reference storage.</summary>
public sealed class DiscordTransientInteractionContext {
    internal static DiscordTransientInteractionContext Unavailable { get; } =
        new(string.Empty, string.Empty, DateTimeOffset.MinValue);

    internal DiscordTransientInteractionContext(
        string applicationId,
        string token,
        DateTimeOffset expiresAt) {
        ApplicationId = applicationId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    /// <summary>Discord application identifier.</summary>
    public string ApplicationId { get; }

    /// <summary>Interaction-token expiration used to prevent late follow-up attempts.</summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>Short-lived interaction token. Never persist, serialize, or log this value.</summary>
    [JsonIgnore]
    public string Token { get; }
}
