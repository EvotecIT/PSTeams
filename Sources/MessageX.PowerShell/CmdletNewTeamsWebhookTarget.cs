using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a send-only Teams incoming webhook or Power Automate Workflow target.
/// </summary>
/// <remarks>
/// Workflow destination metadata documents where the configured flow delivers messages. It does not add reply, update, delete, or conversation capabilities.
/// </remarks>
/// <example>
/// <summary>Create a channel Workflow target</summary>
/// <code>New-TeamsWebhookTarget -Uri $workflowUrl -Workflow -Destination Channel -DisplayName 'Release alerts'</code>
/// </example>
/// <example>
/// <summary>Create a legacy incoming webhook target</summary>
/// <code>New-TeamsWebhookTarget -Uri $incomingWebhookUrl -DisplayName 'Legacy alerts'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "TeamsWebhookTarget", DefaultParameterSetName = "IncomingWebhook")]
[OutputType(typeof(TeamsMessageTarget))]
public sealed class CmdletNewTeamsWebhookTarget : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "WorkflowWebhook")]
    public SwitchParameter Workflow { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "WorkflowWebhook")]
    public TeamsWorkflowDestinationKind Destination { get; set; }

    protected override void ProcessRecord() {
        var target = Workflow
            ? TeamsMessageTarget.ForWorkflowWebhook(Uri, DisplayName, Destination)
            : TeamsMessageTarget.ForIncomingWebhook(Uri, DisplayName);

        WriteObject(target);
    }
}
