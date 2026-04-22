using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveRichTextBlock")]
[OutputType(typeof(TeamsAdaptiveRichTextBlock))]
public sealed class CmdletNewTeamsAdaptiveRichTextBlock : PSCmdlet {
    [Parameter(Mandatory = true, ParameterSetName = "Text")]
    public string[] Text { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public string[] Color { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Subtle { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public string[] Size { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public string[] Weight { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Highlight { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] Italic { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public bool[] StrikeThrough { get; set; } = Array.Empty<bool>();

    [Parameter(Mandatory = false, ParameterSetName = "Text")]
    public string[] FontType { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = true, ParameterSetName = "Inline")]
    public TeamsAdaptiveTextRun[] Inlines { get; set; } = Array.Empty<TeamsAdaptiveTextRun>();

    protected override void ProcessRecord() {
        var block = new TeamsAdaptiveRichTextBlock();

        if (ParameterSetName == "Inline") {
            foreach (var inline in Inlines ?? Array.Empty<TeamsAdaptiveTextRun>()) {
                if (inline is not null) {
                    block.Inlines.Add(inline);
                }
            }

            WriteObject(block);
            return;
        }

        for (var i = 0; i < Text.Length; i++) {
            var run = new TeamsAdaptiveTextRun {
                Text = Text[i],
                Color = GetValue(Color, i),
                Subtle = GetBoolean(Subtle, i),
                Size = GetValue(Size, i),
                Weight = GetValue(Weight, i),
                Highlight = GetBoolean(Highlight, i),
                Italic = GetBoolean(Italic, i),
                StrikeThrough = GetBoolean(StrikeThrough, i),
                FontType = GetValue(FontType, i)
            };

            block.Inlines.Add(run);
        }

        WriteObject(block);
    }

    private static string? GetValue(string[] values, int index) {
        return values.Length > index ? values[index] : null;
    }

    private static bool? GetBoolean(bool[] values, int index) {
        return values.Length > index ? values[index] : null;
    }
}
