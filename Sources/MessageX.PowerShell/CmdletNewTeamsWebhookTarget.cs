using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsWebhookTarget")]
[OutputType(typeof(TeamsMessageTarget))]
public sealed class CmdletNewTeamsWebhookTarget : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Workflow { get; set; }

    protected override void ProcessRecord() {
        var target = Workflow
            ? TeamsMessageTarget.ForWorkflowWebhook(Uri, DisplayName)
            : TeamsMessageTarget.ForIncomingWebhook(Uri, DisplayName);

        WriteObject(target);
    }
}
