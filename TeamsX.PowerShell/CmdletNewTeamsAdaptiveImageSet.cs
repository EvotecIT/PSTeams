using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveImageSet")]
[OutputType(typeof(TeamsAdaptiveImageSet))]
public sealed class CmdletNewTeamsAdaptiveImageSet : PSCmdlet {
    [Parameter(Mandatory = true)]
    public TeamsAdaptiveImage[] Images { get; set; } = Array.Empty<TeamsAdaptiveImage>();

    [Parameter(Mandatory = false)]
    public string? ImageSize { get; set; }

    protected override void ProcessRecord() {
        var imageSet = new TeamsAdaptiveImageSet {
            ImageSize = ImageSize
        };

        foreach (var image in Images ?? Array.Empty<TeamsAdaptiveImage>()) {
            if (image is not null) {
                imageSet.Images.Add(image);
            }
        }

        WriteObject(imageSet);
    }
}
