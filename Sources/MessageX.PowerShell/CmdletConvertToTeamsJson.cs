using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Renders a Teams Workflow message as JSON without sending it.</summary>
[Cmdlet(VerbsData.ConvertTo, "TeamsJson")]
[OutputType(typeof(string))]
public sealed class CmdletConvertToTeamsJson : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public TeamsMessageRequest InputObject { get; set; } = null!;

    protected override void ProcessRecord() => WriteObject(WebhookMessageRenderer.Render(InputObject));
}
