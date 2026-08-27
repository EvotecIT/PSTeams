using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a provider-native Discord message.</summary>
[Cmdlet(VerbsCommon.New, "DiscordMessage")]
[OutputType(typeof(DiscordMessageRequest))]
public sealed class CmdletNewDiscordMessage : PSCmdlet {
    /// <summary>Plain or Discord-markdown message content.</summary>
    [Parameter(Mandatory = false, Position = 0)]
    public string? Content { get; set; }

    /// <summary>Rich Discord embeds.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbed[] Embeds { get; set; } = Array.Empty<DiscordEmbed>();

    /// <summary>Files uploaded with the message.</summary>
    [Parameter(Mandatory = false)]
    public DiscordAttachment[] Attachments { get; set; } = Array.Empty<DiscordAttachment>();

    /// <summary>Interactive Discord action rows.</summary>
    [Parameter(Mandatory = false)]
    public DiscordActionRow[] Components { get; set; } = Array.Empty<DiscordActionRow>();

    /// <summary>Explicit mention policy. Defaults to notifying nobody.</summary>
    [Parameter(Mandatory = false)]
    public DiscordAllowedMentions? AllowedMentions { get; set; }

    /// <summary>Existing message identifier when creating a reply.</summary>
    [Parameter(Mandatory = false)]
    public string? ReplyToMessageId { get; set; }

    /// <summary>Allows a reply to proceed if the referenced message no longer exists.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter AllowMissingReply { get; set; }

    /// <summary>Optional nonce used for correlation or deduplication.</summary>
    [Parameter(Mandatory = false)]
    public string? Nonce { get; set; }

    /// <summary>Asks Discord to enforce nonce uniqueness for recent messages.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter EnforceNonce { get; set; }

    /// <summary>Optional incoming-webhook username override.</summary>
    [Parameter(Mandatory = false)]
    public string? WebhookUsername { get; set; }

    /// <summary>Optional incoming-webhook avatar override.</summary>
    [Parameter(Mandatory = false)]
    public Uri? WebhookAvatarUrl { get; set; }

    /// <summary>Requests text-to-speech output.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter TextToSpeech { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var message = new DiscordMessageRequest {
            Content = Content,
            AllowedMentions = AllowedMentions ?? DiscordAllowedMentions.None,
            ReplyToMessageId = ReplyToMessageId,
            FailIfReplyMissing = !AllowMissingReply,
            Nonce = Nonce,
            EnforceNonce = EnforceNonce.IsPresent,
            WebhookUsername = WebhookUsername,
            WebhookAvatarUrl = WebhookAvatarUrl,
            TextToSpeech = TextToSpeech.IsPresent
        };
        foreach (var embed in Embeds ?? Array.Empty<DiscordEmbed>()) {
            if (embed is not null) {
                message.Embeds.Add(embed);
            }
        }
        foreach (var attachment in Attachments ?? Array.Empty<DiscordAttachment>()) {
            if (attachment is not null) {
                message.Attachments.Add(attachment);
            }
        }
        foreach (var component in Components ?? Array.Empty<DiscordActionRow>()) {
            if (component is not null) {
                message.Components.Add(component);
            }
        }
        WriteObject(message);
    }
}
