using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveColumn")]
[OutputType(typeof(TeamsAdaptiveColumn))]
public sealed class CmdletNewTeamsAdaptiveColumn : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Width { get; set; }

    [Parameter(Mandatory = false)]
    public int WidthInWeight { get; set; }

    [Parameter(Mandatory = false)]
    public int WidthInPixels { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Top", "Center", "Bottom")]
    public string? VerticalContentAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Emphasis", "Good", "Warning", "Attention")]
    public string? Style { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

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

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCardElement[] Items { get; set; } = Array.Empty<TeamsAdaptiveCardElement>();

    protected override void ProcessRecord() {
        var column = new TeamsAdaptiveColumn {
            Width = ResolveWidth(),
            Height = Height,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            HorizontalAlignment = HorizontalAlignment,
            VerticalContentAlignment = VerticalContentAlignment,
            Spacing = Spacing,
            Style = Style,
            IsVisible = Hidden.IsPresent ? false : null,
            Separator = Separator.IsPresent ? true : null,
            SelectAction = TeamsAdaptiveActionSupport.CreateSelectAction(
                SelectAction,
                SelectActionId,
                SelectActionUrl,
                SelectActionTitle,
                SelectActionTargetElement)
        };

        foreach (var item in Items) {
            if (item is not null) {
                column.Items.Add(item);
            }
        }

        WriteObject(column);
    }

    private string? ResolveWidth() {
        if (WidthInWeight > 0) {
            return WidthInWeight.ToString();
        }

        if (WidthInPixels > 0) {
            return $"{WidthInPixels}px";
        }

        return Width;
    }
}
