using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Creates a send-only Teams Workflow webhook target.</summary>
/// <example><summary>Create a channel destination</summary><code>New-TeamsWebhookTarget -Uri $workflowUrl -Destination Channel -DisplayName 'Release alerts'</code></example>
[Cmdlet(VerbsCommon.New, "TeamsWebhookTarget")]
[OutputType(typeof(TeamsMessageTarget))]
public sealed class CmdletNewTeamsWebhookTarget : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;
    [Parameter]
    public string? DisplayName { get; set; }
    [Parameter]
    public TeamsWorkflowDestinationKind Destination { get; set; }

    protected override void ProcessRecord() =>
        WriteObject(TeamsMessageTarget.ForWorkflowWebhook(Uri, DisplayName, Destination));
}
