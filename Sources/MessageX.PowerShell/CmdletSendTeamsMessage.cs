using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Sends an Adaptive Card through a Teams Workflow webhook.</summary>
/// <example>
/// <summary>Send a Workflow notification</summary>
/// <code>$target = New-TeamsWebhookTarget -Uri $workflowUrl -Destination Channel; $card = New-TeamsAdaptiveCard -Body { New-TeamsAdaptiveTextBlock -Text 'Build failed' }; $message = New-TeamsMessage -AdaptiveCard $card -Summary 'Build failed'; Send-TeamsMessage -Message $message -Target $target</code>
/// </example>
[Cmdlet(VerbsCommunications.Send, "TeamsMessage", SupportsShouldProcess = true)]
[OutputType(typeof(TeamsDeliveryResult))]
public sealed class CmdletSendTeamsMessage : TeamsWebhookCmdletBase {
    [Parameter(Mandatory = true, Position = 0)]
    public TeamsMessageRequest Message { get; set; } = null!;
    [Parameter(Mandatory = true, Position = 1)]
    public TeamsMessageTarget Target { get; set; } = null!;
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Target.ToString(), "Send Teams Workflow message")) {
            return;
        }
        var result = await SendWithClientAsync(client => client.SendAsync(Message, Target, CancelToken));
        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-TeamsMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
