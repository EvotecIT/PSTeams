using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveColumn")]
[OutputType(typeof(TeamsAdaptiveColumn))]
public sealed class CmdletNewTeamsAdaptiveColumn : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Width { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Items { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    protected override void ProcessRecord() {
        var column = new TeamsAdaptiveColumn {
            Width = Width
        };

        foreach (var item in Items) {
            if (item is not null) {
                column.Items.Add(item);
            }
        }

        WriteObject(column);
    }
}
