using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveImageSet")]
[OutputType(typeof(TeamsAdaptiveImageSet))]
public sealed class CmdletNewTeamsAdaptiveImageSet : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock Images { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? ImageSize { get; set; }

    protected override void ProcessRecord() {
        var imageSet = new TeamsAdaptiveImageSet {
            ImageSize = ImageSize
        };

        foreach (var image in TeamsAdaptiveScriptBlock.Evaluate<TeamsAdaptiveImage>(Images)) imageSet.Images.Add(image);

        WriteObject(imageSet);
    }
}
