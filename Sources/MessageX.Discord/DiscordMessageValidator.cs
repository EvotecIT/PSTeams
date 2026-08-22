namespace MessageX.Discord;

internal static class DiscordMessageValidator {
    public const int MaximumRequestBytes = 25 * 1024 * 1024;
    public const int MaximumAttachmentBytes = 10 * 1024 * 1024;

    public static void Validate(DiscordMessageRequest message, DiscordMessageTarget target) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        ValidateTarget(target);
        if (string.IsNullOrWhiteSpace(message.Content) && message.Embeds.Count == 0 && message.Attachments.Count == 0) {
            throw new ArgumentException("A Discord message requires content, an embed, or an attachment.", nameof(message));
        }
        if (message.Content?.Length > 2000) {
            throw new ArgumentException("Discord message content cannot exceed 2000 characters.", nameof(message));
        }
        if (message.Embeds.Count > 10) {
            throw new ArgumentException("Discord messages cannot contain more than 10 embeds.", nameof(message));
        }
        if (message.Attachments.Count > 10) {
            throw new ArgumentException("Discord messages cannot contain more than 10 attachments.", nameof(message));
        }
        var attachmentFileNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attachment in message.Attachments) {
            if (attachment is null) {
                throw new ArgumentException("Discord attachment collections cannot contain null values.", nameof(message));
            }
            if (!attachmentFileNames.Add(attachment.FileName)) {
                throw new ArgumentException("Discord attachment file names must be unique after spoiler normalization.", nameof(message));
            }
            if (attachment.Length > MaximumAttachmentBytes) {
                throw new ArgumentException(
                    $"Discord attachments cannot exceed the default {MaximumAttachmentBytes} byte per-file limit.",
                    nameof(message));
            }
        }
        if (message.Nonce?.Length > 25) {
            throw new ArgumentException("Discord nonces cannot exceed 25 characters.", nameof(message));
        }
        if (message.EnforceNonce && string.IsNullOrWhiteSpace(message.Nonce)) {
            throw new ArgumentException("Discord nonce enforcement requires a nonce.", nameof(message));
        }
        if (target.DeliveryMethod == DiscordDeliveryMethod.IncomingWebhook &&
            (!string.IsNullOrWhiteSpace(message.Nonce) || message.EnforceNonce)) {
            throw new ArgumentException("Discord nonce options require an authenticated bot target.", nameof(message));
        }
        if (!string.IsNullOrWhiteSpace(message.ReplyToMessageId)) {
            if (target.DeliveryMethod == DiscordDeliveryMethod.IncomingWebhook) {
                throw new ArgumentException("Discord incoming webhooks do not accept message reply references.", nameof(message));
            }
            DiscordSnowflake.Normalize(message.ReplyToMessageId, nameof(message.ReplyToMessageId));
        }
        if (message.WebhookUsername?.Length > 80) {
            throw new ArgumentException("Discord webhook usernames cannot exceed 80 characters.", nameof(message));
        }
        if (target.DeliveryMethod != DiscordDeliveryMethod.IncomingWebhook &&
            (!string.IsNullOrWhiteSpace(message.WebhookUsername) || message.WebhookAvatarUrl is not null)) {
            throw new ArgumentException("Discord webhook identity overrides require an incoming-webhook target.", nameof(message));
        }
        if (message.WebhookAvatarUrl is not null) {
            ValidateHttpsUri(message.WebhookAvatarUrl, "Discord webhook avatar URLs");
        }

        ValidateAllowedMentions(message.AllowedMentions);
        var totalEmbedCharacters = 0;
        foreach (var embed in message.Embeds) {
            totalEmbedCharacters += ValidateEmbed(embed);
        }
        ValidateAttachmentReferences(message);
        if (totalEmbedCharacters > 6000) {
            throw new ArgumentException("Discord embed text cannot exceed 6000 characters per message.", nameof(message));
        }
    }

    private static void ValidateAttachmentReferences(DiscordMessageRequest message) {
        foreach (var embed in message.Embeds) {
            foreach (var uri in new[] {
                embed.Author?.IconUrl,
                embed.Footer?.IconUrl,
                embed.Image?.Url,
                embed.Thumbnail?.Url
            }) {
                if (uri is null || !string.Equals(uri.Scheme, "attachment", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                if (!DiscordAttachmentReferenceResolver.TryResolve(uri, message.Attachments, out var attachment)) {
                    throw new ArgumentException("Discord attachment URLs must reference a file in the same message.", nameof(message));
                }
                if (!DiscordAttachmentReferenceResolver.IsSafeEmbedFileName(attachment!.FileName)) {
                    throw new ArgumentException(
                        "Discord attachment URLs require ASCII alphanumeric file names using only underscores, hyphens, and periods.",
                        nameof(message));
                }
                if (!DiscordAttachmentReferenceResolver.IsSupportedEmbedFileName(attachment.FileName)) {
                    throw new ArgumentException(
                        "Discord embed attachments require a JPG, JPEG, PNG, WebP, or GIF file name.",
                        nameof(message));
                }
            }
        }
    }

    private static void ValidateTarget(DiscordMessageTarget target) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        switch (target.DeliveryMethod) {
            case DiscordDeliveryMethod.IncomingWebhook:
                DiscordMessageTarget.ValidateWebhookUri(target.WebhookUri);
                break;
            case DiscordDeliveryMethod.BotChannel:
                DiscordSnowflake.Normalize(target.ChannelId, nameof(target.ChannelId));
                break;
            case DiscordDeliveryMethod.BotThread:
                DiscordSnowflake.Normalize(target.ChannelId, nameof(target.ChannelId));
                DiscordSnowflake.Normalize(target.ThreadId, nameof(target.ThreadId));
                if (!string.Equals(target.ChannelId, target.ThreadId, StringComparison.Ordinal)) {
                    throw new ArgumentException("Discord bot thread coordinates are inconsistent.", nameof(target));
                }
                break;
            case DiscordDeliveryMethod.BotDirectMessage:
                if (target.UserId is null) {
                    DiscordSnowflake.Normalize(target.ChannelId, nameof(target.ChannelId));
                }
                else {
                    DiscordSnowflake.Normalize(target.UserId, nameof(target.UserId));
                }
                break;
            default:
                throw new ArgumentException("Unsupported Discord delivery method.", nameof(target));
        }
    }

    private static void ValidateAllowedMentions(DiscordAllowedMentions mentions) {
        if (mentions is null) {
            throw new ArgumentException("A Discord allowed-mentions policy is required.", nameof(mentions));
        }
        if (mentions.UserIds.Count > 100 || mentions.RoleIds.Count > 100) {
            throw new ArgumentException("Discord allows at most 100 explicit users and 100 explicit roles in mention policy.", nameof(mentions));
        }
        if (mentions.ParseUsers && mentions.UserIds.Count > 0) {
            throw new ArgumentException("Discord user mention parsing cannot be combined with explicit user identifiers.", nameof(mentions));
        }
        if (mentions.ParseRoles && mentions.RoleIds.Count > 0) {
            throw new ArgumentException("Discord role mention parsing cannot be combined with explicit role identifiers.", nameof(mentions));
        }
        foreach (var userId in mentions.UserIds) {
            DiscordSnowflake.Normalize(userId, nameof(mentions.UserIds));
        }
        foreach (var roleId in mentions.RoleIds) {
            DiscordSnowflake.Normalize(roleId, nameof(mentions.RoleIds));
        }
    }

    private static int ValidateEmbed(DiscordEmbed embed) {
        if (embed is null) {
            throw new ArgumentException("Discord embed collections cannot contain null values.", nameof(embed));
        }
        if (!HasRenderableEmbedProperty(embed)) {
            throw new ArgumentException("Discord embeds require at least one renderable property.", nameof(embed));
        }
        if (embed.Title is not null && string.IsNullOrWhiteSpace(embed.Title)) {
            throw new ArgumentException("Discord embed titles cannot be empty or whitespace.", nameof(embed));
        }
        if (embed.Description is not null && string.IsNullOrWhiteSpace(embed.Description)) {
            throw new ArgumentException("Discord embed descriptions cannot be empty or whitespace.", nameof(embed));
        }
        if (embed.Title?.Length > 256) {
            throw new ArgumentException("Discord embed titles cannot exceed 256 characters.", nameof(embed));
        }
        if (embed.Description?.Length > 4096) {
            throw new ArgumentException("Discord embed descriptions cannot exceed 4096 characters.", nameof(embed));
        }
        if (embed.Color is < 0 or > 0xFFFFFF) {
            throw new ArgumentException("Discord embed colors must be a 24-bit RGB value.", nameof(embed));
        }
        if (embed.Url is not null) {
            ValidateHttpsUri(embed.Url, "Discord embed URLs");
        }
        if (embed.Author is not null) {
            if (string.IsNullOrWhiteSpace(embed.Author.Name) || embed.Author.Name.Length > 256) {
                throw new ArgumentException("Discord embed author names must contain 1 to 256 characters.", nameof(embed));
            }
            if (embed.Author.Url is not null) {
                ValidateHttpsUri(embed.Author.Url, "Discord embed author URLs");
            }
            if (embed.Author.IconUrl is not null) {
                ValidateMediaUri(embed.Author.IconUrl, "Discord embed author icon URLs");
            }
        }
        if (embed.Footer is not null) {
            if (string.IsNullOrWhiteSpace(embed.Footer.Text) || embed.Footer.Text.Length > 2048) {
                throw new ArgumentException("Discord embed footer text must contain 1 to 2048 characters.", nameof(embed));
            }
            if (embed.Footer.IconUrl is not null) {
                ValidateMediaUri(embed.Footer.IconUrl, "Discord embed footer icon URLs");
            }
        }
        if (embed.Image is not null) {
            ValidateMediaUri(embed.Image.Url, "Discord embed image URLs");
        }
        if (embed.Thumbnail is not null) {
            ValidateMediaUri(embed.Thumbnail.Url, "Discord embed thumbnail URLs");
        }
        if (embed.Fields.Count > 25) {
            throw new ArgumentException("Discord embeds cannot contain more than 25 fields.", nameof(embed));
        }

        var characters = embed.Title?.Length ?? 0;
        characters += embed.Description?.Length ?? 0;
        characters += embed.Author?.Name.Length ?? 0;
        characters += embed.Footer?.Text.Length ?? 0;
        foreach (var field in embed.Fields) {
            if (field is null || string.IsNullOrWhiteSpace(field.Name) || field.Name.Length > 256 ||
                string.IsNullOrWhiteSpace(field.Value) || field.Value.Length > 1024) {
                throw new ArgumentException(
                    "Discord embed fields require a 1-256 character name and 1-1024 character value.",
                    nameof(embed));
            }
            characters += field.Name.Length + field.Value.Length;
        }
        return characters;
    }

    private static bool HasRenderableEmbedProperty(DiscordEmbed embed) =>
        !string.IsNullOrWhiteSpace(embed.Title) ||
        !string.IsNullOrWhiteSpace(embed.Description) ||
        embed.Url is not null ||
        embed.Color is not null ||
        embed.Timestamp is not null ||
        embed.Author is not null ||
        embed.Footer is not null ||
        embed.Image is not null ||
        embed.Thumbnail is not null ||
        embed.Fields.Count > 0;

    private static void ValidateHttpsUri(Uri uri, string label) {
        if (!uri.IsAbsoluteUri || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException($"{label} must use absolute HTTPS URIs.", nameof(uri));
        }
    }

    private static void ValidateMediaUri(Uri? uri, string label) {
        if (uri is null || !uri.IsAbsoluteUri ||
            (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(uri.Scheme, "attachment", StringComparison.OrdinalIgnoreCase))) {
            throw new ArgumentException($"{label} must use HTTPS or attachment URIs.", nameof(uri));
        }
    }
}
