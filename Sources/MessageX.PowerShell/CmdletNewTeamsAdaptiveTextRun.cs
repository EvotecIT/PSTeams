using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveTextRun")]
[OutputType(typeof(TeamsAdaptiveTextRun))]
public sealed class CmdletNewTeamsAdaptiveTextRun : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? Color { get; set; }

    [Parameter(Mandatory = false)]
    public bool? Subtle { get; set; }

    [Parameter(Mandatory = false)]
    public string? Size { get; set; }

    [Parameter(Mandatory = false)]
    public string? Weight { get; set; }

    [Parameter(Mandatory = false)]
    public bool? Highlight { get; set; }

    [Parameter(Mandatory = false)]
    public bool? Italic { get; set; }

    [Parameter(Mandatory = false)]
    public bool? StrikeThrough { get; set; }

    [Parameter(Mandatory = false)]
    public string? FontType { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveTextRun {
            Text = Text,
            Color = Color,
            Subtle = Subtle,
            Size = Size,
            Weight = Weight,
            Highlight = Highlight,
            Italic = Italic,
            StrikeThrough = StrikeThrough,
            FontType = FontType
        });
    }
}
