namespace MessageX.Discord;

/// <summary>An authenticated Discord bot REST connection.</summary>
public sealed class DiscordConnection : IProviderCapabilities {
    private readonly string _botToken;

    private DiscordConnection(string botToken, string? applicationId) {
        _botToken = botToken;
        ApplicationId = string.IsNullOrWhiteSpace(applicationId)
            ? null
            : DiscordSnowflake.Normalize(applicationId, nameof(applicationId));
    }

    /// <summary>Discord REST API v10 base URI.</summary>
    public static Uri DefaultApiBaseUri { get; } = new("https://discord.com/api/v10/");

    /// <summary>Optional non-secret Discord application identifier.</summary>
    public string? ApplicationId { get; }

    /// <inheritdoc />
    public MessageCapabilities Capabilities =>
        MessageCapabilities.Send |
        MessageCapabilities.Reply |
        MessageCapabilities.UploadFile;

    internal string BotToken => _botToken;

    /// <summary>Creates a connection from a Discord bot token.</summary>
    public static DiscordConnection ForBotToken(string botToken, string? applicationId = null) {
        var normalized = botToken?.Trim();
        if (string.IsNullOrEmpty(normalized) || normalized!.Length < 20 ||
            normalized.Any(character => char.IsWhiteSpace(character) || char.IsControl(character))) {
            throw new ArgumentException("A Discord bot token is required.", nameof(botToken));
        }
        return new DiscordConnection(normalized, applicationId);
    }

    /// <inheritdoc />
    public override string ToString() {
        return ApplicationId is null
            ? "Discord bot connection"
            : $"Discord bot connection for application {ApplicationId}";
    }
}
