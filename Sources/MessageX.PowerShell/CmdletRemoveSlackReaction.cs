using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Removes the authenticated Slack application's reaction from a message.</summary>
/// <example>
/// <summary>Remove the application's eyes reaction from a message</summary>
/// <code>$connection = New-SlackConnection -BotToken (Read-Host -AsSecureString); $target = New-SlackConversationTarget -ConversationId 'C0123456789'; $message = New-SlackMessage -Text 'Review complete'; $reference = (Send-SlackMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Remove-SlackReaction -Reference $reference -Reaction 'eyes' -Connection $connection</code>
/// </example>
[Cmdlet(VerbsCommon.Remove, "SlackReaction", SupportsShouldProcess = true)]
[OutputType(typeof(SlackDeliveryResult))]
public sealed class CmdletRemoveSlackReaction : SlackLifecycleCmdletBase {
    /// <summary>Durable Slack message reference.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Slack reaction name without surrounding colons.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Reaction { get; set; } = string.Empty;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, $"Remove Slack reaction {Reaction}")) {
            return;
        }
        var result = await LifecycleClient
            .RemoveReactionAsync(Reference, Reaction, CancelToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Remove-SlackReaction"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
