namespace MessageX.Discord;

/// <summary>A Discord webhook, channel, thread-channel, or direct-message target.</summary>
public sealed class DiscordMessageTarget : IProviderCapabilities {
    /// <summary>Delivery transport.</summary>
    public DiscordDeliveryMethod DeliveryMethod { get; private set; }

    /// <summary>Secret webhook URI retained only inside the Discord transport assembly.</summary>
    internal Uri? WebhookUri { get; private set; }

    /// <summary>Existing Discord channel or thread-channel identifier.</summary>
    public string? ChannelId { get; private set; }

    /// <summary>User identifier used to open or reuse a direct-message channel.</summary>
    public string? UserId { get; private set; }

    /// <summary>Thread identifier appended to an incoming-webhook execution.</summary>
    public string? ThreadId { get; private set; }

    /// <summary>Optional guild identifier retained with durable references.</summary>
    public string? GuildId { get; private set; }

    /// <summary>Optional safe display label.</summary>
    public string? DisplayName { get; private set; }

    /// <inheritdoc />
    public MessageCapabilities Capabilities => DeliveryMethod switch {
        DiscordDeliveryMethod.IncomingWebhook =>
            MessageCapabilities.Send | MessageCapabilities.UploadFile |
            MessageCapabilities.Read | MessageCapabilities.Update | MessageCapabilities.Delete,
        DiscordDeliveryMethod.BotChannel or DiscordDeliveryMethod.BotThread or DiscordDeliveryMethod.BotDirectMessage =>
            MessageCapabilities.Send | MessageCapabilities.Reply | MessageCapabilities.UploadFile |
            MessageCapabilities.Update | MessageCapabilities.Delete | MessageCapabilities.React |
            MessageCapabilities.Read,
        _ => MessageCapabilities.None
    };

    /// <summary>Creates a fixed webhook target, optionally within an existing thread.</summary>
    public static DiscordMessageTarget ForIncomingWebhook(
        Uri uri,
        string? threadId = null,
        string? displayName = null) {
        ValidateWebhookUri(uri);
        return new DiscordMessageTarget {
            DeliveryMethod = DiscordDeliveryMethod.IncomingWebhook,
            WebhookUri = uri,
            ThreadId = string.IsNullOrWhiteSpace(threadId)
                ? null
                : DiscordSnowflake.Normalize(threadId, nameof(threadId)),
            DisplayName = NormalizeDisplayName(displayName)
        };
    }

    /// <summary>Creates a bot target for an existing channel or thread channel.</summary>
    public static DiscordMessageTarget ForChannel(
        string channelId,
        string? guildId = null,
        string? displayName = null) {
        return new DiscordMessageTarget {
            DeliveryMethod = DiscordDeliveryMethod.BotChannel,
            ChannelId = DiscordSnowflake.Normalize(channelId, nameof(channelId)),
            GuildId = string.IsNullOrWhiteSpace(guildId)
                ? null
                : DiscordSnowflake.Normalize(guildId, nameof(guildId)),
            DisplayName = NormalizeDisplayName(displayName)
        };
    }

    /// <summary>Creates a bot target for an existing thread channel.</summary>
    public static DiscordMessageTarget ForThread(
        string threadId,
        string? guildId = null,
        string? displayName = null) {
        var normalizedThreadId = DiscordSnowflake.Normalize(threadId, nameof(threadId));
        return new DiscordMessageTarget {
            DeliveryMethod = DiscordDeliveryMethod.BotThread,
            ChannelId = normalizedThreadId,
            ThreadId = normalizedThreadId,
            GuildId = string.IsNullOrWhiteSpace(guildId)
                ? null
                : DiscordSnowflake.Normalize(guildId, nameof(guildId)),
            DisplayName = NormalizeDisplayName(displayName)
        };
    }

    /// <summary>Creates a bot target that opens or reuses a one-to-one direct message.</summary>
    public static DiscordMessageTarget ForDirectMessage(string userId, string? displayName = null) {
        return new DiscordMessageTarget {
            DeliveryMethod = DiscordDeliveryMethod.BotDirectMessage,
            UserId = DiscordSnowflake.Normalize(userId, nameof(userId)),
            DisplayName = NormalizeDisplayName(displayName)
        };
    }

    internal static DiscordMessageTarget ForDirectMessageChannel(string channelId) {
        return new DiscordMessageTarget {
            DeliveryMethod = DiscordDeliveryMethod.BotDirectMessage,
            ChannelId = DiscordSnowflake.Normalize(channelId, nameof(channelId))
        };
    }

    internal static void ValidateWebhookUri(Uri? uri) {
        if (uri is null) {
            throw new ArgumentNullException(nameof(uri));
        }
        if (!uri.IsAbsoluteUri) {
            throw new ArgumentException("Discord webhooks must use an official HTTPS Discord webhook URI.", nameof(uri));
        }

        var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var webhookIndex = segments.Length >= 4 && segments[0] == "api" && segments[1] == "webhooks"
            ? 1
            : segments.Length >= 5 && segments[0] == "api" &&
                segments[1].Length >= 2 && segments[1][0] == 'v' &&
                segments[1].Skip(1).All(char.IsDigit) && segments[2] == "webhooks"
                ? 2
                : -1;
        var hasValidCoordinates = webhookIndex >= 0 &&
            segments.Length == webhookIndex + 3 &&
            DiscordSnowflake.TryNormalize(segments[webhookIndex + 1], out _) &&
            segments[webhookIndex + 2].Length >= 20;
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !uri.IsDefaultPort ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !string.Equals(uri.Host, "discord.com", StringComparison.OrdinalIgnoreCase) ||
            !hasValidCoordinates) {
            throw new ArgumentException("Discord webhooks must use an official HTTPS Discord webhook URI.", nameof(uri));
        }
    }

    internal string SafeLabel() {
        if (!string.IsNullOrWhiteSpace(DisplayName)) {
            return DisplayName!;
        }
        return DeliveryMethod switch {
            DiscordDeliveryMethod.IncomingWebhook => ThreadId is null
                ? "Discord incoming webhook"
                : $"Discord webhook thread {ThreadId}",
            DiscordDeliveryMethod.BotChannel => $"Discord channel {ChannelId}",
            DiscordDeliveryMethod.BotThread => $"Discord thread {ThreadId}",
            DiscordDeliveryMethod.BotDirectMessage => UserId is null
                ? $"Discord direct message channel {ChannelId}"
                : $"Discord direct message to {UserId}",
            _ => "Discord target"
        };
    }

    /// <inheritdoc />
    public override string ToString() => SafeLabel();

    private static string? NormalizeDisplayName(string? displayName) {
        return string.IsNullOrWhiteSpace(displayName) ? null : displayName!.Trim();
    }
}
