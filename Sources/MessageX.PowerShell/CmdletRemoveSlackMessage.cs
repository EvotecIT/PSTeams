using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Deletes an application-owned Slack message.</summary>
/// <example>
/// <summary>Delete an application-owned Slack message</summary>
/// <code>$connection = New-SlackConnection -BotToken (Read-Host -AsSecureString); $target = New-SlackConversationTarget -ConversationId 'C0123456789'; $message = New-SlackMessage -Text 'Temporary notice'; $reference = (Send-SlackMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Remove-SlackMessage -Reference $reference -Connection $connection</code>
/// </example>
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
