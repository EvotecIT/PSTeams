using System.Text.Json;

namespace MessageX.Discord;

internal static class DiscordMessageRenderer {
    public static string Render(DiscordMessageRequest message, DiscordMessageTarget target) {
        DiscordMessageValidator.Validate(message, target);
        return JsonSerializer.Serialize(CreatePayload(message, target));
    }

    public static Dictionary<string, object?> CreatePayload(
        DiscordMessageRequest message,
        DiscordMessageTarget target) {
        DiscordMessageValidator.Validate(message, target);
        var payload = new Dictionary<string, object?>();
        AddOptional(payload, "content", string.IsNullOrWhiteSpace(message.Content) ? null : message.Content);
        if (message.TextToSpeech) {
            payload["tts"] = true;
        }
        if (target.DeliveryMethod != DiscordDeliveryMethod.IncomingWebhook) {
            AddOptional(payload, "nonce", string.IsNullOrWhiteSpace(message.Nonce) ? null : message.Nonce);
            if (message.EnforceNonce) {
                payload["enforce_nonce"] = true;
            }
        }
        payload["allowed_mentions"] = RenderAllowedMentions(message.AllowedMentions);
        if (message.Embeds.Count > 0) {
            payload["embeds"] = message.Embeds.Select(RenderEmbed).ToArray();
        }
        if (!string.IsNullOrWhiteSpace(message.ReplyToMessageId)) {
            payload["message_reference"] = new Dictionary<string, object?> {
                ["message_id"] = message.ReplyToMessageId,
                ["fail_if_not_exists"] = message.FailIfReplyMissing
            };
        }
        if (message.Attachments.Count > 0) {
            payload["attachments"] = message.Attachments.Select((attachment, index) => {
                var item = new Dictionary<string, object?> {
                    ["id"] = index,
                    ["filename"] = attachment.FileName
                };
                AddOptional(item, "description", attachment.Description);
                if (attachment.IsSpoiler) {
                    item["is_spoiler"] = true;
                }
                return item;
            }).ToArray();
        }
        if (target.DeliveryMethod == DiscordDeliveryMethod.IncomingWebhook) {
            AddOptional(payload, "username", string.IsNullOrWhiteSpace(message.WebhookUsername) ? null : message.WebhookUsername);
            AddOptional(payload, "avatar_url", message.WebhookAvatarUrl?.AbsoluteUri);
        }
        return payload;
    }

    private static Dictionary<string, object?> RenderAllowedMentions(DiscordAllowedMentions mentions) {
        var parse = new List<string>();
        if (mentions.ParseUsers) {
            parse.Add("users");
        }
        if (mentions.ParseRoles) {
            parse.Add("roles");
        }
        if (mentions.ParseEveryone) {
            parse.Add("everyone");
        }
        var payload = new Dictionary<string, object?> {
            ["parse"] = parse.ToArray(),
            ["replied_user"] = mentions.RepliedUser
        };
        if (mentions.UserIds.Count > 0) {
            payload["users"] = mentions.UserIds.Select(id => DiscordSnowflake.Normalize(id, nameof(mentions.UserIds))).ToArray();
        }
        if (mentions.RoleIds.Count > 0) {
            payload["roles"] = mentions.RoleIds.Select(id => DiscordSnowflake.Normalize(id, nameof(mentions.RoleIds))).ToArray();
        }
        return payload;
    }

    private static Dictionary<string, object?> RenderEmbed(DiscordEmbed embed) {
        var payload = new Dictionary<string, object?>();
        AddOptional(payload, "title", embed.Title);
        AddOptional(payload, "description", embed.Description);
        AddOptional(payload, "url", embed.Url?.AbsoluteUri);
        AddOptional(payload, "color", embed.Color);
        AddOptional(payload, "timestamp", embed.Timestamp?.ToUniversalTime().ToString("O"));
        if (embed.Author is not null) {
            var author = new Dictionary<string, object?> { ["name"] = embed.Author.Name };
            AddOptional(author, "url", embed.Author.Url?.AbsoluteUri);
            AddOptional(author, "icon_url", RenderMediaUri(embed.Author.IconUrl));
            payload["author"] = author;
        }
        if (embed.Footer is not null) {
            var footer = new Dictionary<string, object?> { ["text"] = embed.Footer.Text };
            AddOptional(footer, "icon_url", RenderMediaUri(embed.Footer.IconUrl));
            payload["footer"] = footer;
        }
        if (embed.Image is not null) {
            payload["image"] = new Dictionary<string, object?> { ["url"] = RenderMediaUri(embed.Image.Url) };
        }
        if (embed.Thumbnail is not null) {
            payload["thumbnail"] = new Dictionary<string, object?> { ["url"] = RenderMediaUri(embed.Thumbnail.Url) };
        }
        if (embed.Fields.Count > 0) {
            payload["fields"] = embed.Fields.Select(field => new Dictionary<string, object?> {
                ["name"] = field.Name,
                ["value"] = field.Value,
                ["inline"] = field.Inline
            }).ToArray();
        }
        return payload;
    }

    private static string? RenderMediaUri(Uri? uri) {
        if (uri is null) {
            return null;
        }
        return string.Equals(uri.Scheme, "attachment", StringComparison.OrdinalIgnoreCase)
            ? uri.OriginalString
            : uri.AbsoluteUri;
    }

    private static void AddOptional(Dictionary<string, object?> payload, string name, object? value) {
        if (value is not null) {
            payload[name] = value;
        }
    }
}
