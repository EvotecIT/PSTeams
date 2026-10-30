using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Groups label/value facts in a Teams Adaptive Card.</summary>
/// <example><summary>Add incident facts</summary><code>New-TeamsAdaptiveFactSet -Facts { New-TeamsAdaptiveFact -Title 'Incident ID' -Value 'inc-42' }</code></example>
[Cmdlet(VerbsCommon.New, "TeamsAdaptiveFactSet")]
[OutputType(typeof(TeamsAdaptiveFactSet))]
public sealed class CmdletNewTeamsAdaptiveFactSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Facts { get; set; }

    protected override void ProcessRecord() {
        var factSet = new TeamsAdaptiveFactSet();

        foreach (var fact in TeamsAdaptiveScriptBlock.Evaluate<TeamsAdaptiveFact>(Facts)) factSet.Facts.Add(fact);

        WriteObject(factSet);
    }
}
