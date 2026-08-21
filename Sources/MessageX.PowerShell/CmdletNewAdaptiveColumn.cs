using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive column backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveColumn")]
[OutputType(typeof(TeamsAdaptiveColumn))]
public sealed class CmdletNewAdaptiveColumn : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Items { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Auto", "Weighted")]
    public string? Width { get; set; }

    [Parameter(Mandatory = false)]
    public int WidthInWeight { get; set; }

    [Parameter(Mandatory = false)]
    public int WidthInPixels { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Top", "Center", "Bottom")]
    public string? VerticalContentAlignment { get; set; }

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

    protected override void ProcessRecord() {
        if (Items is null) {
            return;
        }

        var column = new TeamsAdaptiveColumn {
            Width = GetWidthValue(),
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

        foreach (var item in Items.Invoke()) {
            var value = item is PSObject psObject ? psObject.BaseObject : item;
            if (value is TeamsAdaptiveCardElement element) {
                column.Items.Add(element);
            }
        }

        if (column.Items.Count > 0) {
            WriteObject(column);
        }
    }

    private string? GetWidthValue() {
        if (WidthInWeight > 0) {
            return WidthInWeight.ToString();
        }

        if (WidthInPixels > 0) {
            return $"{WidthInPixels}px";
        }

        var width = Width;
        if (string.IsNullOrWhiteSpace(width)) {
            return null;
        }

        return width!.ToLowerInvariant();
    }
}
