using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Updates an application-owned Slack message.</summary>
/// <example>
/// <summary>Replace an application-owned Slack message</summary>
/// <code>$connection = New-SlackConnection -BotToken (Read-Host -AsSecureString); $target = New-SlackConversationTarget -ConversationId 'C0123456789'; $original = New-SlackMessage -Text 'Deployment started'; $reference = (Send-SlackMessage -Message $original -Target $target -Connection $connection -PassThru).Reference; $replacement = New-SlackMessage -Text 'Deployment completed'; Update-SlackMessage -Message $replacement -Reference $reference -Connection $connection</code>
/// </example>
[Cmdlet(VerbsData.Update, "SlackMessage", SupportsShouldProcess = true)]
[OutputType(typeof(SlackDeliveryResult))]
public sealed class CmdletUpdateSlackMessage : SlackLifecycleCmdletBase {
    /// <summary>Replacement Slack message.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public SlackMessageRequest Message { get; set; } = null!;

    /// <summary>Durable Slack message reference returned by MessageX.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, "Update Slack message")) {
            return;
        }
        var result = await LifecycleClient.UpdateAsync(Message, Reference, CancelToken).ConfigureAwait(false);
        WriteResult(result, "Update-SlackMessage");
    }

    private void WriteResult(SlackDeliveryResult result, string commandName) {
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, commandName));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
