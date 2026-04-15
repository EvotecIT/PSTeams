using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveToggleVisibilityAction")]
[OutputType(typeof(TeamsAdaptiveToggleVisibilityAction))]
public sealed class CmdletNewTeamsAdaptiveToggleVisibilityAction : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = true, Position = 1)]
    public string[] TargetElementIds { get; set; } = Array.Empty<string>();

    protected override void ProcessRecord() {
        var action = new TeamsAdaptiveToggleVisibilityAction {
            Title = Title
        };

        foreach (var targetElementId in TargetElementIds ?? Array.Empty<string>()) {
            if (!string.IsNullOrWhiteSpace(targetElementId)) {
                action.TargetElements.Add(targetElementId);
            }
        }

        WriteObject(action);
    }
}
