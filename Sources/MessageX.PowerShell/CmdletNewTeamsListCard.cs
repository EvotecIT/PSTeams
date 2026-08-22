using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsListCard")]
[OutputType(typeof(TeamsListCard))]
public sealed class CmdletNewTeamsListCard : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsListCardItem[] Items { get; set; } = Array.Empty<TeamsListCardItem>();

    [Parameter(Mandatory = false)]
    public TeamsCardButton[] Buttons { get; set; } = Array.Empty<TeamsCardButton>();

    protected override void ProcessRecord() {
        var card = new TeamsListCard {
            Title = Title
        };

        foreach (var item in Items ?? Array.Empty<TeamsListCardItem>()) {
            if (item is not null) {
                card.Items.Add(item);
            }
        }

        foreach (var button in Buttons ?? Array.Empty<TeamsCardButton>()) {
            if (button is not null) {
                card.Buttons.Add(button);
            }
        }

        WriteObject(card);
    }
}
