using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Deletes an application-owned Slack message.</summary>
[Cmdlet(VerbsCommon.Remove, "SlackMessage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
[OutputType(typeof(SlackDeliveryResult))]
public sealed class CmdletRemoveSlackMessage : SlackLifecycleCmdletBase {
    /// <summary>Durable Slack message reference returned by MessageX.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, "Delete Slack message")) {
            return;
        }
        var result = await LifecycleClient.DeleteAsync(Reference, CancelToken).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Remove-SlackMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
