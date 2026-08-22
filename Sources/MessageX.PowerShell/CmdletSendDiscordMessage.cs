using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Sends simple or typed messages through Discord incoming webhooks or authenticated bot REST.</summary>
[Cmdlet(VerbsCommunications.Send, "DiscordMessage", SupportsShouldProcess = true, DefaultParameterSetName = "Typed")]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletSendDiscordMessage : DiscordMessageCmdletBase {
    /// <summary>Typed Discord message.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Typed")]
    public DiscordMessageRequest Message { get; set; } = null!;

    /// <summary>Typed Discord webhook, channel, thread, or direct-message target.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Typed")]
    public DiscordMessageTarget Target { get; set; } = null!;

    /// <summary>Simple message text.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "WebhookText")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ChannelText")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ThreadText")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "DirectMessageText")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Secret incoming-webhook URI for the simple webhook flow.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "WebhookText")]
    public Uri WebhookUri { get; set; } = null!;

    /// <summary>Discord channel identifier for the simple bot channel flow.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ChannelText")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Discord thread identifier for webhook or bot thread delivery.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "WebhookText")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ThreadText")]
    public string? ThreadId { get; set; }

    /// <summary>Discord user identifier for the simple direct-message flow.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "DirectMessageText")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Optional guild identifier retained in channel and thread references.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "ChannelText")]
    [Parameter(Mandatory = false, ParameterSetName = "ThreadText")]
    public string? GuildId { get; set; }

    /// <summary>Existing message identifier when creating a bot reply.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "ChannelText")]
    [Parameter(Mandatory = false, ParameterSetName = "ThreadText")]
    [Parameter(Mandatory = false, ParameterSetName = "DirectMessageText")]
    public string? ReplyToMessageId { get; set; }

    /// <summary>Allows a reply to proceed if the referenced message no longer exists.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "ChannelText")]
    [Parameter(Mandatory = false, ParameterSetName = "ThreadText")]
    [Parameter(Mandatory = false, ParameterSetName = "DirectMessageText")]
    public SwitchParameter AllowMissingReply { get; set; }

    /// <summary>Explicit mention policy. Defaults to notifying nobody.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "WebhookText")]
    [Parameter(Mandatory = false, ParameterSetName = "ChannelText")]
    [Parameter(Mandatory = false, ParameterSetName = "ThreadText")]
    [Parameter(Mandatory = false, ParameterSetName = "DirectMessageText")]
    public DiscordAllowedMentions? AllowedMentions { get; set; }

    /// <summary>Returns the typed delivery result.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        var (message, target) = ResolveRequest();
        EnsureConnectionAvailable(target);
        if (!ShouldProcess(target.ToString(), $"Send Discord message using {target.DeliveryMethod}")) {
            return;
        }

        var result = await SendWithClientAsync(message, target).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-DiscordMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }

    private (DiscordMessageRequest Message, DiscordMessageTarget Target) ResolveRequest() {
        if (ParameterSetName == "Typed") {
            return (Message, Target);
        }
        var message = new DiscordMessageRequest {
            Content = Text,
            AllowedMentions = AllowedMentions ?? DiscordAllowedMentions.None,
            ReplyToMessageId = ReplyToMessageId,
            FailIfReplyMissing = !AllowMissingReply
        };
        return ParameterSetName switch {
            "WebhookText" => (message, DiscordMessageTarget.ForIncomingWebhook(WebhookUri, ThreadId)),
            "ChannelText" => (message, DiscordMessageTarget.ForChannel(ChannelId, GuildId)),
            "ThreadText" => (message, DiscordMessageTarget.ForThread(ThreadId!, GuildId)),
            "DirectMessageText" => (message, DiscordMessageTarget.ForDirectMessage(UserId)),
            _ => throw new InvalidOperationException($"Unsupported Discord parameter set '{ParameterSetName}'.")
        };
    }

    private void EnsureConnectionAvailable(DiscordMessageTarget target) {
        if (target.DeliveryMethod != DiscordDeliveryMethod.IncomingWebhook && Connection is null) {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("A DiscordConnection is required for authenticated bot targets."),
                "DiscordConnectionRequired",
                ErrorCategory.AuthenticationError,
                target.ToString()));
        }
    }
}
