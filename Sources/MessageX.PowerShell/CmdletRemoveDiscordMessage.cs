using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Deletes an application-owned Discord message through a bot or owning webhook.</summary>
[Cmdlet(VerbsCommon.Remove, "DiscordMessage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Bot")]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletRemoveDiscordMessage : DiscordMessageLifecycleCmdletBase {
    /// <summary>Durable Discord message reference.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Bot")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Webhook")]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, "Delete Discord message")) {
            return;
        }
        var result = UsesBot
            ? await BotClient.DeleteAsync(Reference, CancelToken).ConfigureAwait(false)
            : await WebhookClient.DeleteAsync(Reference, CancelToken).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Remove-DiscordMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
