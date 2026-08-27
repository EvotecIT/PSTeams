using System.Management.Automation;
using System.Collections;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveSubmitAction")]
[OutputType(typeof(TeamsAdaptiveSubmitAction))]
public sealed class CmdletNewTeamsAdaptiveSubmitAction : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public IDictionary? Data { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveSubmitAction {
            Id = Id,
            Title = Title,
            Data = PowerShellMessageDataValueConverter.FromDictionary(Data)
        });
    }
}
