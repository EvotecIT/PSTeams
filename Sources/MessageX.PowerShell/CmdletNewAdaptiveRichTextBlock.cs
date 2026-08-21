using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive rich text block backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveRichTextBlock", DefaultParameterSetName = "Text")]
[OutputType(typeof(TeamsAdaptiveRichTextBlock))]
public sealed class CmdletNewAdaptiveRichTextBlock : PSCmdlet {
    [Parameter(Mandatory = true, ParameterSetName = "Text")]
    public string[] Text { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    [ValidateSet("Accent", "Default", "Dark", "Light", "Good", "Warning", "Attention")]
    public string[] Color { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Subtle { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    [Alias("FontSize")]
    [ValidateSet("Small", "Default", "Medium", "Large", "ExtraLarge")]
    public string[] Size { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    [Alias("FontWeight")]
    [ValidateSet("Lighter", "Default", "Bolder")]
    public string[] Weight { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Highlight { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Italic { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] StrikeThrough { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    [ValidateSet("Default", "Monospace")]
    public string[] FontType { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = true, ParameterSetName = "Inline")]
    public TeamsAdaptiveTextRun[] Inlines { get; set; } = Array.Empty<TeamsAdaptiveTextRun>();

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
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    protected override void ProcessRecord() {
        var block = new TeamsAdaptiveRichTextBlock {
            Id = Id,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null
        };

        if (ParameterSetName == "Inline") {
            foreach (var inline in Inlines ?? Array.Empty<TeamsAdaptiveTextRun>()) {
                if (inline is not null) {
                    block.Inlines.Add(inline);
                }
            }

            if (block.Inlines.Count > 0) {
                WriteObject(block);
            }

            return;
        }

        for (var i = 0; i < Text.Length; i++) {
            block.Inlines.Add(new TeamsAdaptiveTextRun {
                Text = Text[i],
                Color = GetValue(Color, i),
                Subtle = GetBoolean(Subtle, i),
                Size = GetValue(Size, i),
                Weight = GetValue(Weight, i),
                Highlight = GetBoolean(Highlight, i),
                Italic = GetBoolean(Italic, i),
                StrikeThrough = GetBoolean(StrikeThrough, i),
                FontType = GetValue(FontType, i)
            });
        }

        if (block.Inlines.Count > 0) {
            WriteObject(block);
        }
    }

    private static string? GetValue(string[] values, int index) {
        return values.Length > index ? values[index] : null;
    }

    private static bool? GetBoolean(bool[] values, int index) {
        return values.Length > index ? values[index] : null;
    }
}
