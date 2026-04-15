using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsMessage")]
[OutputType(typeof(TeamsMessageRequest))]
public sealed class CmdletNewTeamsMessage : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    public string? Summary { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCard? AdaptiveCard { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageRequest {
            Title = Title,
            Text = Text,
            Summary = Summary,
            AdaptiveCard = AdaptiveCard
        });
    }
}
