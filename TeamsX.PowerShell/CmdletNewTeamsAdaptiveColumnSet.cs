using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveColumnSet")]
[OutputType(typeof(TeamsAdaptiveColumnSet))]
public sealed class CmdletNewTeamsAdaptiveColumnSet : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveColumn[] Columns { get; set; } = Array.Empty<TeamsAdaptiveColumn>();

    protected override void ProcessRecord() {
        var columnSet = new TeamsAdaptiveColumnSet();

        foreach (var column in Columns) {
            if (column is not null) {
                columnSet.Columns.Add(column);
            }
        }

        WriteObject(columnSet);
    }
}
