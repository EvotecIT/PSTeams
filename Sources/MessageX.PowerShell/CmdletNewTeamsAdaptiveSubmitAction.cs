using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveSubmitAction")]
[OutputType(typeof(TeamsAdaptiveSubmitAction))]
public sealed class CmdletNewTeamsAdaptiveSubmitAction : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveSubmitAction {
            Id = Id,
            Title = Title
        });
    }
}
