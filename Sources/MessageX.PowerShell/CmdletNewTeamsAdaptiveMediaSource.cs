using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveMediaSource")]
[OutputType(typeof(TeamsAdaptiveMediaSource))]
public sealed class CmdletNewTeamsAdaptiveMediaSource : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Type { get; set; } = string.Empty;

    [Parameter(Mandatory = true, Position = 1)]
    public string Url { get; set; } = string.Empty;

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveMediaSource {
            MimeType = Type,
            Url = Url
        });
    }
}
