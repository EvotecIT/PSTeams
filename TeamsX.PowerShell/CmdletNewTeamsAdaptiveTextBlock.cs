using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveTextBlock")]
[OutputType(typeof(TeamsAdaptiveTextBlock))]
public sealed class CmdletNewTeamsAdaptiveTextBlock : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public SwitchParameter NoWrap { get; set; }

    [Parameter(Mandatory = false)]
    public string? Size { get; set; }

    [Parameter(Mandatory = false)]
    public string? Weight { get; set; }

    [Parameter(Mandatory = false)]
    public string? Color { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveTextBlock {
            Text = Text,
            Wrap = !NoWrap,
            Size = Size,
            Weight = Weight,
            Color = Color
        });
    }
}
