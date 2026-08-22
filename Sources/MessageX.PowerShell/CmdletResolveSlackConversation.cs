using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Resolves or opens a Slack direct-message conversation for explicit user identifiers.</summary>
[Cmdlet(VerbsDiagnostic.Resolve, "SlackConversation", SupportsShouldProcess = true)]
[OutputType(typeof(MessageReference))]
public sealed class CmdletResolveSlackConversation : SlackLifecycleCmdletBase {
    /// <summary>One to eight explicit Slack user identifiers beginning with U or W.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateCount(1, 8)]
    public string[] UserId { get; set; } = Array.Empty<string>();

    /// <summary>Looks up only an existing conversation and never creates one.</summary>
    [Parameter]
    public SwitchParameter PreventCreation { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        var target = UserId.Length == 1
            ? $"Slack direct message to {UserId[0]}"
            : $"Slack multiparty direct message to {UserId.Length} users";
        if (!PreventCreation && !ShouldProcess(target, "Open Slack conversation")) {
            return;
        }
        var result = await ConversationDirectory
            .OpenDirectMessageAsync(UserId, PreventCreation, CancelToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Resolve-SlackConversation"));
            return;
        }
        WriteObject(result.Reference);
    }
}
