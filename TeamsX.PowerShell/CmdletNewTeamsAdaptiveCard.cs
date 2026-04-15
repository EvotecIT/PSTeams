using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveCard")]
[OutputType(typeof(TeamsAdaptiveCard))]
public sealed class CmdletNewTeamsAdaptiveCard : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Body { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveAction[] Actions { get; set; } = Array.Empty<TeamsAdaptiveAction>();

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveMention[] Mentions { get; set; } = Array.Empty<TeamsAdaptiveMention>();

    [Parameter(Mandatory = false)]
    public string Version { get; set; } = "1.2";

    protected override void ProcessRecord() {
        var card = new TeamsAdaptiveCard {
            Version = Version
        };

        foreach (var element in Body ?? Array.Empty<TeamsAdaptiveCardElement>()) {
            if (element is not null) {
                card.Body.Add(element);
            }
        }

        foreach (var action in Actions ?? Array.Empty<TeamsAdaptiveAction>()) {
            if (action is not null) {
                card.Actions.Add(action);
            }
        }

        foreach (var mention in Mentions ?? Array.Empty<TeamsAdaptiveMention>()) {
            if (mention is not null) {
                card.Mentions.Add(mention);
            }
        }

        WriteObject(card);
    }
}
