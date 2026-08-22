using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveContainer")]
[OutputType(typeof(TeamsAdaptiveContainer))]
public sealed class CmdletNewTeamsAdaptiveContainer : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Items { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Emphasis", "Good", "Warning", "Attention")]
    public string? Style { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Bleed { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("top", "center", "bottom")]
    public string? VerticalContentAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

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
    [ValidateSet("Action.Submit", "Action.OpenUrl", "Action.ToggleVisibility")]
    public string? SelectAction { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionId { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string[]? SelectActionTargetElement { get; set; }

    protected override void ProcessRecord() {
        var container = new TeamsAdaptiveContainer {
            Id = Id,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Style = Style,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Bleed = Bleed.IsPresent ? true : null,
            VerticalContentAlignment = VerticalContentAlignment,
            IsVisible = Hidden.IsPresent ? false : null,
            BackgroundImage = TeamsAdaptiveBackgroundImageSupport.Create(
                BackgroundUrl,
                BackgroundFillMode,
                BackgroundHorizontalAlignment,
                BackgroundVerticalAlignment),
            SelectAction = TeamsAdaptiveActionSupport.CreateSelectAction(
                SelectAction,
                SelectActionId,
                SelectActionUrl,
                SelectActionTitle,
                SelectActionTargetElement)
        };

        foreach (var item in Items) {
            if (item is not null) {
                container.Items.Add(item);
            }
        }

        WriteObject(container);
    }

}
