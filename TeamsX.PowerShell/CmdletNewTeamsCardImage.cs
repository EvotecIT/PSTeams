using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates an image entry for HeroCard or ThumbnailCard content.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsCardImage")]
[OutputType(typeof(TeamsCardImage))]
public sealed class CmdletNewTeamsCardImage : PSCmdlet {
    [Alias("Link")]
    [Parameter(Mandatory = false, Position = 0)]
    public string? Url { get; set; }

    [Alias("AltText", "Alt")]
    [Parameter(Mandatory = false)]
    public string? AlternateText { get; set; }

    protected override void ProcessRecord() {
        if (string.IsNullOrWhiteSpace(Url)) {
            return;
        }

        WriteObject(new TeamsCardImage {
            Url = Url,
            Alt = AlternateText
        });
    }
}
