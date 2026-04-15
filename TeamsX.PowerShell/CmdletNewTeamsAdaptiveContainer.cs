using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveContainer")]
[OutputType(typeof(TeamsAdaptiveContainer))]
public sealed class CmdletNewTeamsAdaptiveContainer : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Items { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    protected override void ProcessRecord() {
        var container = new TeamsAdaptiveContainer();

        foreach (var item in Items) {
            if (item is not null) {
                container.Items.Add(item);
            }
        }

        WriteObject(container);
    }
}
