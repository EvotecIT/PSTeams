using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Creates a Teams Workflow message with an Adaptive Card.</summary>
[Cmdlet(VerbsCommon.New, "TeamsMessage")]
[OutputType(typeof(TeamsMessageRequest))]
public sealed class CmdletNewTeamsMessage : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public TeamsAdaptiveCard AdaptiveCard { get; set; } = null!;
    [Parameter]
    public string? Summary { get; set; }

    protected override void ProcessRecord() =>
        WriteObject(new TeamsMessageRequest { AdaptiveCard = AdaptiveCard, Summary = Summary });
}
