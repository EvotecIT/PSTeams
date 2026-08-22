using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive text block backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveTextBlock")]
[OutputType(typeof(TeamsAdaptiveTextBlock))]
public sealed class CmdletNewAdaptiveTextBlock : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Dark", "Light", "Good", "Warning", "Attention")]
    public string? Color { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Default", "Monospace")]
    public string? FontType { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Subtle { get; set; }

    [Parameter(Mandatory = false)]
    public int? MaximumLines { get; set; }

    [Alias("FontSize")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Small", "Default", "Medium", "Large", "ExtraLarge")]
    public string? Size { get; set; }

    [Alias("FontWeight")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Lighter", "Default", "Bolder")]
    public string? Weight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Highlight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Italic { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter StrikeThrough { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Wrap { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveTextBlock {
            Text = Text == string.Empty ? $"{(char)0x200F}" : Text ?? string.Empty,
            Id = Id,
            Spacing = Spacing,
            HorizontalAlignment = HorizontalAlignment,
            Size = Size,
            Weight = Weight,
            Color = Color,
            Height = Height,
            FontType = FontType,
            Highlight = Highlight.IsPresent ? true : null,
            Italic = Italic.IsPresent ? true : null,
            StrikeThrough = StrikeThrough.IsPresent ? true : null,
            MaximumLines = MaximumLines,
            Separator = Separator.IsPresent ? true : null,
            Wrap = Wrap.IsPresent ? true : null,
            Subtle = Subtle.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null
        });
    }
}
