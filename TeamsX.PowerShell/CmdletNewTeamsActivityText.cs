using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a typed activity-text directive for connector-card sections.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsActivityText")]
[Alias("ActivityText", "TeamsActivityText")]
[OutputType(typeof(TeamsMessageSectionDirective))]
public sealed class CmdletNewTeamsActivityText : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Text { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageSectionDirective {
            DirectiveType = TeamsMessageSectionDirectiveType.ActivityText,
            Value = Text
        });
    }
}
