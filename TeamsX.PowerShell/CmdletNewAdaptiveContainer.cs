using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive container backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveContainer")]
[OutputType(typeof(TeamsAdaptiveContainer))]
public sealed class CmdletNewAdaptiveContainer : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Items { get; set; }

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
        if (Items is null) {
            return;
        }

        var container = new TeamsAdaptiveContainer {
            Id = Id,
            Spacing = Spacing,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Style = Style,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Bleed = Bleed.IsPresent ? true : null,
            VerticalContentAlignment = VerticalContentAlignment,
            Separator = Separator.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null,
            BackgroundImage = BuildBackgroundImage(),
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
                container.Items.Add(element);
            }
        }

        if (container.Items.Count > 0) {
            WriteObject(container);
        }
    }

    private Dictionary<string, object?>? BuildBackgroundImage() {
        if (string.IsNullOrWhiteSpace(BackgroundUrl) &&
            string.IsNullOrWhiteSpace(BackgroundFillMode) &&
            string.IsNullOrWhiteSpace(BackgroundHorizontalAlignment) &&
            string.IsNullOrWhiteSpace(BackgroundVerticalAlignment)) {
            return null;
        }

        var backgroundImage = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(BackgroundFillMode)) {
            backgroundImage["fillMode"] = BackgroundFillMode;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundHorizontalAlignment)) {
            backgroundImage["horizontalAlignment"] = BackgroundHorizontalAlignment;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundVerticalAlignment)) {
            backgroundImage["verticalAlignment"] = BackgroundVerticalAlignment;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundUrl)) {
            backgroundImage["url"] = BackgroundUrl;
        }

        return backgroundImage;
    }
}
