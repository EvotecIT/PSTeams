using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveImage")]
[OutputType(typeof(TeamsAdaptiveImage))]
public sealed class CmdletNewTeamsAdaptiveImage : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Url { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? AltText { get; set; }

    [Parameter(Mandatory = false)]
    public string? Size { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveImage {
            Url = Url,
            AltText = AltText,
            Size = Size
        });
    }
}
