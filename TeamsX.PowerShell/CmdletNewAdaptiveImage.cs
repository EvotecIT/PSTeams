using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive image backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveImage")]
[OutputType(typeof(TeamsAdaptiveImage))]
public sealed class CmdletNewAdaptiveImage : PSCmdlet {
    [Alias("Link")]
    [Parameter(Mandatory = false, Position = 0)]
    public string? Url { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("person", "default")]
    public string? Style { get; set; }

    [Alias("Alt", "AltText")]
    [Parameter(Mandatory = false)]
    public string? AlternateText { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Auto", "Stretch", "Small", "Medium", "Large")]
    public string? Size { get; set; }

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
    public int HeightInPixels { get; set; }

    [Parameter(Mandatory = false)]
    public int WidthInPixels { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    [Parameter(Mandatory = false)]
    public string? BackgroundColor { get; set; }

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
        var image = new TeamsAdaptiveImage {
            Id = Id,
            Url = Url ?? string.Empty,
            Size = Size,
            AltText = AlternateText,
            Style = Style,
            HorizontalAlignment = HorizontalAlignment,
            Height = HeightInPixels > 0 ? $"{HeightInPixels}px" : Height,
            Width = WidthInPixels > 0 ? $"{WidthInPixels}px" : null,
            Spacing = Spacing,
            BackgroundColor = TeamsColorUtility.NormalizeToHex(BackgroundColor),
            Separator = Separator.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null,
            SelectAction = TeamsAdaptiveActionSupport.CreateSelectAction(
                SelectAction,
                SelectActionId,
                SelectActionUrl,
                SelectActionTitle,
                SelectActionTargetElement)
        };

        WriteObject(image);
    }
}
