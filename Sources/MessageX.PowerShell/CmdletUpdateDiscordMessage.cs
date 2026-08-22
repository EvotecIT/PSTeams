using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Updates an application-owned Discord message through a bot or owning webhook.</summary>
[Cmdlet(VerbsData.Update, "DiscordMessage", SupportsShouldProcess = true, DefaultParameterSetName = "Bot")]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletUpdateDiscordMessage : DiscordMessageLifecycleCmdletBase {
    /// <summary>Replacement Discord message.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Bot")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Webhook")]
    public DiscordMessageRequest Message { get; set; } = null!;

    /// <summary>Durable Discord message reference.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Bot")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Webhook")]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, "Update Discord message")) {
            return;
        }
        var result = UsesBot
            ? await BotClient.UpdateAsync(Message, Reference, CancelToken).ConfigureAwait(false)
            : await WebhookClient.UpdateAsync(Message, Reference, CancelToken).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Update-DiscordMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
