using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a typed legacy list item for connector-card facts.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsListItem")]
[Alias("TeamsListItem")]
[OutputType(typeof(TeamsMessageListItem))]
public sealed class CmdletNewTeamsListItem : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public int Level { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Numbered { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageListItem {
            Text = Text,
            Level = Level,
            Numbered = Numbered.IsPresent
        });
    }
}
