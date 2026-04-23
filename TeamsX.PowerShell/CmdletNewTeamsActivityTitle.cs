using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a typed activity-title directive for connector-card sections.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsActivityTitle")]
[Alias("ActivityTitle", "TeamsActivityTitle")]
[OutputType(typeof(TeamsMessageSectionDirective))]
public sealed class CmdletNewTeamsActivityTitle : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Title { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageSectionDirective {
            DirectiveType = TeamsMessageSectionDirectiveType.ActivityTitle,
            Value = Title
        });
    }
}
