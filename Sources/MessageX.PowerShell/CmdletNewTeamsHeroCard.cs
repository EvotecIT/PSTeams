using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsHeroCard")]
[OutputType(typeof(TeamsHeroCard))]
public sealed class CmdletNewTeamsHeroCard : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? SubTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsCardImage[] Images { get; set; } = Array.Empty<TeamsCardImage>();

    [Parameter(Mandatory = false)]
    public TeamsCardButton[] Buttons { get; set; } = Array.Empty<TeamsCardButton>();

    protected override void ProcessRecord() {
        var card = new TeamsHeroCard {
            Title = Title,
            SubTitle = SubTitle,
            Text = Text
        };

        foreach (var image in Images ?? Array.Empty<TeamsCardImage>()) {
            if (image is not null) {
                card.Images.Add(image);
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
