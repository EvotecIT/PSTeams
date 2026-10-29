using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Composes a Teams Workflow Adaptive Card from nested PowerShell commands.</summary>
/// <example><summary>Create a card with a report link</summary><code>New-TeamsAdaptiveCard -Body { New-TeamsAdaptiveTextBlock -Text 'Build failed' } -Actions { New-TeamsAdaptiveOpenUrlAction -Title 'Open report' -Url 'https://example.test/report' } -FallbackText 'Build failed. Open https://example.test/report'</code></example>
[Cmdlet(VerbsCommon.New, "TeamsAdaptiveCard")]
[OutputType(typeof(TeamsAdaptiveCard))]
public sealed class CmdletNewTeamsAdaptiveCard : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Body { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? Actions { get; set; }

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
    public string? SelectActionId { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionTitle { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter FullWidth { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter AllowImageExpand { get; set; }

    protected override void ProcessRecord() {
        var card = new TeamsAdaptiveCard {
            Version = Version,
            FallbackText = FallbackText,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Speak = Speak,
            Language = Language,
            VerticalContentAlignment = VerticalContentAlignment,
            BackgroundImage = TeamsAdaptiveBackgroundImageSupport.Create(
                BackgroundUrl,
                BackgroundFillMode,
                BackgroundHorizontalAlignment,
                BackgroundVerticalAlignment),
            SelectAction = TeamsAdaptiveActionSupport.CreateSelectAction(
                SelectActionId,
                SelectActionUrl,
                SelectActionTitle),
            AllowImageExpand = AllowImageExpand.IsPresent ? true : null,
            FullWidth = FullWidth.IsPresent
        };

        foreach (var element in TeamsAdaptiveScriptBlock.Evaluate<TeamsAdaptiveCardElement>(Body)) card.Body.Add(element);

        foreach (var action in TeamsAdaptiveScriptBlock.Evaluate<TeamsAdaptiveOpenUrlAction>(Actions)) card.Actions.Add(action);

        foreach (var mention in Mentions ?? Array.Empty<TeamsAdaptiveMention>()) {
            if (mention is not null) {
                card.Mentions.Add(mention);
            }
        }

        WriteObject(card);
    }

}
