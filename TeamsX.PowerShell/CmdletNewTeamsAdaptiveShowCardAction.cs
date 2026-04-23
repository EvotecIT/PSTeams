using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveShowCardAction")]
[OutputType(typeof(TeamsAdaptiveShowCardAction))]
public sealed class CmdletNewTeamsAdaptiveShowCardAction : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCard? Card { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Body { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveAction[] Actions { get; set; } = Array.Empty<TeamsAdaptiveAction>();

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveMention[] Mentions { get; set; } = Array.Empty<TeamsAdaptiveMention>();

    [Parameter(Mandatory = false)]
    public string Version { get; set; } = "1.2";

    [Parameter(Mandatory = false)]
    public string? FallbackText { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    public string? Speak { get; set; }

    [Parameter(Mandatory = false)]
    public string? Language { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("top", "center", "bottom")]
    public string? VerticalContentAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public string? BackgroundUrl { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Cover", "RepeatHorizontally", "RepeatVertically", "Repeat")]
    public string? BackgroundFillMode { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("left", "center", "right")]
    public string? BackgroundHorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("top", "center", "bottom")]
    public string? BackgroundVerticalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter FullWidth { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter AllowImageExpand { get; set; }

    protected override void ProcessRecord() {
        var nestedCard = Card ?? BuildCardFromParameters();

        WriteObject(new TeamsAdaptiveShowCardAction {
            Id = Id,
            Title = Title,
            Card = TeamsAdaptiveCardDictionarySupport.ToDictionary(nestedCard)
        });
    }

    private TeamsAdaptiveCard BuildCardFromParameters() {
        var card = new TeamsAdaptiveCard {
            Version = Version,
            FallbackText = FallbackText,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Speak = Speak,
            Language = Language,
            VerticalContentAlignment = VerticalContentAlignment,
            BackgroundImage = TeamsAdaptiveCardDictionarySupport.BuildBackgroundImage(
                BackgroundUrl,
                BackgroundFillMode,
                BackgroundHorizontalAlignment,
                BackgroundVerticalAlignment),
            AllowImageExpand = AllowImageExpand.IsPresent ? true : null,
            FullWidth = FullWidth.IsPresent
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

        return card;
    }
}
