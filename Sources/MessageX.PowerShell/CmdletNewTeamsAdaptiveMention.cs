using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveMention")]
[OutputType(typeof(TeamsAdaptiveMention))]
public sealed class CmdletNewTeamsAdaptiveMention : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    [Parameter(Mandatory = true, Position = 1)]
    public string UserPrincipalName { get; set; } = string.Empty;

    [Parameter(Mandatory = false, Position = 2)]
    public string? Name { get; set; }

    protected override void ProcessRecord() {
        var mentionText = Text.IndexOf("<at>", StringComparison.OrdinalIgnoreCase) >= 0
            ? Text
            : $"<at>{Text}</at>";

        WriteObject(new TeamsAdaptiveMention {
            Text = mentionText,
            Mentioned = new TeamsMentionedIdentity {
                Id = UserPrincipalName,
                Name = Name
            }
        });
    }
}
