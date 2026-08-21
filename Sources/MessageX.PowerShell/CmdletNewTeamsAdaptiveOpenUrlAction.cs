using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveOpenUrlAction")]
[OutputType(typeof(TeamsAdaptiveOpenUrlAction))]
public sealed class CmdletNewTeamsAdaptiveOpenUrlAction : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = true, Position = 1)]
    public string Url { get; set; } = string.Empty;

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveOpenUrlAction {
            Title = Title,
            Url = Url
        });
    }
}
