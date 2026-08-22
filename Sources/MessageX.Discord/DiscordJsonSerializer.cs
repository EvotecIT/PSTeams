namespace MessageX.Discord;

/// <summary>Serializes typed Discord messages without including credentials or file bytes.</summary>
public static class DiscordJsonSerializer {
    /// <summary>Serializes a Discord JSON or multipart <c>payload_json</c> body.</summary>
    public static string Serialize(DiscordMessageRequest message, DiscordMessageTarget target) {
        return DiscordMessageRenderer.Render(message, target);
    }
}
