using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveActionSet")]
[OutputType(typeof(TeamsAdaptiveActionSet))]
public sealed class CmdletNewTeamsAdaptiveActionSet : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveAction[] Actions { get; set; } = Array.Empty<TeamsAdaptiveAction>();

    protected override void ProcessRecord() {
        var actionSet = new TeamsAdaptiveActionSet();

        foreach (var action in Actions) {
            if (action is not null) {
                actionSet.Actions.Add(action);
            }
        }

        WriteObject(actionSet);
    }
}
