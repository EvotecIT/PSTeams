namespace MessageX.Discord;

/// <summary>A provider-native Discord message request.</summary>
public sealed class DiscordMessageRequest {
    /// <summary>Plain or Discord-markdown message content.</summary>
    public string? Content { get; set; }

    /// <summary>Whether Discord should synthesize text-to-speech for the content.</summary>
    public bool TextToSpeech { get; set; }

    /// <summary>Optional nonce used to correlate or deduplicate message creation.</summary>
    public string? Nonce { get; set; }

    /// <summary>Asks Discord to enforce nonce uniqueness for recent messages.</summary>
    public bool EnforceNonce { get; set; }

    /// <summary>Existing message identifier when creating a reply.</summary>
    public string? ReplyToMessageId { get; set; }

    /// <summary>Whether Discord should reject a reply when the referenced message no longer exists.</summary>
    public bool FailIfReplyMissing { get; set; } = true;

    /// <summary>Safe-by-default mention policy.</summary>
    public DiscordAllowedMentions AllowedMentions { get; set; } = DiscordAllowedMentions.None;

    /// <summary>Optional username override used only by incoming webhooks.</summary>
    public string? WebhookUsername { get; set; }

    /// <summary>Optional avatar override used only by incoming webhooks.</summary>
    public Uri? WebhookAvatarUrl { get; set; }

    /// <summary>Rich embeds.</summary>
    public IList<DiscordEmbed> Embeds { get; } = new List<DiscordEmbed>();

    /// <summary>Files uploaded with the message.</summary>
    public IList<DiscordAttachment> Attachments { get; } = new List<DiscordAttachment>();
}
