using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveActionSet")]
[OutputType(typeof(TeamsAdaptiveActionSet))]
public sealed class CmdletNewTeamsAdaptiveActionSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Actions { get; set; }

    protected override void ProcessRecord() {
        var actionSet = new TeamsAdaptiveActionSet();

        foreach (var action in TeamsAdaptiveScriptBlock.Evaluate<TeamsAdaptiveOpenUrlAction>(Actions)) actionSet.Actions.Add(action);

        WriteObject(actionSet);
    }
}
