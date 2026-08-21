using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a typed activity-subtitle directive for connector-card sections.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsActivitySubtitle")]
[Alias("ActivitySubtitle", "TeamsActivitySubtitle")]
[OutputType(typeof(TeamsMessageSectionDirective))]
public sealed class CmdletNewTeamsActivitySubtitle : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Subtitle { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageSectionDirective {
            DirectiveType = TeamsMessageSectionDirectiveType.ActivitySubtitle,
            Value = Subtitle
        });
    }
}
