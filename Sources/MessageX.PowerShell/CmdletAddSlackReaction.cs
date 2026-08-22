using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Adds the authenticated Slack application's reaction to a message.</summary>
/// <example>
/// <summary>Add an eyes reaction to an application-owned message</summary>
/// <code>$connection = New-SlackConnection -BotToken (Read-Host -AsSecureString); $target = New-SlackConversationTarget -ConversationId 'C0123456789'; $message = New-SlackMessage -Text 'Review ready'; $reference = (Send-SlackMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Add-SlackReaction -Reference $reference -Reaction 'eyes' -Connection $connection</code>
/// </example>
[Cmdlet(VerbsCommon.Add, "SlackReaction", SupportsShouldProcess = true)]
[OutputType(typeof(SlackDeliveryResult))]
public sealed class CmdletAddSlackReaction : SlackLifecycleCmdletBase {
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
        if (!ShouldProcess(Reference.ConversationId, $"Add Slack reaction {Reaction}")) {
            return;
        }
        var result = await LifecycleClient
            .AddReactionAsync(Reference, Reaction, CancelToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Add-SlackReaction"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
