using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack channel, direct-message, multiparty-message, or user target.</summary>
[Cmdlet(VerbsCommon.New, "SlackConversationTarget")]
[OutputType(typeof(SlackMessageTarget))]
public sealed class CmdletNewSlackConversationTarget : PSCmdlet {
    /// <summary>Slack provider identifier, such as a channel, conversation, or user ID.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(SlackMessageTarget.ForConversation(ConversationId, DisplayName));
    }
}
