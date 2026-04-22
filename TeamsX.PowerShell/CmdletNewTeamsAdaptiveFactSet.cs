using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveFactSet")]
[OutputType(typeof(TeamsAdaptiveFactSet))]
public sealed class CmdletNewTeamsAdaptiveFactSet : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveFact[] Facts { get; set; } = Array.Empty<TeamsAdaptiveFact>();

    protected override void ProcessRecord() {
        var factSet = new TeamsAdaptiveFactSet();

        foreach (var fact in Facts) {
            if (fact is not null) {
                factSet.Facts.Add(fact);
            }
        }

        WriteObject(factSet);
    }
}
